using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluenceFrame.Services
{
    public class FluenceConverter
    {
        // Converts a Bitmap image into a fluence matrix.
        // imageWidthMm and imageHeightMm represent the physical dimensions of the image.
        // resolutionMm is the desired resolution of the fluence matrix (2.5 mm in this case).
        public static float[,] ConvertToFluenceMatrix(Bitmap inputImage, float imageWidthMm, float imageHeightMm, float resolutionMm = 2.5f)
        {
            // Determine the dimensions of the fluence matrix.
            int matrixCols = (int)(imageWidthMm / resolutionMm);
            int matrixRows = (int)(imageHeightMm / resolutionMm);
            float[,] fluenceMatrix = new float[matrixRows, matrixCols];

            // Calculate the number of pixels per millimeter.
            float pixelsPerMmX = inputImage.Width / imageWidthMm;
            float pixelsPerMmY = inputImage.Height / imageHeightMm;

            // Loop through each cell of the fluence matrix.
            for (int row = 0; row < matrixRows; row++)
            {
                for (int col = 0; col < matrixCols; col++)
                {
                    // Determine the pixel region corresponding to this cell.
                    int startX = (int)(col * resolutionMm * pixelsPerMmX);
                    int startY = (int)(row * resolutionMm * pixelsPerMmY);
                    int endX = (int)((col + 1) * resolutionMm * pixelsPerMmX);
                    int endY = (int)((row + 1) * resolutionMm * pixelsPerMmY);

                    // Accumulate the intensity values.
                    float intensitySum = 0;
                    int pixelCount = 0;
                    for (int y = startY; y < Math.Min(endY, inputImage.Height); y++)
                    {
                        for (int x = startX; x < Math.Min(endX, inputImage.Width); x++)
                        {
                            // Get the pixel and convert it to a grayscale intensity.
                            Color pixelColor = inputImage.GetPixel(x, y);
                            float intensity = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0f / 255.0f;
                            intensitySum += intensity;
                            pixelCount++;
                        }
                    }
                    // Average intensity for the cell.
                    fluenceMatrix[row, col] = pixelCount > 0 ? intensitySum / pixelCount : 0;
                }
            }

            // Normalize the matrix so that the maximum intensity becomes 1.
            float maxIntensity = 0;
            foreach (float intensity in fluenceMatrix)
            {
                if (intensity > maxIntensity)
                    maxIntensity = intensity;
            }
            if (maxIntensity > 0)
            {
                for (int row = 0; row < matrixRows; row++)
                {
                    for (int col = 0; col < matrixCols; col++)
                    {
                        fluenceMatrix[row, col] /= maxIntensity;
                    }
                }
            }

            return fluenceMatrix;
        }
    }
}
