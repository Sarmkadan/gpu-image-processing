#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Utilities;

/// <summary>
/// Provides extension methods for image processing operations including color space conversion,
/// resolution validation, format detection, and performance estimation.
/// </summary>
public static class ImageProcessingExtensions
{
    /// <summary>
    /// Gets the appropriate <see cref="ColorSpace"/> for the specified <see cref="ImageFormat"/>.
    /// </summary>
    /// <param name="format">The image format to get the color space for.</param>
    /// <returns>The corresponding <see cref="ColorSpace"/> for the format.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown format is provided.</exception>
    public static ColorSpace GetColorSpaceForFormat(this ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => ColorSpace.Ycbcr,
            ImageFormat.Png => ColorSpace.Rgba,
            ImageFormat.Bmp => ColorSpace.Bgra,
            ImageFormat.Tiff => ColorSpace.Rgba,
            ImageFormat.WebP => ColorSpace.Rgba,
            ImageFormat.Raw => ColorSpace.Rgb,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown image format")
        };
    }

    /// <summary>
    /// Calculates total bytes for image data.
    /// </summary>
    /// <param name="image">The image to calculate size for.</param>
    /// <returns>The total size in bytes of the image data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static long CalculateTotalBytes(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return image.CalculatePixelDataSize();
    }

    /// <summary>
    /// Checks if image resolution is within acceptable limits.
    /// </summary>
    /// <param name="image">The image to validate.</param>
    /// <returns>True if the resolution is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static bool IsResolutionValid(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return image.Width >= AppConstants.Processing.MinImageWidth &&
               image.Width <= AppConstants.Processing.MaxImageWidth &&
               image.Height >= AppConstants.Processing.MinImageHeight &&
               image.Height <= AppConstants.Processing.MaxImageHeight;
    }

    /// <summary>
    /// Gets image file extension from format.
    /// </summary>
    /// <param name="format">The image format to get extension for.</param>
    /// <returns>The file extension including leading dot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown format is provided.</exception>
    public static string GetFileExtension(this ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => ".jpg",
            ImageFormat.Png => ".png",
            ImageFormat.Bmp => ".bmp",
            ImageFormat.Tiff => ".tiff",
            ImageFormat.WebP => ".webp",
            ImageFormat.Raw => ".raw",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown image format")
        };
    }

    /// <summary>
    /// Gets image format from file extension.
    /// </summary>
    /// <param name="extension">The file extension to parse.</param>
    /// <returns>The corresponding <see cref="ImageFormat"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="extension"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="extension"/> is empty or whitespace.</exception>
    public static ImageFormat GetFormatFromExtension(this string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png" => ImageFormat.Png,
            ".bmp" => ImageFormat.Bmp,
            ".tiff" or ".tif" => ImageFormat.Tiff,
            ".webp" => ImageFormat.WebP,
            ".raw" => ImageFormat.Raw,
            _ => ImageFormat.Unknown
        };
    }

    /// <summary>
    /// Checks if a filter can be applied to the image.
    /// </summary>
    /// <param name="image">The image to check.</param>
    /// <param name="filterType">The filter type to validate.</param>
    /// <returns>True if the filter can be applied; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static bool CanApplyFilter(this Image image, FilterType filterType)
    {
        ArgumentNullException.ThrowIfNull(image);

        return filterType switch
        {
            FilterType.Grayscale => image.ColorSpace != ColorSpace.Grayscale,
            FilterType.Rotation => image.IsResolutionValid(),
            FilterType.Scaling => image.IsResolutionValid(),
            _ => true
        };
    }

    /// <summary>
    /// Gets aspect ratio description.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <returns>A description of the image's aspect ratio.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static string GetAspectRatioDescription(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var ratio = image.GetAspectRatio();
        return ratio switch
        {
            > 2.0 => "Ultra Wide",
            > 1.7 => "Wide",
            > 1.3 => "Landscape",
            > 0.9 and <= 1.1 => "Square",
            > 0.6 => "Portrait",
            > 0.5 => "Tall",
            _ => "Ultra Tall"
        };
    }

    /// <summary>
    /// Estimates processing time for filter.
    /// </summary>
    /// <param name="image">The image to process.</param>
    /// <param name="filter">The filter configuration.</param>
    /// <returns>Estimated processing time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> or <paramref name="filter"/> is null.</exception>
    public static double EstimateProcessingTime(this Image image, FilterConfiguration filter)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(filter);

        var pixelCount = image.GetPixelCount();
        var baseTime = pixelCount / 1_000_000.0; // 1M pixels per ms baseline

        var multiplier = filter.FilterType switch
        {
            FilterType.Blur or FilterType.GaussianBlur => 2.5,
            FilterType.EdgeDetection or FilterType.Sobel => 2.0,
            FilterType.Median => 3.0,
            FilterType.Rotation => 1.5,
            FilterType.Scaling => 1.8,
            _ => 1.0
        };

        return baseTime * multiplier;
    }

    /// <summary>
    /// Gets memory requirement for processing.
    /// </summary>
    /// <param name="image">The image to analyze.</param>
    /// <returns>Total memory requirement in bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null.</exception>
    public static long GetMemoryRequirement(this Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        var pixelData = image.CalculatePixelDataSize();
        var workingMemory = pixelData * 2; // Working buffer
        return pixelData + workingMemory + (1024 * 1024); // Buffer + overhead
    }
}

