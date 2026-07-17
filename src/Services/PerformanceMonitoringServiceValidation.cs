#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Services;

/// <summary>
/// Validation helpers for <see cref="PerformanceMonitoringService"/>.
/// </summary>
public static class PerformanceMonitoringServiceValidation
{
    /// <summary>
    /// Validates a <see cref="PerformanceMonitoringService"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceMonitoringService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            var currentMetrics = value.GetCurrentMetrics();
            if (currentMetrics == null)
            {
                problems.Add("Current metrics are null");
            }
            else
            {
                ValidatePerformanceMetrics(currentMetrics, problems);
            }

            var history = value.GetMetricsHistory(limit: 1);
            if (history == null)
            {
                problems.Add("Metrics history enumeration is null");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Validation failed with exception: {ex.Message}");
        }

        return problems;
    }

    /// <summary>
    /// Validates a <see cref="PerformanceMetrics"/> instance.
    /// </summary>
    /// <param name="metrics">The metrics to validate.</param>
    /// <param name="problems">Collection to accumulate validation problems.</param>
    private static void ValidatePerformanceMetrics(PerformanceMetrics metrics, ICollection<string> problems)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(problems);

        if (metrics.CpuUsagePercent < 0 || metrics.CpuUsagePercent > 100)
        {
            problems.Add($"CPU usage percentage must be between 0 and 100, got {metrics.CpuUsagePercent:F2}%");
        }

        if (metrics.MemoryUsedBytes < 0)
        {
            problems.Add($"Memory used bytes must be non-negative, got {metrics.MemoryUsedBytes}");
        }

        if (metrics.GpuMemoryUsedBytes < 0)
        {
            problems.Add($"GPU memory used bytes must be non-negative, got {metrics.GpuMemoryUsedBytes}");
        }

        if (metrics.GpuUtilizationPercent < 0 || metrics.GpuUtilizationPercent > 100)
        {
            problems.Add($"GPU utilization percentage must be between 0 and 100, got {metrics.GpuUtilizationPercent:F2}%");
        }

        if (metrics.AverageExecutionTimeMs < 0)
        {
            problems.Add($"Average execution time must be non-negative, got {metrics.AverageExecutionTimeMs:F2}ms");
        }

        if (metrics.MaxExecutionTimeMs < 0)
        {
            problems.Add($"Max execution time must be non-negative, got {metrics.MaxExecutionTimeMs:F2}ms");
        }

        if (metrics.MinExecutionTimeMs < 0)
        {
            problems.Add($"Min execution time must be non-negative, got {metrics.MinExecutionTimeMs:F2}ms");
        }

        if (metrics.TotalOperationsCount < 0)
        {
            problems.Add($"Total operations count must be non-negative, got {metrics.TotalOperationsCount}");
        }

        if (metrics.FailedOperationsCount < 0)
        {
            problems.Add($"Failed operations count must be non-negative, got {metrics.FailedOperationsCount}");
        }

        if (metrics.FailedOperationsCount > metrics.TotalOperationsCount)
        {
            problems.Add($"Failed operations count ({metrics.FailedOperationsCount}) cannot exceed total operations count ({metrics.TotalOperationsCount })");
        }

        if (metrics.ThroughputMegabytesPerSecond < 0)
        {
            problems.Add($"Throughput must be non-negative, got {metrics.ThroughputMegabytesPerSecond:F2} MB/s");
        }

        if (metrics.ImagePixelsProcessedPerSecond < 0)
        {
            problems.Add($"Pixels processed per second must be non-negative, got {metrics.ImagePixelsProcessedPerSecond}");
        }

        if (metrics.ExecutionTimes == null)
        {
            problems.Add("Execution times collection is null");
        }
        else if (metrics.ExecutionTimes.Count > 0)
        {
            foreach (var executionTime in metrics.ExecutionTimes)
            {
                if (executionTime < 0)
                {
                    problems.Add($"Execution time must be non-negative, got {executionTime:F2}ms");
                    break;
                }
            }
        }

        if (metrics.RecordedAt > DateTime.UtcNow.AddMinutes(1))
        {
            problems.Add($"Recorded timestamp is in the future, got {metrics.RecordedAt:O}");
        }

        if (metrics.RecordedAt < DateTime.UtcNow.AddDays(-365))
        {
            problems.Add($"Recorded timestamp is too old, got {metrics.RecordedAt:O}");
        }
    }

    /// <summary>
    /// Checks if a <see cref="PerformanceMonitoringService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PerformanceMonitoringService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a <see cref="PerformanceMonitoringService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <exception cref="ArgumentException">If the instance is not valid.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this PerformanceMonitoringService value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid PerformanceMonitoringService instance:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}", nameof(value));
        }
    }
}
