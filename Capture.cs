using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using DPUruNet;

namespace UareUSampleCSharp
{
    public partial class Capture : Form
    {
        /// <summary>
        /// Holds the main form with many functions common to all of SDK actions.
        /// </summary>
        public Form_Main _sender;
        private int captureCounter = 1; // Counter to increment file names

        public Capture()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        //------------------------------------------25/10 10:22 ---------------------------------
        private void Capture_Load(object sender, EventArgs e)
        {
            // Reset variables
            pbFingerprint.Image = null;

            if (!_sender.OpenReader())
            {
                this.Close();
            }

            if (!_sender.StartCaptureAsync(this.OnCaptured))
            {
                this.Close();
            }
        }

        //private void Capture_Load(object sender, EventArgs e)
        //{
        //    pbFingerprint.Image = null;

        //    if (!_sender.OpenReader())
        //    {
        //        this.Close();
        //    }

        //    if (!_sender.StartCaptureAsync(this.OnCaptured))
        //    {
        //        this.Close();
        //    }
        //}


        //------------------------------------------25/10 10:22 ---------------------------------

        /// <summary>
        /// Handler for when a fingerprint is captured.
        /// </summary>
        /// <param name="captureResult">contains info and data on the fingerprint capture</param>
        //public void OnCaptured(CaptureResult captureResult)
        //{
        //    try
        //    {
        //        // Check capture quality and throw an error if bad.
        //        if (!_sender.CheckCaptureResult(captureResult)) return;

        //        // Create bitmap
        //        foreach (Fid.Fiv fiv in captureResult.Data.Views)
        //        {
        //            SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Send error message, then close form
        //        SendMessage(Action.SendMessage, "Error:  " + ex.Message);
        //    }
        //}




        //--------------------------------------------------------------------------------------------------------

        //public void OnCaptured(CaptureResult captureResult)
        //{
        //    try
        //    {
        //        // Check capture quality and throw an error if bad
        //        if (!_sender.CheckCaptureResult(captureResult)) return;

        //        // Save the captured fingerprint image to a temporary file
        //        CaptureImage(captureResult);  // Pass captureResult to CaptureImage

        //        // Create bitmap
        //        foreach (Fid.Fiv fiv in captureResult.Data.Views)
        //        {
        //            SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Send error message, then close form
        //        SendMessage(Action.SendMessage, "Error: " + ex.Message);
        //    }
        //}


        //------------------------------------------25/10 10:22 ---------------------------------

        private int fingerprintCounter = 0;

        public void OnCaptured(CaptureResult captureResult)
        {
            try
            {
                // Kiểm tra chất lượng ảnh chụp
                if (!_sender.CheckCaptureResult(captureResult)) return;

                // Tăng bộ đếm cho mỗi lần chụp mới
                fingerprintCounter++;

                // Lưu ảnh vân tay vào file tạm thời với tên tăng dần
                CaptureImage(captureResult, fingerprintCounter);

                // Tạo bitmap để hiển thị
                foreach (Fid.Fiv fiv in captureResult.Data.Views)
                {
                    SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
                }

                // Khởi động lại quá trình chụp để cho phép chụp liên tục
                _sender.StartCaptureAsync(OnCaptured);
            }
            catch (Exception ex)
            {
                SendMessage(Action.SendMessage, "Error: " + ex.Message);
            }
        }

        //private void OnCaptured(CaptureResult captureResult)
        //{
        //    // Use the previous check for capture result quality
        //    if (!_sender.CheckCaptureResult(captureResult))
        //    {
        //        return;
        //    }

        //    // Create dynamically named file
        //    string filePath = Path.Combine(Path.GetTempPath(), $"img_temp{captureCounter}.jpg");
        //    using (var ms = new MemoryStream(captureResult.Data))
        //    {
        //        Bitmap bitmap = new Bitmap(ms);
        //        bitmap.Save(filePath, ImageFormat.Jpeg);
        //    }

        //    // Increment the file name counter
        //    captureCounter++;

        //    // Optionally display the image on UI or pass it to the next process
        //    pbFingerprint.Image = Image.FromFile(filePath);

