import os
import time
import subprocess
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import pandas as pd
import numpy as np
from tensorflow.keras.applications.inception_v3 import InceptionV3, preprocess_input
from tensorflow.keras.preprocessing import image
from tensorflow.keras.models import Model
import tensorflow as tf
import tkinter as tk
from tkinter import messagebox
import threading
import queue
import csv

class FingerprintHandler(FileSystemEventHandler):
    def __init__(self):
        super().__init__()
        self.fingerprints = []
        self.processed_files = set()
        self.model = self.setup_model()
        self.person_info = {}
        self.information_csv_path = 'information.csv'
        self.fp_database_csv_path = 'fp_database.csv'
        self.info_queue = queue.Queue()
        print("FingerprintHandler initialized")

    def setup_model(self):
        print("Setting up InceptionV3 model...")
        base_model = InceptionV3(weights='imagenet', include_top=False, pooling='avg')
        model = Model(inputs=base_model.input, outputs=base_model.output)
        print("Model setup complete")
        return model

    def on_created(self, event):
        print(f"File created: {event.src_path}")
        if event.is_directory:
            return
        if event.src_path.endswith('.jpg'):
            print(f"New fingerprint image detected: {event.src_path}")
            if event.src_path not in self.processed_files:
                self.fingerprints.append(event.src_path)
                self.processed_files.add(event.src_path)
                
                print(f"Current number of fingerprints: {len(self.fingerprints)}")
                if len(self.fingerprints) == 20:
                    print("All 20 fingerprints collected. Starting processing...")
                    threading.Thread(target=self.collect_person_info).start()

                    
    def collect_person_info(self):
        """Thu thập thông tin cá nhân"""
        print("\nCollecting person information...")
        try:
            def show_dialog():
                print("Opening information input dialog...")
                root = tk.Tk()
                root.title("Enter Person Information")
                root.geometry("400x300")

                # Name
                tk.Label(root, text="Name:").pack(pady=5)
                name_entry = tk.Entry(root)
                name_entry.pack()

                # Class
                tk.Label(root, text="Class:").pack(pady=5)
                class_entry = tk.Entry(root)
                class_entry.pack()

                # Birthday
                tk.Label(root, text="Birthday (DD/MM/YYYY):").pack(pady=5)
                birthday_entry = tk.Entry(root)
                birthday_entry.pack()

                # Address
                tk.Label(root, text="Address:").pack(pady=5)
                address_entry = tk.Entry(root)
                address_entry.pack()

                def save_info():
                    info = {
                        'name': name_entry.get(),
                        'class': class_entry.get(),
                        'birthday': birthday_entry.get(),
                        'address': address_entry.get()
                    }
                    # Kiểm tra thông tin trước khi lưu
                    if all(info.values()):
                        print("Saving entered information...")
                        self.info_queue.put(info)
                        print("Information put in queue successfully")
                        root.quit()  # Chỉ kết thúc vòng lặp sự kiện
                    else:
                        print("Warning: Some fields are empty")
                        messagebox.showwarning("Warning", "Please fill in all fields")

                tk.Button(root, text="Save", command=save_info).pack(pady=20)
                print("Dialog setup complete")
                root.mainloop()  # Vòng lặp chính của hộp thoại

                # Đảm bảo root được phá hủy sau khi thoát khỏi vòng lặp sự kiện
                root.destroy()

            # Chạy dialog trong main thread
            print("Initializing main dialog window...")
            show_dialog()

            # Lấy thông tin từ queue
            if not self.info_queue.empty():
                self.person_info = self.info_queue.get()
                print("\nPerson information collected successfully:")
                print("----------------------------------------")
                print(f"Name    : {self.person_info['name']}")
                print(f"Class   : {self.person_info['class']}")
                print(f"Birthday: {self.person_info['birthday']}")
                print(f"Address : {self.person_info['address']}")
                print("----------------------------------------")

                # Validate thông tin
                if self.validate_information():
                    print("\nStarting fingerprint processing...")
                    print(f"Fingerprint count: {len(self.fingerprints)}")
                    self.process_fingerprints()
                else:
                    print("Information validation failed")
                    return
            else:
                print("Information collection cancelled by user")
                return

        except Exception as e:
            print(f"Error collecting person information: {str(e)}")
            import traceback
            print(traceback.format_exc())


    def validate_information(self):
        """Kiểm tra tính hợp lệ của thông tin"""
        try:
            from datetime import datetime
            try:
                datetime.strptime(self.person_info['birthday'], '%d/%m/%Y')
            except ValueError:
                print("Invalid birthday format. Please use DD/MM/YYYY")
                return False

            if len(self.person_info['name']) < 2:
                print("Name is too short")
                return False

            if not self.person_info['class']:
                print("Class information is required")
                return False

            if len(self.person_info['address']) < 5:
                print("Address is too short")
                return False

            return True

        except Exception as e:
            print(f"Error validating information: {str(e)}")
            return False

    def process_fingerprints(self):
        """Xử lý các ảnh vân tay đã thu thập"""
        print("\nProcessing fingerprints...")
        try:
            if not self.fingerprints:
                print("Error: No fingerprints found to process")
                return

            print(f"Found {len(self.fingerprints)} fingerprints to process")
            
            feature_vectors = []
            for i, fp_path in enumerate(self.fingerprints):
                print(f"Processing fingerprint {i+1}/{len(self.fingerprints)}: {fp_path}")
                
                if not os.path.exists(fp_path):
                    print(f"Error: File not found - {fp_path}")
                    continue
                    
                try:
                    print(f"Loading and preprocessing image {i+1}...")
                    img = image.load_img(fp_path, target_size=(299, 299))
                    x = image.img_to_array(img)
                    x = np.expand_dims(x, axis=0)
                    x = preprocess_input(x)
                    
                    print(f"Extracting features for image {i+1}...")
                    features = self.model.predict(x, verbose=0)
                    feature_vectors.append(features.flatten())
                    print(f"Successfully processed image {i+1}")
                    
                except Exception as img_error:
                    print(f"Error processing image {fp_path}: {str(img_error)}")
                    continue
                
            if not feature_vectors:
                print("Error: No feature vectors were generated")
                return
                
            print("Saving data to CSV...")
            self.save_to_csv(feature_vectors)
            
            print("Fingerprint processing completed successfully")
            
            print("Cleaning up temporary files...")
            self.cleanup_temp_files()
            
        except Exception as e:
            print(f"Error in process_fingerprints: {str(e)}")
            import traceback
            print(traceback.format_exc())

    # def save_to_csv(self, feature_vectors):
    #     """Lưu thông tin vào file CSV"""
    #     try:
    #         # Lấy label tiếp theo từ file info.csv
    #         next_label = self.get_next_label()
            
    #         with open(self.info_csv_path, 'a', newline='\n', encoding='utf-8') as file:
    #             writer = csv.writer(file)
    #             writer.writerow([next_label, self.person_info['name'], self.person_info['class'], 
    #                             self.person_info['birthday'], self.person_info['address']])
    #         print(f"Personal information saved to {self.info_csv_path}")

    #         with open(self.db_fp2_csv_path, 'a', newline='\n', encoding='utf-8') as file:
    #             writer = csv.writer(file)
    #             for feature_vector in feature_vectors:
    #                 writer.writerow(feature_vector.tolist() + [next_label])
    #         print(f"Feature vectors saved to {self.db_fp2_csv_path}")

    #     except Exception as e:
    #         print(f"Error saving to CSV: {str(e)}")
            
    # def save_to_csv(self, feature_vectors):
    #     """Lưu thông tin vào file CSV"""
    #     try:
    #         # Lấy label tiếp theo từ file info.csv
    #         next_label = self.get_next_label()
            
    #         # Kiểm tra xem file có tồn tại không
    #         file_exists = os.path.exists(self.info_csv_path)
            
    #         with open(self.info_csv_path, 'a', newline='', encoding='utf-8') as file:
    #             writer = csv.writer(file)
    #             # Nếu file không tồn tại, ghi header
    #             if not file_exists:
    #                 writer.writerow(['label', 'name', 'class', 'birth', 'address'])
                
    #             # Thêm một dòng mới trước khi ghi dữ liệu nếu file không trống
    #             if file_exists:
    #                 file.seek(0, 2)  # Di chuyển con trỏ đến cuối file
    #                 if file.tell() > 0:  # Kiểm tra xem file có trống không
    #                     file.seek(file.tell() - 1)  # Di chuyển về ký tự cuối cùng
    #                     last_char = file.read(1)
    #                     if last_char != '\n':
    #                         file.write('\n')
                
    #             # Ghi dữ liệu mới
    #             writer.writerow([
    #                 next_label,
    #                 self.person_info['name'],
    #                 self.person_info['class'],
    #                 self.person_info['birthday'],
    #                 self.person_info['address']
    #             ])
    #         print(f"Personal information saved to {self.info_csv_path}")

    #         # Lưu feature vectors
    #         with open(self.db_fp2_csv_path, 'a', newline='', encoding='utf-8') as file:
    #             writer = csv.writer(file)
    #             for feature_vector in feature_vectors:
    #                 writer.writerow(feature_vector.tolist() + [next_label])
    #         print(f"Feature vectors saved to {self.db_fp2_csv_path}")

    #     except Exception as e:
    #         print(f"Error saving to CSV: {str(e)}")
            
    def check_file_permissions(self, filepath):
        """Kiểm tra quyền truy cập file"""
        try:
            # Kiểm tra xem file có tồn tại không
            if os.path.exists(filepath):
                # Thử mở file để đọc và ghi
                with open(filepath, 'a+', encoding='utf-8') as f:
                    f.seek(0)
                    f.tell()
                return True
            else:
                # Thử tạo file mới
                with open(filepath, 'w', encoding='utf-8') as f:
                    pass
                return True
        except Exception as e:
            print(f"File permission error for {filepath}: {str(e)}")
            return False
            
    def save_to_csv(self, feature_vectors):
        """Lưu thông tin vào file CSV"""
        try:
            # Lấy label tiếp theo từ file info.csv
            next_label = self.get_next_label()
            print(f"Next label: {next_label}")

            # Lưu thông tin cá nhân
            try:
                # Kiểm tra xem file có tồn tại không
                file_exists = os.path.exists(self.information_csv_path)
                
                with open(self.information_csv_path, 'a', newline='', encoding='utf-8') as file:
                    writer = csv.writer(file)
                    # Nếu file không tồn tại, ghi header
                    if not file_exists:
                        writer.writerow(['label', 'name', 'class', 'birth', 'address'])
                    
                    # Ghi dữ liệu mới
                    row_data = [
                        next_label,
                        self.person_info['name'],
                        self.person_info['class'],
                        self.person_info['birthday'],
                        self.person_info['address']
                    ]
                    writer.writerow(row_data)
                    print(f"Written row to info.csv: {row_data}")
                print(f"Personal information saved to {self.information_csv_path}")
            except Exception as info_error:
                print(f"Error saving personal information: {str(info_error)}")
                raise

            # Lưu feature vectors
            try:
                print(f"Preparing to save {len(feature_vectors)} feature vectors...")
                with open(self.fp_database_csv_path, 'a', newline='', encoding='utf-8') as file:
                    writer = csv.writer(file)
                    for i, feature_vector in enumerate(feature_vectors):
                        row = feature_vector.tolist() + [next_label]
                        writer.writerow(row)
                        print(f"Saved feature vector {i+1}/{len(feature_vectors)}")
                print(f"Feature vectors saved to {self.fp_database_csv_path}")
            except Exception as vector_error:
                print(f"Error saving feature vectors: {str(vector_error)}")
                raise

        except Exception as e:
            print(f"Error in save_to_csv: {str(e)}")
            print("Detailed error information:")
            import traceback
            print(traceback.format_exc())

    def get_next_label(self):
        """Lấy label tiếp theo dựa trên file info.csv"""
        try:
            if not os.path.exists(self.information_csv_path):
                return 'img1'
            
            with open(self.information_csv_path, 'r', encoding='utf-8') as file:
                reader = csv.reader(file)
                rows = [row for row in reader if len(row) > 0]  # Bỏ qua các hàng trống
                if len(rows) <= 1:  # Chỉ có header hoặc không có dữ liệu
                    return 'img1'
                
                last_label = rows[-1][0]
                if last_label.startswith('img'):
                    next_index = int(last_label[3:]) + 1
                    return f'img{next_index}'
                else:
                    return 'img1'
        except Exception as e:
            print(f"Error getting next label: {str(e)}")
            return 'img1'

    def cleanup_temp_files(self):
        """Xóa các file tạm thời"""
        try:
            for fp_path in self.fingerprints:
                if os.path.exists(fp_path):
                    os.remove(fp_path)
                    print(f"Removed temporary file: {fp_path}")
            self.fingerprints.clear()
            print("All temporary files cleaned up")
        except Exception as e:
            print(f"Error during cleanup: {str(e)}")

    def check_system_state(self):
        """Kiểm tra trạng thái của hệ thống"""
        print("\nChecking system state...")
        print(f"Model loaded: {self.model is not None}")
        print(f"Number of fingerprints: {len(self.fingerprints)}")
        print(f"Person info collected: {bool(self.person_info)}")
        print(f"Working directory: {os.getcwd()}")
        print(f"Info CSV path: {self.information_csv_path}")
        print(f"DB FP2 CSV path: {self.fp_database_csv_path}")
        print("System check complete")


