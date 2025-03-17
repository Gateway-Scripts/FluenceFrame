using FluenceFrame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VMS.TPS.Common.Model.Types;

namespace FluenceFrame.Services
{
    public static class FluenceConverter
    {
        public static FluenceModel LocalFluence { get; set; }


        /// <summary>
        /// Converts a normalized fluence map (2D float array) into a heat map BitmapSource.
        /// Each value is mapped to a color from blue (lowest) to red (highest).
        /// </summary>
        /// <param name="fluenceMap">2D float array (values assumed normalized to [0,1]).</param>
        /// <returns>A BitmapSource representing the heat map.</returns>
        public static BitmapSource ConvertFluenceMapToHeatMap()
        {
            var fluence = LocalFluence.Fluence;
            if (fluence == null)
                throw new ArgumentNullException(nameof(fluence));

            int height = fluence.GetLength(0);
            int width = fluence.GetLength(1);
            int bytesPerPixel = 4;
            int stride = width * bytesPerPixel;
            byte[] pixelData = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = fluence[y, x];
                    // Clamp the value to [0,1] just in case.
                    value = Math.Max(0, Math.Min(1, value));
                    Color color = GetHeatMapColor(value);
                    int pixelIndex = y * stride + x * bytesPerPixel;
                    pixelData[pixelIndex] = color.B; // Blue
                    pixelData[pixelIndex + 1] = color.G; // Green
                    pixelData[pixelIndex + 2] = color.R; // Red
                    pixelData[pixelIndex + 3] = 255;       // Alpha
                }
            }

            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            return bitmap;
        }

        /// <summary>
        /// Maps a normalized fluence value [0,1] to a heat map color.
        /// This function uses a hue gradient from blue (240°) to red (0°).
        /// </summary>
        /// <param name="value">Normalized value between 0 and 1.</param>
        /// <returns>A Color representing the corresponding heat map value.</returns>
        private static Color GetHeatMapColor(float value)
        {
            // Map the value to a hue: 0 -> red (0°), 1 -> blue (240°).
            // Invert the value so that 0 gives blue and 1 gives red.
            double hue = (1.0 - value) * 240;
            return ColorFromHSV(hue, 1.0, 1.0);
        }

        /// <summary>
        /// Converts HSV values to a Color (RGB).
        /// Hue is in degrees [0,360), and saturation/value are in [0,1].
        /// </summary>
        /// <param name="hue">Hue in degrees.</param>
        /// <param name="saturation">Saturation (0 to 1).</param>
        /// <param name="value">Value/Brightness (0 to 1).</param>
        /// <returns>A Color in RGB space.</returns>
        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            hue = hue % 360;
            double chroma = value * saturation;
            double hPrime = hue / 60.0;
            double x = chroma * (1 - Math.Abs(hPrime % 2 - 1));
            double r1 = 0, g1 = 0, b1 = 0;

            if (hPrime >= 0 && hPrime < 1)
            {
                r1 = chroma;
                g1 = x;
            }
            else if (hPrime >= 1 && hPrime < 2)
            {
                r1 = x;
                g1 = chroma;
            }
            else if (hPrime >= 2 && hPrime < 3)
            {
                g1 = chroma;
                b1 = x;
            }
            else if (hPrime >= 3 && hPrime < 4)
            {
                g1 = x;
                b1 = chroma;
            }
            else if (hPrime >= 4 && hPrime < 5)
            {
                r1 = x;
                b1 = chroma;
            }
            else if (hPrime >= 5 && hPrime < 6)
            {
                r1 = chroma;
                b1 = x;
            }

            double m = value - chroma;
            byte r = (byte)Math.Round((r1 + m) * 255);
            byte g = (byte)Math.Round((g1 + m) * 255);
            byte b = (byte)Math.Round((b1 + m) * 255);

            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Converts a given BitmapSource into a fluence map array.
        /// The fluence map is a 2D array of floats normalized to a maximum value of 1.0.
        /// The resolution is fixed at 2.5mm (0.25cm) per pixel.
        /// The origin is calculated as half the size of the fluence map (in pixels) multiplied by the resolution.
        /// </summary>
        /// <param name="source">The source image to convert.</param>
        /// <returns>A tuple containing the fluence map (float[,]) and the origin (Point).</returns>
        public static FluenceModel ConvertImageToFluenceMap(BitmapSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            LocalFluence = new FluenceModel();
            // Ensure the image is in a grayscale format (Gray8) for a single intensity value per pixel.
            BitmapSource graySource = DownsampleIfTooLarge(source);
            if (source.Format != PixelFormats.Gray8)
            {
                graySource = new FormatConvertedBitmap(graySource, PixelFormats.Gray8, null, 0);
            }

            int width = graySource.PixelWidth;
            int height = graySource.PixelHeight;

            // Allocate an array to receive pixel data.
            byte[] pixels = new byte[width * height];
            graySource.CopyPixels(pixels, width, 0);

            // Determine the maximum pixel value to use for normalization.
            byte maxPixel = 0;
            foreach (byte pixel in pixels)
            {
                if (pixel > maxPixel)
                    maxPixel = pixel;
            }
            // Prevent division by zero.
            if (maxPixel == 0)
                maxPixel = 1;

            // Create the fluence map as a 2D float array.
            float[,] fluence = new float[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixelValue = pixels[y * width + x];
                    // Normalize pixel value to the range [0, 1].
                    float normalizedValue = pixelValue / (float)maxPixel;
                    fluence[y, x] = normalizedValue;
                }
            }

            // Define the resolution: 2.5mm per pixel = 0.25cm per pixel.
            float resolution = 2.5f; // in mm

            // Calculate the origin position.
            // The formula used here is: originX = -((width - 1) / 2) * resolution, originY = ((height - 1) / 2) * resolution.
            // This positions the center (or near center) of the fluence map about the isocenter.
            float originX = -((width - 1) / 2.0f) * resolution;
            float originY = ((height - 1) / 2.0f) * resolution;
            Point origin = new Point(originX, originY);
            //set local fluence
            LocalFluence.Fluence = fluence;
            LocalFluence.Origin = origin;
            return new FluenceModel()
            {
                Fluence = fluence,
                Origin = origin
            };
            //return (fluenceMap, new Point(originX, originY));
        }
        /// <summary>
        /// Checks if the physical width of the image exceeds maxPhysicalWidth_cm and, if so, returns a downsampled image.
        /// </summary>
        /// <param name="source">The input BitmapSource.</param>
        /// <param name="samplingResolution_cm">Sampling resolution in centimeters (default: 0.25 cm, equivalent to 2.5 mm per pixel).</param>
        /// <param name="maxPhysicalWidth_cm">Maximum allowed physical width in cm (default: 20.0 cm).</param>
        /// <returns>A BitmapSource, downsampled by half if the image is too large, or the original if not.</returns>
        public static BitmapSource DownsampleIfTooLarge(BitmapSource source, double samplingResolution_cm = 0.25, double maxPhysicalWidth_cm = 20.0)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Calculate the physical width in cm.
            double physicalWidth_cm = source.PixelWidth * samplingResolution_cm;

            // Check if the physical width exceeds the maximum allowed.
            if (physicalWidth_cm > maxPhysicalWidth_cm)
            {
                // Downsample by applying a ScaleTransform with a scale factor of 0.5.
                var scaleTransform = new ScaleTransform(0.5, 0.5);
                var transformedBitmap = new TransformedBitmap();
                transformedBitmap.BeginInit();
                transformedBitmap.Source = source;
                transformedBitmap.Transform = scaleTransform;
                transformedBitmap.EndInit();
                transformedBitmap.Freeze(); // Make it cross-thread accessible.
                return transformedBitmap;
            }
            return source;
        }
    }
}