        //    // Continue capturing the next fingerprint automatically
        //    _sender.StartCaptureAsync(this.OnCaptured);
        //}


        //------------------------------------------25/10 10:22 ---------------------------------

        //-----------------------------------------------------------------------------------------------------------

        // Modify CaptureImage to accept captureResult as a parameter
        //private void CaptureImage(CaptureResult captureResult)
        //{
        //    if (captureResult != null && captureResult.Data.Views.Count > 0)  // Use Count instead of Length
        //    {
        //        // Assuming you're saving the first view in the Data.Views array
        //        string tempPath = Path.Combine(Path.GetTempPath(), "temp_fingerprint.jpg");
        //        Fid.Fiv fiv = captureResult.Data.Views[0];  // Access the first view
        //        using (var bitmap = _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
        //        {
        //            bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);  // Save to temporary path
        //        }

        //        // Optionally, you can log the path or any additional info here
        //        Console.WriteLine($"Fingerprint image saved to: {tempPath}");

        //    }
        //}


        private void CaptureImage(CaptureResult captureResult, int counter)
        {
            if (captureResult != null && captureResult.Data.Views.Count > 0)
            {
                // Path to the temporary image
                string tempPath = Path.Combine(Path.GetTempPath(), $"temp_fingerprint{counter}.jpg");

                // Delete the old temp image if it exists
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                        Console.WriteLine("Previous temp image deleted.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error deleting previous temp image: " + ex.Message);
                    }
                }

                // Save the new fingerprint image
                Fid.Fiv fiv = captureResult.Data.Views[0];
                using (var bitmap = _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
                {
                    bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                Console.WriteLine($"New fingerprint image saved to: {tempPath}");
            }
        }






        //------------------------------------------------------------------------------------------------------


        //Update 3rd
        //public void OnCaptured(CaptureResult captureResult)
        //{
        //    try
        //    {
        //        // Check capture quality and throw an error if bad
        //        if (!_sender.CheckCaptureResult(captureResult)) return;

        //        // Overwrite or delete the old fingerprint image and save the new one
        //        CaptureImage(captureResult);  // Pass captureResult to CaptureImage

        //        // Create bitmap for display (optional)
        //        foreach (Fid.Fiv fiv in captureResult.Data.Views)
        //        {
        //            SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
        //        }

        //        // Here you may add logic to start the next capture process, if needed

        //    }
        //    catch (Exception ex)
        //    {
        //        // Send error message, then close form
        //        SendMessage(Action.SendMessage, "Error: " + ex.Message);
        //    }
        //}

        //// Modify CaptureImage to accept captureResult as a parameter and delete previous temp file
        //private void CaptureImage(CaptureResult captureResult)
        //{
        //    if (captureResult != null && captureResult.Data.Views.Count > 0)
        //    {
        //        // Path to the temporary image
        //        string tempPath = Path.Combine(Path.GetTempPath(), "temp_fingerprint.jpg");

        //        // Delete the old temp image if it exists
        //        if (File.Exists(tempPath))
        //        {
        //            try
        //            {
        //                File.Delete(tempPath); // Delete the previous temp image
        //                Console.WriteLine("Previous temp image deleted.");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error deleting previous temp image: " + ex.Message);
        //            }
        //        }

        //        // Save the new fingerprint image to the same temp file path
        //        Fid.Fiv fiv = captureResult.Data.Views[0];  // Access the first view
        //        using (var bitmap = _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
        //        {
        //            bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);  // Save to temporary path
        //        }

        //        Console.WriteLine($"New fingerprint image saved to: {tempPath}");

        //        // Here, you can trigger the process to send the image to your Python model
        //        // For example, you could call a function to start the processing in Python
        //    }
        //}


        //update 4th
        //public void OnCaptured(CaptureResult captureResult)
        //{
        //    try
        //    {
        //        // Check capture quality and throw an error if bad
        //        if (!_sender.CheckCaptureResult(captureResult)) return;

        //        // Save the captured fingerprint image to a temporary file
        //        CaptureImage(captureResult);  // Pass captureResult to CaptureImage

