#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides extension methods for the <see cref="Image"/> class.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Gets the file extension based on the image format.
    /// </summary>
    /// <param name="image">The image instance.</param>
    /// <returns>The file extension including the dot (e.g., ".jpg", ".png").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static string GetFileExtension(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Format switch
        {
            ImageFormat.Jpeg => ".jpg",
            ImageFormat.Png => ".png",
            ImageFormat.Bmp => ".bmp",
            ImageFormat.Tiff => ".tiff",
            ImageFormat.WebP => ".webp",
            ImageFormat.Raw => ".raw",
            _ => ".bin"
        };
    }

    /// <summary>
    /// Gets the human-readable file size with appropriate unit (KB, MB, GB).
    /// </summary>
    /// <param name="image">The image instance.</param>
    /// <returns>A formatted string representing the file size.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static string GetFormattedFileSize(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return FormatFileSize(image.FileSizeBytes);
    }

    /// <summary>
    /// Gets the pixel data size in megabytes.
    /// </summary>
    /// <param name="image">The image instance.</param>
    /// <returns>The size in megabytes (MB) as a double.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static double GetPixelDataSizeInMegabytes(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var bytes = image.CalculatePixelDataSize();
        return Math.Round(bytes / (1024.0 * 1024.0), 2);
    }

    /// <summary>
    /// Determines whether the image is in a grayscale color space.
    /// </summary>
    /// <param name="image">The image instance.</param>
    /// <returns>True if the image uses a grayscale color space; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static bool IsGrayscale(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.ColorSpace is ColorSpace.Grayscale or ColorSpace.Ycbcr;
    }

    /// <summary>
    /// Formats the file size in bytes to a human-readable string.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <returns>A formatted string with appropriate unit.</returns>
    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        double size = bytes;

        while (size >= 1024 && counter < suffixes.Length - 1)
        {
            size /= 1024;
            counter++;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", size, suffixes[counter]);
    }
}