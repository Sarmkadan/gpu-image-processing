// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Utilities;

/// <summary>
/// Extension methods for image processing operations.
/// </summary>
public static class ImageProcessingExtensions
{
    /// <summary>
    /// Gets appropriate color space for format.
    /// </summary>
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
            _ => ColorSpace.Unknown
        };
    }

    /// <summary>
    /// Calculates total bytes for image data.
    /// </summary>
    public static long CalculateTotalBytes(this Image image)
    {
        return image.CalculatePixelDataSize();
    }

    /// <summary>
    /// Checks if image resolution is within acceptable limits.
    /// </summary>
    public static bool IsResolutionValid(this Image image)
    {
        return image.Width >= Constants.Processing.MinImageWidth &&
               image.Width <= Constants.Processing.MaxImageWidth &&
               image.Height >= Constants.Processing.MinImageHeight &&
               image.Height <= Constants.Processing.MaxImageHeight;
    }

    /// <summary>
    /// Gets image file extension from format.
    /// </summary>
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
            _ => ".bin"
        };
    }

    /// <summary>
    /// Gets image format from file extension.
    /// </summary>
    public static ImageFormat GetFormatFromExtension(this string extension)
    {
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
    public static bool CanApplyFilter(this Image image, FilterType filterType)
    {
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
    public static string GetAspectRatioDescription(this Image image)
    {
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
    public static double EstimateProcessingTime(this Image image, FilterConfiguration filter)
    {
        var pixelCount = image.GetPixelCount();
        var baseTime = pixelCount / 1_000_000.0; // 1M pixels per ms baseline

        var multiplier = filter.FilterType switch
        {
            FilterType.Blur or FilterType.GaussianBlur => 2.5,
            FilterType.EdgeDetection or FilterType.Sobel => 2.0,
            FilterType.MedianFilter => 3.0,
            FilterType.Rotation => 1.5,
            FilterType.Scaling => 1.8,
            _ => 1.0
        };

        return baseTime * multiplier;
    }

    /// <summary>
    /// Gets memory requirement for processing.
    /// </summary>
    public static long GetMemoryRequirement(this Image image)
    {
        var pixelData = image.CalculatePixelDataSize();
        var workingMemory = pixelData * 2; // Working buffer
        return pixelData + workingMemory + (1024 * 1024); // Buffer + overhead
    }
}

/// <summary>
/// Extension methods for batch processing.
/// </summary>
public static class BatchProcessingExtensions
{
    /// <summary>
    /// Estimates total processing time for batch.
    /// </summary>
    public static double EstimateTotalTime(this ImageBatch batch, IEnumerable<Image> images, IEnumerable<FilterConfiguration> filters)
    {
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
    public static long GetMemoryRequirements(this ImageBatch batch, IEnumerable<Image> images)
    {
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
/// Extension methods for performance metrics.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Checks if metrics indicate slowdown.
    /// </summary>
    public static bool IsSlowdownDetected(this PerformanceMetrics metrics)
    {
        return metrics.AverageExecutionTimeMs > Constants.Performance.SlowOperationThresholdMs ||
               metrics.FailedOperationsCount > metrics.TotalOperationsCount * 0.1;
    }

    /// <summary>
    /// Gets performance grade A-F.
    /// </summary>
    public static char GetPerformanceGrade(this PerformanceMetrics metrics)
    {
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
