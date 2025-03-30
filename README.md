# 🖐️ Fingerprint Recognition System  
_A fingerprint-based authentication system using image processing and machine learning._

## 📌 Overview  
This project is a fingerprint recognition system that captures, processes, and matches fingerprints for authentication. It utilizes **OpenCV**, **NumPy**, and **scikit-learn** for image processing and classification.

## 🚀 Features  
- ✅ Capture fingerprint images from a scanner or dataset  
- ✅ Preprocess fingerprints (grayscale conversion, noise reduction, edge detection)  
- ✅ Extract and match fingerprint features using ML algorithms  
- ✅ Store and retrieve fingerprint data from a database  

## 🛠 Installation  
Clone this repository:  
```sh
git clone https://github.com/nhuttran02/fingerprint-recognition-system.git
cd fingerprint-recognition-system 
```
## Install dependencies:
```sh
pip install -r requirements.txt
```

## 📂 Project Structure
```sh
📦 fingerprint-recognition-system
├── 📄 fingerprint-system_v1.py      # Main fingerprint recognition script
├── 📄 CaptureDatabase.py            # Script to store fingerprint data in DB
├── 📂 dataset/                      # Folder containing sample fingerprint images
├── 📄 requirements.txt               # Required dependencies
└── 📄 README.md                      # Project documentation
```

## 🖥️ Usage

Run the fingerprint recognition system:
```
python fingerprint-system_v1.py
```
To capture and store fingerprints in the database:
```
python CaptureDatabase.py
```
## ⚡ Technologies Used

Python (Main programming language)

OpenCV (Image processing)

NumPy (Matrix operations)

scikit-learn (Machine learning models)

SQLite/MySQL (Fingerprint data storage)

## Interface of the fingerprint recognition system
### Interface of the main system
 ![](Image/img5/main.png)

### Fingerprint recognition interface successful
 ![](Image/img5/success.png)

### Interface when user information is not found
 ![](Image/img5/fail.png)

## Interface of the database capture system
### Interface of the main system
 ![](Image/img5/inter1.png)

### Interface when new user successfully adds fingerprint data to the system
 ![](Image/img5/inter2.png)

### The interface simulates new user data when saved to the database
 ![](Image/img5/data.png)


## 👤 Author

📌 Nhut Tran🔗 GitHub: nhuttran02