class Application(tk.Tk):
    def __init__(self):
        super().__init__()

        self.title("Fingerprint Database")
        self.geometry("400x200")

        self.main_frame = tk.Frame(self)
        self.main_frame.pack(expand=True)

        self.connect_button = tk.Button(
            self.main_frame,
            text="Connect to Scanner",
            command=self.connect_scanner,
            width=20,
            height=2
        )
        self.connect_button.pack(pady=20)

        self.status_label = tk.Label(
            self.main_frame,
            text="Status: Not Connected",
            font=("Arial", 10)
        )
        self.status_label.pack(pady=10)

        self.scanner_process = None
        self.observer = None
        
    def connect_scanner(self):
        try:
            if self.scanner_process is None:
                print("Connecting to scanner...")
                self.scanner_process = subprocess.Popen(
                    ['D:/Bin/Debug/x64/UareUSampleCSharp_CaptureOnly.exe']
                )
                
                self.start_monitoring()
                
                self.status_label.config(text="Status: Connected")
                self.connect_button.config(text="Disconnect")
                messagebox.showinfo("Success", "Connected to scanner successfully!")
                print("Connected to scanner")
            else:
                print("Disconnecting from scanner...")
                self.stop_monitoring()
                self.scanner_process.terminate()
                self.scanner_process = None
                
                self.status_label.config(text="Status: Not Connected")
                self.connect_button.config(text="Connect to Scanner")
                messagebox.showinfo("Success", "Disconnected from scanner!")
                print("Disconnected from scanner")
                
        except Exception as e:
            print(f"Error connecting to scanner: {str(e)}")
            messagebox.showerror("Error", f"Error connecting to scanner: {str(e)}")

    def start_monitoring(self):
        if self.observer is None:
            print("Starting monitoring...")
            directory_to_watch = "D:/Bin/Debug/x64"
            print(f"Watching directory: {directory_to_watch}")
            event_handler = FingerprintHandler()
            self.observer = Observer()
            self.observer.schedule(event_handler, directory_to_watch, recursive=False)
            self.observer.start()
            print("Monitoring started")

    def stop_monitoring(self):
        if self.observer is not None:
            print("Stopping monitoring...")
            self.observer.stop()
            self.observer.join()
            self.observer = None
            print("Monitoring stopped")

    def on_closing(self):
        if self.scanner_process is not None:
            self.stop_monitoring()
            self.scanner_process.terminate()
        self.destroy()

if __name__ == "__main__":
    app = Application()
    app.protocol("WM_DELETE_WINDOW", app.on_closing)
    app.mainloop()
