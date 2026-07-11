#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Provides validation helpers for <see cref="MetricsSummary"/> instances.
    /// </summary>
    public static class MetricsAggregationWorkerValidation
    {
        /// <summary>
        /// Validates the specified <see cref="MetricsSummary"/> instance.
        /// </summary>
        /// <param name="value">The metrics summary to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MetricsSummary value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate PeriodMinutes
            if (value.PeriodMinutes <= 0)
            {
                errors.Add("PeriodMinutes must be a positive integer.");
            }

            // Validate SnapshotCount
            if (value.SnapshotCount < 0)
            {
                errors.Add("SnapshotCount must be non-negative.");
            }

            // Validate AvgMemoryMb
            if (value.AvgMemoryMb < 0)
            {
                errors.Add("AvgMemoryMb must be non-negative.");
            }

            // Validate MaxMemoryMb
            if (value.MaxMemoryMb < 0)
            {
                errors.Add("MaxMemoryMb must be non-negative.");
            }

            // Validate AvgLatencyMs
            if (value.AvgLatencyMs < 0)
            {
                errors.Add("AvgLatencyMs must be non-negative.");
            }

            // Validate MaxLatencyMs
            if (value.MaxLatencyMs < 0)
            {
                errors.Add("MaxLatencyMs must be non-negative.");
            }

            // Validate AvgSuccessRate
            if (value.AvgSuccessRate < 0 || value.AvgSuccessRate > 100)
            {
                errors.Add("AvgSuccessRate must be between 0 and 100.");
            }

            // Validate StartTime
            if (value.StartTime == default)
            {
                errors.Add("StartTime must be set to a non-default DateTime value.");
            }

            // Validate EndTime
            if (value.EndTime == default)
            {
                errors.Add("EndTime must be set to a non-default DateTime value.");
            }

            // Validate EndTime is after StartTime
            if (value.EndTime < value.StartTime)
            {
                errors.Add("EndTime must be equal to or after StartTime.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="MetricsSummary"/> is valid.
        /// </summary>
        /// <param name="value">The metrics summary to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this MetricsSummary value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="MetricsSummary"/> is valid.
        /// </summary>
        /// <param name="value">The metrics summary to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of validation errors.</exception>
        public static void EnsureValid(this MetricsSummary value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"MetricsSummary is not valid:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", errors)}");
            }
        }
    }
}
