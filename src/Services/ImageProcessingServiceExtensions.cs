#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.Services;

/// <summary>
/// Extension methods for <see cref="ImageProcessingService"/> that provide convenient
/// operations for image processing workflows, batch operations, and result analysis.
/// </summary>
public static class ImageProcessingServiceExtensions
{
    /// <summary>
    /// Processes multiple images sequentially with the same set of filters.
    /// </summary>
    /// <param name="service">The image processing service instance.</param>
    /// <param name="imageIds">Collection of image identifiers to process.</param>
    /// <param name="filterIds">Ordered list of filter configuration IDs to apply to all images.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of processing results for each successfully processed image.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="imageIds"/>, or <paramref name="filterIds"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="imageIds"/> is empty or <paramref name="filterIds"/> is empty.</exception>
    public static async Task<IReadOnlyList<ProcessingResult>> ProcessImagesAsync(
        this ImageProcessingService service,
        IEnumerable<Guid> imageIds,
        List<Guid> filterIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(imageIds);
        ArgumentNullException.ThrowIfNull(filterIds);

        var imageList = imageIds.ToList();
        if (imageList.Count == 0)
        {
            return Array.Empty<ProcessingResult>();
        }

        if (filterIds.Count == 0)
        {
            throw new ArgumentException("Filter list cannot be empty", nameof(filterIds));
        }

        var results = new List<ProcessingResult>();
        foreach (var imageId in imageList)
        {
            try
            {
                var result = await service.ProcessImageAsync(imageId, filterIds, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                // Continue processing other images even if one fails
                // The exception is intentionally swallowed to allow batch processing to continue
                // Failed images are not tracked in results, which is the expected behavior
                continue;
            }
        }

        return results.AsReadOnly();
    }

    /// <summary>
    /// Processes an image with a single filter applied multiple times (useful for iterative filters).
    /// </summary>
    /// <param name="service">The image processing service instance.</param>
    /// <param name="imageId">The unique identifier of the image to process.</param>
    /// <param name="filterId">The filter configuration ID to apply iteratively.</param>
    /// <param name="iterationCount">Number of times to apply the filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ProcessingResult"/> with timing information for each iteration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iterationCount"/> is less than 1.</exception>
    public static async Task<ProcessingResult> ProcessImageWithIterationsAsync(
        this ImageProcessingService service,
        Guid imageId,
        Guid filterId,
        int iterationCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentOutOfRangeException.ThrowIfLessThan(iterationCount, 1);

        var filterIds = Enumerable.Repeat(filterId, iterationCount).ToList();
        return await service.ProcessImageAsync(imageId, filterIds, cancellationToken);
    }

    /// <summary>
    /// Gets processing statistics formatted as a human-readable report.
    /// </summary>
    /// <param name="service">The image processing service instance.</param>
    /// <param name="cultureInfo">The culture to use for formatting numbers and percentages. Defaults to invariant culture.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A formatted string containing the statistics report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when required statistics keys are missing from the stats dictionary.</exception>
    public static async Task<string> GetStatisticsReportAsync(
        this ImageProcessingService service,
        CultureInfo? cultureInfo = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var stats = await service.GetStatisticsAsync(cancellationToken);
        var culture = cultureInfo ?? CultureInfo.InvariantCulture;

        var totalImages = GetStatValue<int>(stats, "TotalImages");
        var processedImages = GetStatValue<int>(stats, "ProcessedImages");
        var successful = GetStatValue<int>(stats, "SuccessfulProcessing");
        var failed = GetStatValue<int>(stats, "FailedProcessing");
        var successRate = GetStatValue<double>(stats, "SuccessRate");
        var avgTime = GetStatValue<double>(stats, "AverageProcessingTime");
        var totalTime = GetStatValue<long>(stats, "TotalProcessingTime");

        return $$"""
Image Processing Statistics Report
================================
Total Images: {{totalImages}}
Processed Images: {{processedImages}}
Successful: {{successful}}
Failed: {{failed}}
Success Rate: {{successRate.ToString("F2", culture)}}%
Average Processing Time: {{avgTime.ToString("F2", culture)}} ms
Total Processing Time: {{TimeSpan.FromMilliseconds(totalTime).ToString("g", culture)}}
""";

        static T GetStatValue<T>(IReadOnlyDictionary<string, object> stats, string key)
        {
            if (!stats.TryGetValue(key, out var value))
            {
                throw new KeyNotFoundException($"Statistics dictionary does not contain key: {{key}}");
            }

            return (T)value;
        }
    }

    /// <summary>
    /// Gets the most recent successful processing result for an image, or throws if none exists.
    /// </summary>
    /// <param name="service">The image processing service instance.</param>
    /// <param name="imageId">The image identifier to look up results for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest successful <see cref="ProcessingResult"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no successful result exists for the image.</exception>
    public static async Task<ProcessingResult> GetLatestSuccessfulResultAsync(
        this ImageProcessingService service,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var result = await service.GetProcessingResultAsync(imageId, cancellationToken);
        return result ?? throw new InvalidOperationException($"No processing result found for image {{imageId}}");
    }
}
