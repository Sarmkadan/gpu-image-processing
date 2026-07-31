#nullable enable
using System;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Utilities;

/// <summary>
/// Extension methods for analysing <see cref="Image"/> pixel data.
/// </summary>
public static class ImageAnalysisExtensions
{
    /// <summary>
    /// Calculates the average brightness of the image.
    /// Brightness is defined as the average of the R, G and B channel values
    /// (or the single channel value for grayscale images), normalised to the range 0‑1.
    /// </summary>
    /// <param name="image">The image to analyse.</param>
    /// <returns>The average brightness, or 0 if the image has no pixel data.</returns>
    public static double GetAverageBrightness(this Image image)
    {
        if (image.PixelData is null || image.PixelData.Length == 0)
            return 0.0;

        int channels = image.Channels;
        int bytesPerPixel = image.BitsPerPixel / 8;
        if (bytesPerPixel == 0 || channels == 0)
            return 0.0;

        long pixelCount = (long)image.Width * image.Height;
        double totalBrightness = 0.0;

        // Iterate over the raw byte array without any extra allocations.
        for (int i = 0, offset = 0; i < pixelCount; i++, offset += bytesPerPixel)
        {
            // If the image has at least three channels, use R, G, B.
            // Otherwise fall back to the first channel (grayscale).
            if (channels >= 3)
            {
                byte r = image.PixelData[offset];
                byte g = image.PixelData[offset + 1];
                byte b = image.PixelData[offset + 2];
                totalBrightness += (r + g + b) / 3.0;
            }
            else
            {
                // Grayscale – the first channel represents intensity.
                totalBrightness += image.PixelData[offset];
            }
        }

        // Normalise to 0‑1 (max value per channel is 255).
        return totalBrightness / (pixelCount * 255.0);
    }

    /// <summary>
    /// Determines whether the image is effectively grayscale.
    /// For each pixel the difference between the R, G and B channels must be
    /// less than or equal to the supplied <paramref name="tolerance"/>.
    /// </summary>
    /// <param name="image">The image to test.</param>
    /// <param name="tolerance">
    /// The maximum allowed absolute difference between any two colour channels
    /// (0‑255). Default is 0 (exact grayscale).
    /// </param>
    /// <returns>
    /// <c>true</c> if every pixel is within the tolerance; otherwise <c>false</c>.
    /// </returns>
    public static bool IsGrayscale(this Image image, int tolerance = 0)
    {
        if (image.PixelData is null || image.PixelData.Length == 0)
            return true; // No data – treat as grayscale.

        if (image.Channels < 3)
            return true; // Fewer than 3 channels cannot represent colour.

        int bytesPerPixel = image.BitsPerPixel / 8;
        if (bytesPerPixel == 0)
            return true;

        long pixelCount = (long)image.Width * image.Height;

        for (int i = 0, offset = 0; i < pixelCount; i++, offset += bytesPerPixel)
        {
            byte r = image.PixelData[offset];
            byte g = image.PixelData[offset + 1];
            byte b = image.PixelData[offset + 2];

            if (Math.Abs(r - g) > tolerance ||
                Math.Abs(r - b) > tolerance ||
                Math.Abs(g - b) > tolerance)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Produces a histogram for a single colour channel.
    /// </summary>
    /// <param name="image">The image to analyse.</param>
    /// <param name="channel">
    /// Zero‑based index of the channel (e.g. 0 = R, 1 = G, 2 = B, 3 = A).
    /// Must be less than <see cref="Image.Channels"/>.
    /// </param>
    /// <returns>
    /// An array of 256 integers where each index represents the count of pixels
    /// that have the corresponding channel value.
    /// </returns>
    public static int[] GetChannelHistogram(this Image image, int channel)
    {
        if (image.PixelData is null || image.PixelData.Length == 0)
            return new int[256];

        if (channel < 0 || channel >= image.Channels)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel index is out of range.");

        int bytesPerPixel = image.BitsPerPixel / 8;
        if (bytesPerPixel == 0)
            return new int[256];

        long pixelCount = (long)image.Width * image.Height;
        int[] histogram = new int[256];

        for (int i = 0, offset = 0; i < pixelCount; i++, offset += bytesPerPixel)
        {
            byte value = image.PixelData[offset + channel];
            histogram[value]++;
        }

        return histogram;
    }
}
