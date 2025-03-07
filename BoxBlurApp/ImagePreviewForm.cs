// ------------------------------------------------------------------------------
// Description: Form displaying an image in fullscreen mode
//              while maintaining the image proportions
//------------------------------------------------------------------------------

using System.Drawing;
using System.Windows.Forms;

namespace BoxBlurApp
{
    /// <summary>
    /// Form for fullscreen image preview
    /// </summary>
    public class ImagePreviewForm : Form
    {
        /// <summary>
        /// Initializes the preview form with the provided image
        /// </summary>
        public ImagePreviewForm(Image image)
        {
            // Main window configuration
            this.WindowState = FormWindowState.Maximized;    // Fullscreen mode
            this.Text = "Image Preview";                     // Window title

            // Initialization of the image display control
            PictureBox pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,                      // Fill the entire window
                SizeMode = PictureBoxSizeMode.Zoom,         // Maintain image proportions
                Image = image                               // Assign the image to display
            };

            // Add the control to the form
            this.Controls.Add(pictureBox);
        }
    }
}