        //        // Create bitmap (optional, if needed for UI display)
        //        foreach (Fid.Fiv fiv in captureResult.Data.Views)
        //        {
        //            SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
        //        }

        //        // Restart the capture process to allow continuous fingerprint capture
        //        _sender.StartCaptureAsync(OnCaptured);  // Restart capture after processing
        //    }
        //    catch (Exception ex)
        //    {
        //        // Send error message, then close form
        //        SendMessage(Action.SendMessage, "Error: " + ex.Message);
        //    }
        //}

        //// Modify CaptureImage to accept captureResult as a parameter and delete previous temp file
        //private void CaptureImage(CaptureResult captureResult)
        //{
        //    if (captureResult != null && captureResult.Data.Views.Count > 0)
        //    {
        //        // Path to the temporary image
        //        string tempPath = Path.Combine(Path.GetTempPath(), "temp_fingerprint.jpg");

        //        // Delete the old temp image if it exists
        //        if (File.Exists(tempPath))
        //        {
        //            try
        //            {
        //                File.Delete(tempPath); // Delete the previous temp image
        //                Console.WriteLine("Previous temp image deleted.");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine("Error deleting previous temp image: " + ex.Message);
        //            }
        //        }

        //        // Save the new fingerprint image to the same temp file path
        //        Fid.Fiv fiv = captureResult.Data.Views[0];  // Access the first view
        //        using (var bitmap = _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
        //        {
        //            bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);  // Save to temporary path
        //        }

        //        Console.WriteLine($"New fingerprint image saved to: {tempPath}");

        //        // You can trigger your Python processing here as well
        //    }
        //}

        ////Update 5th
        //private int fingerprintCounter = 0;

        //public void OnCaptured(CaptureResult captureResult)
        //{
        //    try
        //    {
        //        // Check capture quality and throw an error if bad
        //        if (!_sender.CheckCaptureResult(captureResult)) return;

        //        // Increment the counter for each new capture
        //        fingerprintCounter++;

        //        // Save the captured fingerprint image to a temporary file with an incremented name
        //        CaptureImage(captureResult, fingerprintCounter);  // Pass the counter to CaptureImage

        //        // Create bitmap
        //        foreach (Fid.Fiv fiv in captureResult.Data.Views)
        //        {
        //            SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Send error message, then close form
        //        SendMessage(Action.SendMessage, "Error: " + ex.Message);
        //    }
        //}

        //// Modify CaptureImage to accept captureResult and the counter as parameters
        //private void CaptureImage(CaptureResult captureResult, int counter)
        //{
        //    if (captureResult != null && captureResult.Data.Views.Count > 0)  // Use Count instead of Length
        //    {
        //        // Save the fingerprint image with an incremented filename (e.g., temp_fingerprint1.jpg)
        //        string tempPath = Path.Combine(Path.GetTempPath(), $"temp_fingerprint{counter}.jpg");
        //        Fid.Fiv fiv = captureResult.Data.Views[0];  // Access the first view
        //        using (var bitmap = _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
        //        {
        //            bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);  // Save to temporary path
        //        }

        //        // Optionally, you can log the path or any additional info here
        //        Console.WriteLine($"Fingerprint image saved to: {tempPath}");
        //    }
        //}


        ////Update Sonnet
        //private int fingerprintCounter = 0;

        //public void OnCaptured(CaptureResult captureResult)
        //{
        //    try
        //    {
        //        // Kiểm tra chất lượng ảnh chụp
        //        if (!_sender.CheckCaptureResult(captureResult)) return;

        //        // Tăng bộ đếm cho mỗi lần chụp mới
        //        fingerprintCounter++;

        //        // Lưu ảnh vân tay vào file tạm thời với tên tăng dần
        //        CaptureImage(captureResult, fingerprintCounter);

        //        // Tạo bitmap để hiển thị (nếu cần)
        //        foreach (Fid.Fiv fiv in captureResult.Data.Views)
        //        {
        //            SendMessage(Action.SendBitmap, _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height));
        //        }

        //        // Gọi hàm để xử lý ảnh mới trong Python (cần được triển khai)
        //        ProcessNewFingerprint(fingerprintCounter);

