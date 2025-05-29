#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GpuImageProcessing.Core;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Tracks performance metrics for GPU operations.
/// </summary>
public class PerformanceMetrics
{
    public Guid Id { get; set; }
    public DateTime RecordedAt { get; set; }
    public double CpuUsagePercent { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long GpuMemoryUsedBytes { get; set; }
    public double GpuUtilizationPercent { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public double MaxExecutionTimeMs { get; set; }
    public double MinExecutionTimeMs { get; set; }
    public long ImagePixelsProcessedPerSecond { get; set; }
    public int TotalOperationsCount { get; set; }
    public int FailedOperationsCount { get; set; }
    public double ThroughputMegabytesPerSecond { get; set; }
    public List<double> ExecutionTimes { get; set; } = [];

    public PerformanceMetrics()
    {
        Id = Guid.NewGuid();
        RecordedAt = DateTime.UtcNow;
        MinExecutionTimeMs = double.MaxValue;
    }

    /// <summary>
    /// Records an operation execution time.
    /// </summary>
    public void RecordExecution(double executionTimeMs)
    {
        ExecutionTimes.Add(executionTimeMs);
        UpdateStatistics();
    }

    /// <summary>
    /// Updates aggregated statistics from execution times.
    /// </summary>
    private void UpdateStatistics()
    {
        if (ExecutionTimes.Count == 0)
            return;

        AverageExecutionTimeMs = ExecutionTimes.Average();
        MaxExecutionTimeMs = ExecutionTimes.Max();
        MinExecutionTimeMs = ExecutionTimes.Min();
        TotalOperationsCount = ExecutionTimes.Count;
    }

    /// <summary>
    /// Calculates success rate percentage.
    /// </summary>
    public double GetSuccessRate()
    {
        if (TotalOperationsCount == 0)
            return 0.0;

        return ((TotalOperationsCount - FailedOperationsCount) / (double)TotalOperationsCount) * 100.0;
    }

    /// <summary>
    /// Checks if GPU memory usage is above warning threshold.
    /// </summary>
    public bool IsMemoryWarningRequired()
    {
        return GpuMemoryUsedBytes >= AppConstants.Memory.MemoryWarningThreshold;
    }

    /// <summary>
    /// Gets memory usage percentage.
    /// </summary>
    public double GetMemoryUsagePercent()
    {
        return (GpuMemoryUsedBytes / (double)AppConstants.Memory.MaxTotalGpuMemory) * 100.0;
    }

    /// <summary>
    /// Resets metrics for a new measurement period.
    /// </summary>
    public void Reset()
    {
        ExecutionTimes.Clear();
        TotalOperationsCount = 0;
        FailedOperationsCount = 0;
        AverageExecutionTimeMs = 0.0;
        MaxExecutionTimeMs = 0.0;
        MinExecutionTimeMs = double.MaxValue;
        RecordedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets a performance summary string.
    /// </summary>
    public override string ToString()
    {
        return $"Metrics [CPU: {CpuUsagePercent:F2}%, GPU: {GpuUtilizationPercent:F2}%, " +
               $"Avg Time: {AverageExecutionTimeMs:F2}ms, Throughput: {ThroughputMegabytesPerSecond:F2} MB/s, " +
               $"Success Rate: {GetSuccessRate():F2}%]";
    }
}
