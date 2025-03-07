//------------------------------------------------------------------------------
// Method: CalculatePixelSumsCS
// Description: Procedure calculates the sums of RGB components of pixels in the blur area around a given point (x, y). Used for implementing an image blur filter.
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace BoxBlurCS
{
    public static unsafe class BoxBlurCS
    {
        /// <summary>
        /// Calculates the sums of RGB components of pixels in a square blur area.
        /// </summary>
        /// <param name="rgbValues">Pointer to the image buffer in RGB format (3 bytes/pixel)</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <param name="blurSize">Radius of the blur area</param>
        /// <param name="stride">Line width in bytes (with alignment)</param>
        /// <param name="x">X-coordinate of the central point</param>
        /// <param name="y">Y-coordinate of the central point</param>
        /// <param name="maxIndex">Maximum index in the image buffer</param>
        /// <param name="sumR">Sum of the red (R) component in the area</param>
        /// <param name="sumG">Sum of the green (G) component in the area</param>
        /// <param name="sumB">Sum of the blue (B) component in the area</param>
        /// <param name="count">Number of pixels included</param>
        [DllExport("CalculatePixelSumsCS", CallingConvention = CallingConvention.StdCall)]
        public static void CalculatePixelSumsCS(
            byte* rgbValues,
            int width,
            int height,
            int blurSize,
            int stride,
            int x,
            int y,
            int maxIndex,
            out long sumR,
            out long sumG,
            out long sumB,
            out int count)
        {
            // Constant defining the size of a single pixel in bytes (RGB)
            const int pixelSize = 3;

            // Initialization of result variables
            sumR = 0;
            sumG = 0;
            sumB = 0;
            count = 0;

            // Iteration over the Y-coordinate of the blur area
            for (int dy = -blurSize; dy <= blurSize; dy++)
            {
                int ny = y + dy;  // Calculation of the current Y-coordinate
                if (ny < 0 || ny >= height) continue;  // Checking image boundaries

                // Iteration over the X-coordinate of the blur area
                for (int dx = -blurSize; dx <= blurSize; dx++)
                {
                    int nx = x + dx;  // Calculation of the current X-coordinate
                    if (nx < 0 || nx >= width) continue;  // Checking image boundaries

                    // Calculation of the pixel index in the buffer
                    int sourceIndex = (ny * stride) + (nx * pixelSize);
                    if (sourceIndex < 0 || sourceIndex + 2 >= maxIndex) continue;  // Verification of buffer boundaries

                    // Retrieving a pointer to the current pixel
                    byte* pixel = rgbValues + sourceIndex;

                    // Adding RGB components to the sums
                    sumB += *pixel;        // Blue component
                    sumG += *(pixel + 1);  // Green component
                    sumR += *(pixel + 2);  // Red component
                    count++;               // Incrementing the pixel counter
                }
            }
        }
    }
}
