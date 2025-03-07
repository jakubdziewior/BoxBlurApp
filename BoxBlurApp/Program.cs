// ------------------------------------------------------------------------------
// Description: Program implements image blur effect using Box Blur method
//              with multithreading and the option to choose implementation
//              in C# or ASM
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoxBlurApp
{
    public partial class Program : Form
    {
        /// <summary>
        /// Imported assembly procedure for calculating pixel sums
        /// </summary>
        [SecurityCritical]
        [DllImport(@"C:\Users\jakub\source\repos\BoxBlurApp\x64\Release\BoxBlurASM.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void CalculatePixelSumsASM(
            byte* rgbValues,     // Pointer to image buffer
            int width,           // Image width
            int height,          // Image height
            int blurSize,        // Blur radius
            int stride,          // Line width in bytes
            int x,               // X coordinate
            int y,               // Y coordinate
            int maxIndex,        // Maximum buffer index
            out long sumR,       // Sum of R component
            out long sumG,       // Sum of G component
            out long sumB,       // Sum of B component
            out int count        // Number of pixels
        );

        /// <summary>
        /// Imported C# method for calculating pixel sums
        /// </summary>
        [SecurityCritical]
        [DllImport(@"C:\Users\jakub\source\repos\BoxBlurApp\BoxBlurCS\bin\x64\Release\BoxBlurCS.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern unsafe void CalculatePixelSumsCS(
            byte* rgbValues,     // Pointer to image buffer
            int width,           // Image width
            int height,          // Image height
            int blurSize,        // Blur radius
            int stride,          // Line width in bytes
            int x,               // X coordinate
            int y,               // Y coordinate
            int maxIndex,        // Maximum buffer index
            out long sumR,       // Sum of R component
            out long sumG,       // Sum of G component
            out long sumB,       // Sum of B component
            out int count        // Number of pixels
        );

        // Synchronization object for concurrent operations
        private readonly object _lockObject = new object();

        // Flag indicating whether the image is currently being processed
        private bool isProcessing = false;

        /// <summary>
        /// Class constructor initializing form components
        /// </summary>
        public Program()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method displaying preview of the processed image
        /// </summary>
        private void PreviewImage(object sender, EventArgs e)
        {
            try
            {
                if (blurredPictureBox.Image != null)
                {
                    using (var previewImage = new Bitmap(blurredPictureBox.Image))
                    {
                        if (previewImage.Width > 0 && previewImage.Height > 0)
                        {
                            var previewForm = new ImagePreviewForm(previewImage);
                            previewForm.ShowDialog();
                        }
                    }
                }
            }
            // In PreviewImage method
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Error creating preview: {ex.Message}", "Preview Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying preview: {ex.Message}", "Preview Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method loading image from file
        /// </summary>
        private void LoadImage(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (originalImage != null)
                        {
                            originalImage.Dispose();
                        }
                        if (blurredPictureBox.Image != null)
                        {
                            blurredPictureBox.Image.Dispose();
                            blurredPictureBox.Image = null;
                        }
                        timeLabel.Text = "Execution time: 0 ms";
                        originalImage = new Bitmap(openFileDialog.FileName);
                        originalPictureBox.Image = new Bitmap(originalImage);
                        processImageButton.Enabled = true;
                        saveImageButton.Enabled = false;
                        previewImageButton.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Method processing the image with protection against multiple calls
        /// </summary>
        private async void ProcessImage(object sender, EventArgs e)
        {
            // Check if the image is not already being processed
            if (isProcessing) return;

            try
            {
                // Set processing flag and disable buttons
                isProcessing = true;
                processImageButton.Enabled = false;
                previewImageButton.Enabled = false;
                saveImageButton.Enabled = false;

                // Check if there is an image to process
                if (originalImage == null) return;

                // Get processing parameters
                int blurSize = blurIntensityTrackBar.Value;
                int threadCount = (int)threadCountNumericUpDown.Value;

                // Initialize stopwatch to measure execution time
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Release resources of the previous processed image
                if (blurredPictureBox.Image != null)
                {
                    blurredPictureBox.Image.Dispose();
                }

                // Asynchronous image processing
                var blurredImage = await Task.Run(() => BoxBlur(originalImage, blurSize, threadCount));

                // Stop the stopwatch
                stopwatch.Stop();

                // Display the processed image and execution time
                blurredPictureBox.Image = blurredImage;
                timeLabel.Text = $"Execution time: {stopwatch.ElapsedMilliseconds} ms";
            }
            finally
            {
                // Restore button state and processing flag
                isProcessing = false;
                saveImageButton.Enabled = true;
                previewImageButton.Enabled = true;
                processImageButton.Enabled = true;
            }
        }

        /// <summary>
        /// Method saving the processed image to a file
        /// </summary>
        private void SaveImage(object sender, EventArgs e)
        {
            if (blurredPictureBox.Image == null) return;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
                saveFileDialog.Title = "Save blurred image";
                saveFileDialog.DefaultExt = "jpg";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                    ImageFormat format = ImageFormat.Jpeg;

                    switch (ext)
                    {
                        case ".png":
                            format = ImageFormat.Png;
                            break;
                        case ".bmp":
                            format = ImageFormat.Bmp;
                            break;
                    }

                    try
                    {
                        blurredPictureBox.Image.Save(saveFileDialog.FileName, format);
                        MessageBox.Show("Image has been saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while saving the image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Method implementing the Box Blur algorithm
        /// </summary>
        private Bitmap BoxBlur(Bitmap sourceImage, int blurSize, int threadCount)
        {
            if (threadCount <= 0) threadCount = Environment.ProcessorCount;
            if (blurSize < 0) blurSize = 0;

            Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height);
            try
            {
                Rectangle rect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);

                // Copying the original image to result
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(sourceImage, 0, 0);
                }

                // Locking the bitmap for read/write
                BitmapData sourceData = result.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                try
                {
                    int bytes = Math.Abs(sourceData.Stride) * result.Height;
                    byte[] rgbValues = new byte[bytes];
                    byte[] resultValues = new byte[bytes];

                    // Copying data from bitmap to array
                    Marshal.Copy(sourceData.Scan0, rgbValues, 0, bytes);
                    Array.Clear(resultValues, 0, bytes);

                    int height = result.Height;
                    int width = result.Width;
                    int blockSize = height / threadCount;

                    // Multithreaded processing
                    var tasks = new Task[threadCount];
                    for (int i = 0; i < threadCount; i++)
                    {
                        int startY = i * blockSize;
                        int endY = (i == threadCount - 1) ? height : startY + blockSize;

                        tasks[i] = Task.Factory.StartNew((Object obj) =>
                        {
                            var range = (Tuple<int, int>)obj;
                            ProcessImageBlock(range.Item1, range.Item2, width, height, blurSize,
                                            sourceData.Stride, 3, rgbValues, resultValues);
                        }, Tuple.Create(startY, endY));
                    }

                    // Waiting for all tasks to complete
                    Task.WaitAll(tasks);

                    // Copying processed data back to bitmap
                    Marshal.Copy(resultValues, 0, sourceData.Scan0, bytes);
                }
                finally
                {
                    result.UnlockBits(sourceData);
                }
            }
            // In BoxBlur method
            catch (Exception ex)
            {
                result.Dispose();
                MessageBox.Show($"Error processing image: {ex.Message}", "Processing Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Method processing an image block in a separate thread
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private unsafe void ProcessImageBlock(int startY, int endY, int width, int height, int blurSize, int stride, int pixelSize, byte[] rgbValues, byte[] resultValues)
        {
            try
            {
                fixed (byte* rgbPtr = rgbValues)
                fixed (byte* resultPtr = resultValues)
                {
                    int maxIndex = height * stride;

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            long sumR, sumG, sumB;
                            int count;

                            if (asmRadioButton.Checked)
                            {
                                CalculatePixelSumsASM(
                                    rgbPtr, width, height, blurSize, stride, x, y, maxIndex,
                                    out sumR, out sumG, out sumB, out count);
                            }
                            else
                            {
                                CalculatePixelSumsCS(
                                    rgbPtr, width, height, blurSize, stride, x, y, maxIndex,
                                    out sumR, out sumG, out sumB, out count);
                            }

                            if (count > 0)
                            {
                                int targetIndex = (y * stride) + (x * pixelSize);
                                resultPtr[targetIndex] = (byte)(sumB / count);
                                resultPtr[targetIndex + 1] = (byte)(sumG / count);
                                resultPtr[targetIndex + 2] = (byte)(sumR / count);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image block: {ex.Message}", "Processing Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Application entry point
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }

        /// <summary>
        /// Method releasing resources when closing the form
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            originalImage?.Dispose();
            originalPictureBox.Image?.Dispose();
            blurredPictureBox.Image?.Dispose();
        }
    }
}
