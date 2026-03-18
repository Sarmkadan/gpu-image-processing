#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Domain;

/// <summary>
/// Represents the result of an image processing operation.
/// </summary>
public class ProcessingResult
{
    public Guid Id { get; set; }
    public Guid ImageId { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public long ProcessingTimeMilliseconds { get; set; }
    public string? ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public List<FilterApplied> FiltersApplied { get; set; } = [];
    public PerformanceMetrics Metrics { get; set; } = new();
    public Dictionary<string, object> ResultMetadata { get; set; } = new();
    public bool IsSuccessful { get; set; }

    public ProcessingResult()
    {
        Id = Guid.NewGuid();
        StartedAt = DateTime.UtcNow;
        Status = ProcessingStatus.Pending;
    }

    /// <summary>
    /// Completes the processing result.
    /// </summary>
    public void Complete(string outputPath)
    {
        CompletedAt = DateTime.UtcNow;
        OutputPath = outputPath;
        Status = ProcessingStatus.Completed;
        IsSuccessful = true;
        ProcessingTimeMilliseconds = (long)(CompletedAt - StartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Marks the processing as failed with error details.
    /// </summary>
    public void Fail(string errorMessage, int errorCode = 0)
    {
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Status = ProcessingStatus.Failed;
        IsSuccessful = false;
        ProcessingTimeMilliseconds = (long)(CompletedAt - StartedAt).TotalMilliseconds;
    }

    /// <summary>
    /// Records a filter application.
    /// </summary>
    public void AddFilterApplied(string filterName, FilterType filterType, double executionTimeMs)
    {
        FiltersApplied.Add(new FilterApplied
        {
            FilterName = filterName,
            FilterType = filterType,
            ExecutionTimeMilliseconds = executionTimeMs,
            AppliedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets total execution time for all applied filters.
    /// </summary>
    public double GetTotalFilterExecutionTime()
    {
        return FiltersApplied.Sum(f => f.ExecutionTimeMilliseconds);
    }
}

/// <summary>
/// Represents a filter applied during processing.
/// </summary>
public class FilterApplied
{
    public string FilterName { get; set; } = string.Empty;
    public FilterType FilterType { get; set; }
    public double ExecutionTimeMilliseconds { get; set; }
    public DateTime AppliedAt { get; set; }
}
