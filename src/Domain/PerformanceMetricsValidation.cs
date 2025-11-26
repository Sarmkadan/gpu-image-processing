#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides validation helpers for <see cref="PerformanceMetrics"/> instances.
/// </summary>
public static class PerformanceMetricsValidation
{
    private const double MinValidPercentage = 0.0;
    private const double MaxValidPercentage = 100.0;
    private const long MinValidMemoryBytes = 0;
    private const double MinValidExecutionTimeMs = 0.0;
    private const int MinValidCount = 0;
    private const double MinValidThroughput = 0.0;

    /// <summary>
    /// Validates a <see cref="PerformanceMetrics"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The metrics instance to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceMetrics? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id must be a non-empty GUID.");
        }

        // Validate RecordedAt
        if (value.RecordedAt == default)
        {
            errors.Add("RecordedAt must be set to a valid DateTime.");
        }
        else if (value.RecordedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("RecordedAt must be in UTC timezone.");
        }

        // Validate CpuUsagePercent
        if (double.IsNaN(value.CpuUsagePercent) || double.IsInfinity(value.CpuUsagePercent))
        {
            errors.Add("CpuUsagePercent must be a valid number.");
        }
        else if (value.CpuUsagePercent < MinValidPercentage || value.CpuUsagePercent > MaxValidPercentage)
        {
            errors.Add($"CpuUsagePercent must be between {MinValidPercentage} and {MaxValidPercentage} (inclusive).");
        }

        // Validate MemoryUsedBytes
        if (value.MemoryUsedBytes < MinValidMemoryBytes)
        {
            errors.Add($"MemoryUsedBytes must be non-negative (>= {MinValidMemoryBytes}).");
        }

        // Validate GpuMemoryUsedBytes
        if (value.GpuMemoryUsedBytes < MinValidMemoryBytes)
        {
            errors.Add($"GpuMemoryUsedBytes must be non-negative (>= {MinValidMemoryBytes}).");
        }

        // Validate GpuUtilizationPercent
        if (double.IsNaN(value.GpuUtilizationPercent) || double.IsInfinity(value.GpuUtilizationPercent))
        {
            errors.Add("GpuUtilizationPercent must be a valid number.");
        }
        else if (value.GpuUtilizationPercent < MinValidPercentage || value.GpuUtilizationPercent > MaxValidPercentage)
        {
            errors.Add($"GpuUtilizationPercent must be between {MinValidPercentage} and {MaxValidPercentage} (inclusive).");
        }

        // Validate AverageExecutionTimeMs
        if (double.IsNaN(value.AverageExecutionTimeMs) || double.IsInfinity(value.AverageExecutionTimeMs))
        {
            errors.Add("AverageExecutionTimeMs must be a valid number.");
        }
        else if (value.AverageExecutionTimeMs < MinValidExecutionTimeMs)
        {
            errors.Add($"AverageExecutionTimeMs must be non-negative (>= {MinValidExecutionTimeMs}).");
        }

        // Validate MaxExecutionTimeMs
        if (double.IsNaN(value.MaxExecutionTimeMs) || double.IsInfinity(value.MaxExecutionTimeMs))
        {
            errors.Add("MaxExecutionTimeMs must be a valid number.");
        }
        else if (value.MaxExecutionTimeMs < MinValidExecutionTimeMs)
        {
            errors.Add($"MaxExecutionTimeMs must be non-negative (>= {MinValidExecutionTimeMs}).");
        }

        // Validate MinExecutionTimeMs
        if (double.IsNaN(value.MinExecutionTimeMs) || double.IsInfinity(value.MinExecutionTimeMs))
        {
            errors.Add("MinExecutionTimeMs must be a valid number.");
        }
        else if (value.MinExecutionTimeMs < MinValidExecutionTimeMs && value.MinExecutionTimeMs != double.MaxValue)
        {
            errors.Add($"MinExecutionTimeMs must be non-negative (>= {MinValidExecutionTimeMs}) or double.MaxValue for uninitialized state.");
        }

        // Validate ImagePixelsProcessedPerSecond
        if (value.ImagePixelsProcessedPerSecond < 0)
        {
            errors.Add("ImagePixelsProcessedPerSecond must be non-negative.");
        }

        // Validate TotalOperationsCount
        if (value.TotalOperationsCount < MinValidCount)
        {
            errors.Add($"TotalOperationsCount must be non-negative (>= {MinValidCount}).");
        }

        // Validate FailedOperationsCount
        if (value.FailedOperationsCount < 0)
        {
            errors.Add("FailedOperationsCount must be non-negative.");
        }
        else if (value.FailedOperationsCount > value.TotalOperationsCount)
        {
            errors.Add("FailedOperationsCount cannot exceed TotalOperationsCount.");
        }

        // Validate ThroughputMegabytesPerSecond
        if (double.IsNaN(value.ThroughputMegabytesPerSecond) || double.IsInfinity(value.ThroughputMegabytesPerSecond))
        {
            errors.Add("ThroughputMegabytesPerSecond must be a valid number.");
        }
        else if (value.ThroughputMegabytesPerSecond < MinValidThroughput)
        {
            errors.Add($"ThroughputMegabytesPerSecond must be non-negative (>= {MinValidThroughput}).");
        }

        // Validate ExecutionTimes list
        if (value.ExecutionTimes is null)
        {
            errors.Add("ExecutionTimes list cannot be null.");
        }
        else
        {
            for (int i = 0; i < value.ExecutionTimes.Count; i++)
            {
                double executionTime = value.ExecutionTimes[i];
                if (double.IsNaN(executionTime) || double.IsInfinity(executionTime))
                {
                    errors.Add($"ExecutionTimes[{i}] must be a valid number.");
                }
                else if (executionTime < MinValidExecutionTimeMs)
                {
                    errors.Add($"ExecutionTimes[{i}] must be non-negative (>= {MinValidExecutionTimeMs}).");
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PerformanceMetrics"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this PerformanceMetrics? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="PerformanceMetrics"/> instance is valid.
    /// </summary>
    /// <param name="value">The metrics instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this PerformanceMetrics? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"PerformanceMetrics validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", errors)
            }");
    }
}