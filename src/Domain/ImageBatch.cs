// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Domain;

/// <summary>
/// Represents a batch of images to be processed together.
/// </summary>
public class ImageBatch
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> ImageIds { get; set; } = [];
    public ProcessingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int TotalImages { get; set; }
    public int ProcessedImages { get; set; }
    public int FailedImages { get; set; }
    public List<Guid> FilterIds { get; set; } = [];
    public Dictionary<string, object> BatchOptions { get; set; } = new();
    public string OutputDirectory { get; set; } = string.Empty;
    public PerformanceMetrics Metrics { get; set; } = new();

    public ImageBatch()
    {
        Id = Guid.NewGuid();
        Status = ProcessingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an image to the batch.
    /// </summary>
    public bool AddImage(Guid imageId)
    {
        if (ImageIds.Contains(imageId))
            return false;

        if (ImageIds.Count >= Constants.Processing.MaxBatchSize)
            return false;

        ImageIds.Add(imageId);
        TotalImages = ImageIds.Count;
        return true;
    }

    /// <summary>
    /// Removes an image from the batch.
    /// </summary>
    public bool RemoveImage(Guid imageId)
    {
        var removed = ImageIds.Remove(imageId);
        if (removed)
            TotalImages = ImageIds.Count;
        return removed;
    }

    /// <summary>
    /// Adds a filter to the processing pipeline.
    /// </summary>
    public bool AddFilter(Guid filterId)
    {
        if (FilterIds.Contains(filterId))
            return false;

        FilterIds.Add(filterId);
        return true;
    }

    /// <summary>
    /// Starts batch processing.
    /// </summary>
    public void Start()
    {
        Status = ProcessingStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes batch processing.
    /// </summary>
    public void Complete()
    {
        Status = ProcessingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records image processing completion.
    /// </summary>
    public void MarkImageProcessed(bool success = true)
    {
        if (success)
            ProcessedImages++;
        else
            FailedImages++;
    }

    /// <summary>
    /// Gets the progress percentage.
    /// </summary>
    public double GetProgressPercentage()
    {
        if (TotalImages == 0)
            return 0.0;

        return ((ProcessedImages + FailedImages) / (double)TotalImages) * 100.0;
    }

    /// <summary>
    /// Gets the success rate.
    /// </summary>
    public double GetSuccessRate()
    {
        var totalProcessed = ProcessedImages + FailedImages;
        if (totalProcessed == 0)
            return 0.0;

        return (ProcessedImages / (double)totalProcessed) * 100.0;
    }

    /// <summary>
    /// Validates batch before processing.
    /// </summary>
    public bool Validate()
    {
        if (ImageIds.Count == 0)
            return false;

        if (FilterIds.Count == 0)
            return false;

        if (string.IsNullOrWhiteSpace(OutputDirectory))
            return false;

        return true;
    }

    /// <summary>
    /// Gets estimated remaining time based on current progress.
    /// </summary>
    public TimeSpan? GetEstimatedRemainingTime()
    {
        if (Status != ProcessingStatus.Processing || ProcessedImages == 0)
            return null;

        var elapsed = DateTime.UtcNow - StartedAt;
        var avgTimePerImage = elapsed.TotalSeconds / ProcessedImages;
        var remainingImages = TotalImages - ProcessedImages - FailedImages;
        var estimatedSeconds = avgTimePerImage * remainingImages;

        return TimeSpan.FromSeconds(estimatedSeconds);
    }
}