        //        // Khởi động lại quá trình chụp để cho phép chụp liên tục
        //        _sender.StartCaptureAsync(OnCaptured);
        //    }
        //    catch (Exception ex)
        //    {
        //        SendMessage(Action.SendMessage, "Lỗi: " + ex.Message);
        //    }
        //}

        //private void CaptureImage(CaptureResult captureResult, int counter)
        //{
        //    if (captureResult != null && captureResult.Data.Views.Count > 0)
        //    {
        //        string tempPath = Path.Combine(Path.GetTempPath(), $"temp_fingerprint{counter}.jpg");
        //        Fid.Fiv fiv = captureResult.Data.Views[0];
        //        using (var bitmap = _sender.CreateBitmap(fiv.RawImage, fiv.Width, fiv.Height))
        //        {
        //            bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
        //        }
        //        Console.WriteLine($"Ảnh vân tay đã được lưu tại: {tempPath}");
        //    }
        //}

        //private void ProcessNewFingerprint(int counter)
        //{
        //    // Gọi mã Python để xử lý ảnh vân tay mới
        //    // Ví dụ: sử dụng Process.Start để chạy script Python
        //    // Truyền counter như một tham số để Python biết file nào cần xử lý
        //}








        /// <summary>
        /// Close window.
        /// </summary>
        /// 







        //---------------------------------------------------------------------------------------------------------------

        //using System;
        //using System.Drawing;
        //using System.Drawing.Imaging;
        //using System.IO;
        //using System.Windows.Forms;
        //using DPUruNet;

        //namespace UareUSampleCSharp
        //{
        //    public partial class Capture : Form
        //    {
        //        private int captureCounter = 1; // Counter to increment file names

        //        public Form_Main _sender;

        //        public Capture()
        //        {
        //            InitializeComponent();
        //        }

        //        private void Capture_Load(object sender, EventArgs e)
        //        {
        //            pbFingerprint.Image = null;

        //            if (!_sender.OpenReader())
        //            {
        //                this.Close();
        //            }

        //            if (!_sender.StartCaptureAsync(this.OnCaptured))
        //            {
        //                this.Close();
        //            }
        //        }

        //        private void OnCaptured(CaptureResult captureResult)
        //        {
        //            if (captureResult.Quality == Constants.CaptureQuality.Good)
        //            {
        //                // Create dynamically named file
        //                string filePath = Path.Combine(Path.GetTempPath(), $"img_temp{captureCounter}.jpg");
        //                using (var ms = new MemoryStream(captureResult.Data))
        //                {
        //                    Bitmap bitmap = new Bitmap(ms);
        //                    bitmap.Save(filePath, ImageFormat.Jpeg);
        //                }

        //                // Increment the file name counter
        //                captureCounter++;

        //                // Optionally display the image on UI or pass it to the next process
        //                pbFingerprint.Image = Image.FromFile(filePath);

        //                // Continue capturing the next fingerprint automatically
        //                _sender.StartCaptureAsync(this.OnCaptured);
        //            }
        //        }
        //    }

        //}

        //----------------------------------------------------------------------------------------------------------

        private void btnBack_Click(System.Object sender, System.EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Close window.
        /// </summary>
        private void Capture_Closed(object sender, EventArgs e)
        {
            _sender.CancelCaptureAndCloseReader(this.OnCaptured);
        }

        #region SendMessage
        private enum Action
        {
            SendBitmap,
            SendMessage
        }
        private delegate void SendMessageCallback(Action action, object payload);
        private void SendMessage(Action action, object payload)
        {
            try
            {
                if (this.pbFingerprint.InvokeRequired)
                {
                    SendMessageCallback d = new SendMessageCallback(SendMessage);
                    this.Invoke(d, new object[] { action, payload });
                }
                else
                {
                    switch (action)
                    {
                        case Action.SendMessage:
                            MessageBox.Show((string)payload);
                            break;
                        case Action.SendBitmap:
                            pbFingerprint.Image = (Bitmap)payload;
                            pbFingerprint.Refresh();
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}