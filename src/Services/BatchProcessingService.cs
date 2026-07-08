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
/// Service for handling batch image processing operations.
/// </summary>
public class BatchProcessingService
{
    private readonly ImageProcessingService _processingService;
    private readonly ImageRepository _imageRepository;
    private readonly ILogger<BatchProcessingService> _logger;
    private readonly Dictionary<Guid, ImageBatch> _activeBatches = new();
    private readonly SemaphoreSlim _concurrencySemaphore;

    public BatchProcessingService(
        ImageProcessingService processingService,
        ImageRepository imageRepository,
        ILogger<BatchProcessingService> logger)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _concurrencySemaphore = new SemaphoreSlim(AppConstants.Processing.MaxConcurrentOperations);
    }

    /// <summary>
    /// Processes a batch of images.
    /// </summary>
    public async Task<ImageBatch> ProcessBatchAsync(ImageBatch batch, CancellationToken cancellationToken = default)
    {
        if (batch == null)
            throw new ArgumentNullException(nameof(batch));

        if (!batch.Validate())
            throw new ProcessingException("Batch validation failed");

        _activeBatches[batch.Id] = batch;

        // A linked source lets us cancel all in-flight tasks when the caller
        // cancels, giving every task a chance to release GPU buffers cleanly.
        using var batchCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            batch.Start();
            _logger.LogInformation("Starting batch processing {BatchId} with {ImageCount} images",
                batch.Id, batch.TotalImages);

            Directory.CreateDirectory(batch.OutputDirectory);

            var tasks = batch.ImageIds
                .Select(imageId => ProcessImageInBatchAsync(batch, imageId, batchCts.Token))
                .ToList();

            await Task.WhenAll(tasks);

            batch.Complete();
            _logger.LogInformation("Batch {BatchId} completed: {Processed} processed, {Failed} failed",
                batch.Id, batch.ProcessedImages, batch.FailedImages);

            return batch;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller-requested cancellation — return partial results so the caller
            // can inspect how many images were processed before the abort.
            batch.Status = ProcessingStatus.Cancelled;
            _logger.LogInformation(
                "Batch {BatchId} cancelled by caller: {Processed} processed, {Failed} failed",
                batch.Id, batch.ProcessedImages, batch.FailedImages);
            return batch;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Internal cancellation triggered by batchCts — treat as a failure.
            _logger.LogError("Batch processing failed for {BatchId}", batch.Id);
            batch.Status = ProcessingStatus.Failed;
            throw new ProcessingException($"Batch {batch.Id} was aborted due to an internal error.");
        }
        catch (Exception ex)
        {
            // Cancel any tasks still waiting on the semaphore so GPU allocations
            // are freed before the exception propagates to the caller.
            await batchCts.CancelAsync();
            _logger.LogError(ex, "Batch processing failed for {BatchId}", batch.Id);
            batch.Status = ProcessingStatus.Failed;
            throw;
        }
        finally
        {
            _activeBatches.Remove(batch.Id);
        }
    }

    /// <summary>
    /// Gets the status of an active batch.
    /// </summary>
    public ImageBatch? GetBatchStatus(Guid batchId)
    {
        _activeBatches.TryGetValue(batchId, out var batch);
        return batch;
    }

    /// <summary>
    /// Cancels a batch processing operation.
    /// </summary>
    public bool CancelBatch(Guid batchId)
    {
        if (!_activeBatches.TryGetValue(batchId, out var batch))
            return false;

        batch.Status = ProcessingStatus.Cancelled;
        _logger.LogWarning("Batch {BatchId} has been cancelled", batchId);
        return true;
    }

    private async Task ProcessImageInBatchAsync(ImageBatch batch, Guid imageId, CancellationToken cancellationToken)
    {
        bool semaphoreAcquired = false;
        try
        {
            await _concurrencySemaphore.WaitAsync(cancellationToken);
            semaphoreAcquired = true;

            if (batch.Status == ProcessingStatus.Cancelled)
                return;

            _logger.LogDebug("Processing image {ImageId} in batch {BatchId}", imageId, batch.Id);

            try
            {
                var result = await _processingService.ProcessImageAsync(imageId, batch.FilterIds.ToList(), cancellationToken);

                if (result.IsSuccessful)
                {
                    batch.MarkImageProcessed(true);
                    _logger.LogDebug("Image {ImageId} processed successfully", imageId);
                }
                else
                {
                    batch.MarkImageProcessed(false);
                    _logger.LogWarning("Image {ImageId} processing failed: {Error}", imageId, result.ErrorMessage);
                }
            }
            catch (OperationCanceledException)
            {
                // Propagate cancellation without marking as a processing failure.
                throw;
            }
            catch (Exception ex)
            {
                batch.MarkImageProcessed(false);
                _logger.LogError(ex, "Error processing image {ImageId} in batch", imageId);
            }
        }
        finally
        {
            // Only release if the semaphore was successfully acquired; releasing
            // without a prior WaitAsync would incorrectly inflate the counter.
            if (semaphoreAcquired)
                _concurrencySemaphore.Release();
        }
    }

    /// <summary>
    /// Creates a batch from multiple images and filters.
    /// </summary>
    public async Task<ImageBatch> CreateBatchAsync(
        List<Guid> imageIds,
        List<Guid> filterIds,
        string batchName,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        if (!imageIds.Any() || !filterIds.Any())
            throw new ArgumentException("Images and filters are required");

        var batch = new ImageBatch
        {
            Name = batchName,
            OutputDirectory = outputDirectory,
            FilterIds = filterIds,
            TotalImages = imageIds.Count
        };

        foreach (var imageId in imageIds)
        {
            batch.AddImage(imageId);
        }

        foreach (var filterId in filterIds)
        {
            batch.AddFilter(filterId);
        }

        _logger.LogInformation("Batch {BatchId} created with {ImageCount} images and {FilterCount} filters",
            batch.Id, imageIds.Count, filterIds.Count);

        return batch;
    }

    /// <summary>
    /// Gets batch processing progress.
    /// </summary>
    public Dictionary<string, object> GetBatchProgress(Guid batchId)
    {
        var batch = GetBatchStatus(batchId);
        if (batch == null)
            return new Dictionary<string, object>();

        var estimatedTime = batch.GetEstimatedRemainingTime();
        return new Dictionary<string, object>
        {
            { "BatchId", batch.Id },
            { "Status", batch.Status },
            { "TotalImages", batch.TotalImages },
            { "ProcessedImages", batch.ProcessedImages },
            { "FailedImages", batch.FailedImages },
            { "ProgressPercent", batch.GetProgressPercentage() },
            { "SuccessRate", batch.GetSuccessRate() },
            { "EstimatedRemainingTime", estimatedTime ?? TimeSpan.Zero }
        };
    }

    /// <summary>
    /// Gets active batch count.
    /// </summary>
    public int GetActiveBatchCount()
    {
        return _activeBatches.Count;
    }

    /// <summary>
    /// Gets all active batches.
    /// </summary>
    public IEnumerable<ImageBatch> GetActiveBatches()
    {
        return _activeBatches.Values.ToList();
    }
}
