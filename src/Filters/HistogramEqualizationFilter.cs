#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Filters;

/// <summary>
/// Per-channel histogram equalization. Each colour channel is remapped through its
/// own cumulative distribution so the output spans the full 0–255 range.
/// </summary>
/// <remarks>
/// Only the first three channels (R, G, B) are equalized; any additional channel
/// (alpha) is copied unchanged, matching the other CPU filters.
/// </remarks>
public static class HistogramEqualizationFilter
{
    /// <summary>Number of intensity bins in an 8-bit channel.</summary>
    public const int BinCount = 256;

    /// <summary>
    /// Creates a <see cref="FilterConfiguration"/> that selects this filter via <see cref="FilterType.Histogram"/>.
    /// </summary>
    public static FilterConfiguration CreateConfiguration() => new()
    {
        Name = "HistogramEqualization",
        FilterType = FilterType.Histogram,
        Description = "Per-channel histogram equalization"
    };

    /// <summary>
    /// Equalizes the pixel data of <paramref name="image"/> in place and returns it.
    /// </summary>
    /// <param name="image">The image to equalize. Its <see cref="Image.PixelData"/> is replaced.</param>
    /// <returns>The same <paramref name="image"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="image"/> is null.</exception>
    public static Image Apply(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        int bpp = Math.Max(1, image.BitsPerPixel / 8);
        var src = image.PixelData ?? new byte[image.Width * image.Height * bpp];

        image.PixelData = Equalize(src, bpp);
        image.ModifiedAt = DateTime.UtcNow;
        return image;
    }

    /// <summary>
    /// Computes a 256-bin histogram for each colour channel of interleaved pixel data.
    /// </summary>
    /// <param name="src">Interleaved pixel bytes.</param>
    /// <param name="bpp">Bytes per pixel.</param>
    /// <returns>One histogram per colour channel (at most three).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bpp"/> is not positive.</exception>
    public static int[][] ComputeHistograms(ReadOnlySpan<byte> src, int bpp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bpp);

        int channels = Math.Min(bpp, 3);
        var histograms = new int[channels][];
        for (int c = 0; c < channels; c++)
        {
            var hist = new int[BinCount];
            for (int p = c; p < src.Length; p += bpp)
                hist[src[p]]++;
            histograms[c] = hist;
        }
        return histograms;
    }

    /// <summary>
    /// Returns a new pixel buffer with every colour channel equalized.
    /// </summary>
    /// <param name="src">Interleaved pixel bytes. Not modified.</param>
    /// <param name="bpp">Bytes per pixel.</param>
    /// <returns>A new array of the same length as <paramref name="src"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bpp"/> is not positive.</exception>
    public static byte[] Equalize(ReadOnlySpan<byte> src, int bpp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bpp);

        int channels = Math.Min(bpp, 3);
        int pixelCount = src.Length / bpp;
        var histograms = ComputeHistograms(src, bpp);
        var luts = new byte[channels][];
        for (int c = 0; c < channels; c++)
            luts[c] = BuildLookupTable(histograms[c], pixelCount);

        var dst = new byte[src.Length];
        for (int p = 0; p < src.Length; p++)
        {
            int c = p % bpp;
            dst[p] = c < channels ? luts[c][src[p]] : src[p];
        }
        return dst;
    }

    /// <summary>
    /// Builds the intensity remapping table for one channel from its histogram.
    /// </summary>
    /// <remarks>
    /// Uses the standard formula <c>round((cdf(v) - cdfMin) * 255 / (N - cdfMin))</c>.
    /// When the channel holds a single intensity (<c>N - cdfMin == 0</c>) the identity
    /// table is returned so flat regions keep their value.
    /// </remarks>
    private static byte[] BuildLookupTable(int[] histogram, int pixelCount)
    {
        var lut = new byte[BinCount];

        int cdfMin = 0;
        for (int v = 0; v < BinCount; v++)
        {
            if (histogram[v] > 0)
            {
                cdfMin = histogram[v];
                break;
            }
        }

        int denominator = pixelCount - cdfMin;
        if (denominator <= 0)
        {
            for (int v = 0; v < BinCount; v++)
                lut[v] = (byte)v;
            return lut;
        }

        int running = 0;
        for (int v = 0; v < BinCount; v++)
        {
            running += histogram[v];
            lut[v] = (byte)Math.Clamp(
                (int)Math.Round((running - cdfMin) * 255.0 / denominator), 0, 255);
        }
        return lut;
    }
}
