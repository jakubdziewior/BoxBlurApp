// ------------------------------------------------------------------------------
// Description: Graphical user interface for an application implementing image blur effect
//              with controls for managing processing parameters
//------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;

namespace BoxBlurApp
{
    partial class Program
    {
        /// <summary>
        /// Declaration of user interface controls
        /// </summary>
        // Image display controls
        private PictureBox originalPictureBox;          // Displays the original image
        private PictureBox blurredPictureBox;           // Displays the processed (blurred) image

        // Processing parameter controls
        private TrackBar blurIntensityTrackBar;         // Slider for adjusting blur intensity
        private NumericUpDown threadCountNumericUpDown; // Field for selecting the number of processing threads
        private System.Windows.Forms.Label timeLabel;   // Label displaying processing time

        // Action buttons
        private Button loadImageButton;                 // Button for loading images
        private Button processImageButton;              // Button for starting image processing
        private Button saveImageButton;                 // Button for saving the processed image
        private Button previewImageButton;              // Button for previewing the processed image

        // Implementation selection
        private RadioButton asmRadioButton;             // Button for selecting ASM implementation
        private RadioButton csharpRadioButton;          // Button for selecting C# implementation

        // Image storage
        private Bitmap originalImage;                   // Stores the original image

        /// <summary>
        /// Initializes and configures all user interface components
        /// </summary>
        private void InitializeComponent()
        {
            // Main window configuration
            this.Text = "BoxBlurApp";
            this.Size = new Size(870, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Block window resizing
            this.MaximizeBox = false;                           // Disable maximize button
            this.MinimizeBox = true;                            // Keep minimize button

            // Initialization of original image display
            originalPictureBox = new PictureBox
            {
                Width = 400,
                Height = 300,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,    // Maintain image proportions
                Anchor = AnchorStyles.None
            };

            // Initialization of processed image display
            blurredPictureBox = new PictureBox
            {
                Width = 400,
                Height = 300,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,    // Maintain image proportions
                Anchor = AnchorStyles.None
            };

            // Initialization of blur intensity control
            blurIntensityTrackBar = new TrackBar
            {
                Minimum = 1,                           // Minimum blur radius
                Maximum = 20,                          // Maximum blur radius
                Value = 3,                             // Default blur radius
                Width = 200,
                TickStyle = TickStyle.BottomRight
            };

            // Initialization of thread count control
            threadCountNumericUpDown = new NumericUpDown
            {
                Minimum = 1,                           // Minimum number of threads
                Maximum = 64,                          // Maximum number of threads
                Value = Environment.ProcessorCount,    // Default is processor core count
                Width = 60
            };

            // Initialization of processing time display
            timeLabel = new Label
            {
                Text = "Execution time: 0 ms",         // Default execution time text
                AutoSize = true
            };

            // Initialization of action buttons
            loadImageButton = new Button
            {
                Text = "Load Image",
                Width = 100
            };

            processImageButton = new Button
            {
                Text = "Process Image",
                Width = 100,
                Enabled = false                        // Disabled until an image is loaded
            };

            saveImageButton = new Button
            {
                Text = "Save Image",
                Width = 100,
                Enabled = false                        // Disabled until an image is processed
            };

            previewImageButton = new Button
            {
                Text = "Preview Image",
                Width = 100,
                Enabled = false                        // Disabled until an image is processed
            };

            // Initialization of implementation selection controls
            asmRadioButton = new RadioButton
            {
                Text = "Use ASM",
                Checked = false                        // ASM implementation is not selected by default
            };

            csharpRadioButton = new RadioButton
            {
                Text = "Use C#",
                Checked = true                         // C# implementation is selected by default
            };

            // Layout configuration
            // Main container configuration
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                AutoSize = false
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 350));  // Height of the upper section
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));  // Height of the lower section

            // Image display panel configuration
            var imagePanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = false,
                Width = 850,
                Height = 350
            };
            imagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));  // Width of the left column
            imagePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));  // Width of the right column

            // Adding labels and image displays
            imagePanel.Controls.Add(new Label { Text = "Original Image", TextAlign = ContentAlignment.MiddleCenter }, 0, 0);
            imagePanel.Controls.Add(new Label { Text = "Blurred Image", TextAlign = ContentAlignment.MiddleCenter }, 1, 0);
            imagePanel.Controls.Add(originalPictureBox, 0, 1);
            imagePanel.Controls.Add(blurredPictureBox, 1, 1);

            // Bottom control panel configuration
            var bottomPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                Height = 250,
                Width = 850
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));  // Width of the left section
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));  // Width of the right section

            // Left control panel configuration
            var leftPanel = new TableLayoutPanel
            {
                Width = 425,
                Height = 250,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(20, 10, 0, 0)
            };
            leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Adding blur intensity controls
            leftPanel.Controls.Add(new Label
            {
                Text = "Blur Intensity:",
                Padding = new Padding(left: 0, top: 0, right: 0, bottom: 25),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left
            }, 0, 0);
            leftPanel.Controls.Add(blurIntensityTrackBar, 1, 0);

            // Adding thread count controls
            leftPanel.Controls.Add(new Label
            {
                Text = "Thread Count:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left
            }, 0, 1);
            leftPanel.Controls.Add(threadCountNumericUpDown, 1, 1);

            // Adding implementation selection controls
            leftPanel.Controls.Add(new Label
            {
                Text = "Library:",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left,
                Padding = new Padding(left: 0, top: 20, right: 0, bottom: 0)
            }, 0, 2);

            // Configuration of radio button panel
            var radioPanel = new Panel { AutoSize = true };
            asmRadioButton.Location = new Point(0, 20);
            csharpRadioButton.Location = new Point(120, 20);
            radioPanel.Controls.Add(asmRadioButton);
            radioPanel.Controls.Add(csharpRadioButton);
            leftPanel.Controls.Add(radioPanel, 1, 2);

            // Configuration of action buttons panel
            var buttonsPanel = new Panel { AutoSize = true };
            loadImageButton.Location = new Point(0, 15);
            processImageButton.Location = new Point(110, 15);
            loadImageButton.Size = new Size(100, 30);
            processImageButton.Size = new Size(100, 30);

            buttonsPanel.Controls.Add(loadImageButton);
            buttonsPanel.Controls.Add(processImageButton);
            leftPanel.Controls.Add(buttonsPanel, 1, 3);

            // Configuration of right control panel
            var rightPanel = new TableLayoutPanel
            {
                Width = 425,
                Height = 250,
                ColumnCount = 1,
                RowCount = 2
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 70));

            // Configuration of additional buttons panel
            var buttonFlowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Anchor = AnchorStyles.None
            };

            // Configuration of time label
            timeLabel.Anchor = AnchorStyles.None;
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;
            rightPanel.Controls.Add(timeLabel, 0, 0);

            // Standardization of button sizes
            previewImageButton.Size = new Size(100, 30);
            saveImageButton.Size = new Size(100, 30);

            // Adding spacing between buttons
            previewImageButton.Margin = new Padding(0, 0, 0, 60);
            saveImageButton.Margin = new Padding(10, 0, 0, 0);

            // Adding buttons to the flow panel
            buttonFlowPanel.Controls.Add(previewImageButton);
            buttonFlowPanel.Controls.Add(saveImageButton);

            rightPanel.Controls.Add(buttonFlowPanel, 0, 1);

            // Assembling the final layout
            bottomPanel.Controls.Add(leftPanel, 0, 0);
            bottomPanel.Controls.Add(rightPanel, 1, 0);
            mainLayout.Controls.Add(imagePanel, 0, 0);
            mainLayout.Controls.Add(bottomPanel, 0, 1);

            Controls.Add(mainLayout);

            // Connecting event handlers
            loadImageButton.Click += LoadImage;         // Image loading handler
            processImageButton.Click += ProcessImage;   // Image processing handler
            saveImageButton.Click += SaveImage;         // Processed image saving handler
            previewImageButton.Click += PreviewImage;   // Image preview handler
        }
    }
}
