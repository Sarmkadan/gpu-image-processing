#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Services;

/// <summary>
/// Main service for orchestrating image processing operations.
/// </summary>
public class ImageProcessingService
{
    private readonly ImageRepository _imageRepository;
    private readonly FilterConfigurationRepository _filterRepository;
    private readonly ProcessingResultRepository _resultRepository;
    private readonly FilterService _filterService;
    private readonly GpuManagementService _gpuService;
    private readonly PerformanceMonitoringService _performanceService;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(
        ImageRepository imageRepository,
        FilterConfigurationRepository filterRepository,
        ProcessingResultRepository resultRepository,
        FilterService filterService,
        GpuManagementService gpuService,
        PerformanceMonitoringService performanceService,
        ILogger<ImageProcessingService> logger)
    {
        _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        _filterRepository = filterRepository ?? throw new ArgumentNullException(nameof(filterRepository));
        _resultRepository = resultRepository ?? throw new ArgumentNullException(nameof(resultRepository));
        _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService));
        _gpuService = gpuService ?? throw new ArgumentNullException(nameof(gpuService));
        _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a single image by sequentially applying the specified GPU filters.
    /// Allocates GPU memory, applies each filter in order, persists the processed output,
    /// and records performance metrics. On failure, marks the image as failed and stores
    /// a failed result record for diagnostics.
    /// </summary>
    /// <param name="imageId">The unique identifier of the image to process.</param>
    /// <param name="filterIds">Ordered list of filter configuration IDs to apply sequentially.</param>
    /// <param name="cancellationToken">Token to cancel the processing operation.</param>
    /// <returns>A <see cref="ProcessingResult"/> with per-filter timings and the output file path.</returns>
    /// <exception cref="InvalidImageException">Thrown when the image is not found or fails validation.</exception>
    /// <exception cref="GpuException">Thrown when no GPU device is available.</exception>
    /// <exception cref="ProcessingException">Thrown when any filter application or I/O step fails.</exception>
    public async Task<ProcessingResult> ProcessImageAsync(Guid imageId, List<Guid> filterIds, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var image = await _imageRepository.GetByIdAsync(imageId, cancellationToken);
            if (image == null)
                throw new InvalidImageException($"Image {imageId} not found");

            if (!image.Validate())
                throw new InvalidImageException("Image validation failed", image.FilePath);

            var result = new ProcessingResult { ImageId = imageId };
            image.MarkAsProcessing();
            await _imageRepository.UpdateAsync(image, cancellationToken);

            var device = _gpuService.GetBestDevice();
            if (device == null)
                throw new GpuException("No available GPU device found");

            var requiredMemory = image.CalculatePixelDataSize();
            _gpuService.AllocateMemory(requiredMemory, device.Id);

            foreach (var filterId in filterIds)
            {
                var filterStopwatch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    image = await _filterService.ApplyFilterAsync(image, filterId, cancellationToken);
                    filterStopwatch.Stop();

                    var filterConfig = await _filterRepository.GetByIdAsync(filterId, cancellationToken);
                    result.AddFilterApplied(filterConfig?.Name ?? "Unknown", filterConfig?.FilterType ?? FilterType.None, filterStopwatch.ElapsedMilliseconds);

                    _logger.LogDebug("Filter {FilterId} applied in {ElapsedMs}ms", filterId, filterStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying filter {FilterId}", filterId);
                    throw;
                }
            }

            var outputPath = Path.Combine(AppConstants.FileSystem.DefaultOutputDirectory, $"{image.Id}.processed.png");
            image.MarkAsCompleted(outputPath);
            await _imageRepository.UpdateAsync(image, cancellationToken);

            result.Complete(outputPath);
            await _resultRepository.CreateAsync(result, cancellationToken);

            _gpuService.DeallocateMemory(requiredMemory, device.Id);

            stopwatch.Stop();
            _performanceService.RecordOperation(stopwatch.ElapsedMilliseconds, true);

            _logger.LogInformation("Image {ImageId} processed successfully in {ElapsedMs}ms", imageId, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process image {ImageId}", imageId);

            var failedImage = await _imageRepository.GetByIdAsync(imageId, cancellationToken);
            if (failedImage != null)
            {
                failedImage.MarkAsFailed(ex.Message);
                await _imageRepository.UpdateAsync(failedImage, cancellationToken);
            }

            _performanceService.RecordOperation(stopwatch.ElapsedMilliseconds, false);

            var result = new ProcessingResult { ImageId = imageId };
            result.Fail(ex.Message, AppConstants.ErrorCodes.ProcessingTimeout);
            await _resultRepository.CreateAsync(result, cancellationToken);

            throw new ProcessingException($"Failed to process image", ex, imageId.ToString());
        }
    }

    /// <summary>
    /// Retrieves the most recent processing result for the specified image.
    /// </summary>
    /// <param name="imageId">The image identifier to look up results for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest <see cref="ProcessingResult"/>, or <c>null</c> if the image has not been processed.</returns>
    public async Task<ProcessingResult?> GetProcessingResultAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var results = await _resultRepository.GetByImageIdAsync(imageId, cancellationToken);
        return results.OrderByDescending(r => r.StartedAt).FirstOrDefault();
    }

    /// <summary>
    /// Computes aggregate image processing statistics including total/successful/failed counts,
    /// success rate percentage, and average/total processing time in milliseconds.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A dictionary with keys: TotalImages, ProcessedImages, SuccessfulProcessing,
    /// FailedProcessing, SuccessRate, AverageProcessingTime, TotalProcessingTime.
    /// </returns>
    public async Task<Dictionary<string, object>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var totalImages = await _imageRepository.CountAsync(cancellationToken);
        var results = (await _resultRepository.GetAllAsync(cancellationToken)).ToList();
        var successfulResults = results.Where(r => r.IsSuccessful).ToList();

        return new Dictionary<string, object>
        {
            { "TotalImages", totalImages },
            { "ProcessedImages", results.Count },
            { "SuccessfulProcessing", successfulResults.Count },
            { "FailedProcessing", results.Count - successfulResults.Count },
            { "SuccessRate", results.Count > 0 ? (successfulResults.Count / (double)results.Count * 100) : 0.0 },
            { "AverageProcessingTime", results.Any() ? results.Average(r => r.ProcessingTimeMilliseconds) : 0.0 },
            { "TotalProcessingTime", results.Sum(r => r.ProcessingTimeMilliseconds) }
        };
    }
}