/// <summary>
/// Provides extension methods for batch processing operations.
/// </summary>
public static class BatchProcessingExtensions
{
    /// <summary>
    /// Estimates total processing time for batch.
    /// </summary>
    /// <param name="batch">The image batch to process.</param>
    /// <param name="images">The collection of images.</param>
    /// <param name="filters">The collection of filters to apply.</param>
    /// <returns>Total estimated processing time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static double EstimateTotalTime(
        this ImageBatch batch,
        IEnumerable<Image> images,
        IEnumerable<FilterConfiguration> filters)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(images);
        ArgumentNullException.ThrowIfNull(filters);

        var totalTime = 0.0;

        foreach (var imageId in batch.ImageIds)
        {
            var image = images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                continue;

            foreach (var filterId in batch.FilterIds)
            {
                var filter = filters.FirstOrDefault(f => f.Id == filterId);
                if (filter != null)
                {
                    totalTime += image.EstimateProcessingTime(filter);
                }
            }
        }

        return totalTime;
    }

    /// <summary>
    /// Gets memory requirements for batch.
    /// </summary>
    /// <param name="batch">The image batch to analyze.</param>
    /// <param name="images">The collection of images.</param>
    /// <returns>Maximum memory requirement in bytes across all images in the batch.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static long GetMemoryRequirements(this ImageBatch batch, IEnumerable<Image> images)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(images);

        var maxImageMemory = 0L;

        foreach (var imageId in batch.ImageIds)
        {
            var image = images.FirstOrDefault(i => i.Id == imageId);
            if (image != null)
            {
                var memory = image.GetMemoryRequirement();
                if (memory > maxImageMemory)
                    maxImageMemory = memory;
            }
        }

        return maxImageMemory;
    }
}

/// <summary>
/// Provides extension methods for performance metrics analysis.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Checks if metrics indicate slowdown.
    /// </summary>
    /// <param name="metrics">The performance metrics to analyze.</param>
    /// <returns>True if slowdown is detected; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static bool IsSlowdownDetected(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.AverageExecutionTimeMs > AppConstants.Performance.SlowOperationThresholdMs ||
               metrics.FailedOperationsCount > metrics.TotalOperationsCount * 0.1;
    }

    /// <summary>
    /// Gets performance grade A-F.
    /// </summary>
    /// <param name="metrics">The performance metrics to grade.</param>
    /// <returns>A character grade from A to F.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static char GetPerformanceGrade(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var successRate = metrics.GetSuccessRate();
        var throughputScore = Math.Min(100, metrics.ThroughputMegabytesPerSecond);

        var score = (successRate * 0.7) + (throughputScore * 0.3);

        return score switch
        {
            >= 90 => 'A',
            >= 80 => 'B',
            >= 70 => 'C',
            >= 60 => 'D',
            >= 50 => 'E',
            _ => 'F'
        };
    }
}