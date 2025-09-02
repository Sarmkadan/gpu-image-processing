using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Domain
{
    /// <summary>
    /// Extension methods for <see cref="PerformanceMetrics"/> to enhance performance analysis capabilities.
    /// </summary>
    public static class PerformanceMetricsExtensions
    {
        /// <summary>
        /// Calculates the average execution time from recorded execution times.
        /// </summary>
        /// <param name="metrics">The performance metrics instance.</param>
        /// <returns>The average execution time in milliseconds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no execution times are recorded.</exception>
        public static double CalculateAverageExecutionTime(this PerformanceMetrics metrics)
        {
            ArgumentNullException.ThrowIfNull(metrics);
            if (metrics.ExecutionTimes.Count == 0)
                throw new InvalidOperationException("No execution times recorded to calculate average.");

            return metrics.ExecutionTimes.Average();
        }

        /// <summary>
        /// Determines if GPU utilization is below a critical threshold.
        /// </summary>
        /// <param name="metrics">The performance metrics instance.</param>
        /// <param name="criticalThresholdPercent">The GPU utilization threshold percentage (0-100).</param>
        /// <returns>True if GPU utilization is below the critical threshold.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="criticalThresholdPercent"/> is outside 0-100.</exception>
        public static bool IsGpuUnderutilized(this PerformanceMetrics metrics, double criticalThresholdPercent)
        {
            ArgumentNullException.ThrowIfNull(metrics);
            if (criticalThresholdPercent < 0 || criticalThresholdPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(criticalThresholdPercent), "Value must be between 0 and 100.");

            return metrics.GpuUtilizationPercent < criticalThresholdPercent;
        }

        /// <summary>
        /// Checks if throughput has dropped below a baseline threshold.
        /// </summary>
        /// <param name="metrics">The performance metrics instance.</param>
        /// <param name="baselineThroughputMbps">The baseline throughput threshold in Mbps.</param>
        /// <returns>True if throughput is below the baseline.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="baselineThroughputMbps"/> is negative.</exception>
        public static bool HasThroughputDropped(this PerformanceMetrics metrics, double baselineThroughputMbps)
        {
            ArgumentNullException.ThrowIfNull(metrics);
            if (baselineThroughputMbps < 0)
                throw new ArgumentOutOfRangeException(nameof(baselineThroughputMbps), "Baseline throughput cannot be negative.");

            return metrics.ThroughputMegabytesPerSecond < baselineThroughputMbps;
        }
    }
}
