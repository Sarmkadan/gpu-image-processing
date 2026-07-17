using System;
using System.Globalization;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Provides extension methods for <see cref="MetricsAggregationWorker"/>.
    /// </summary>
    public static class MetricsAggregationWorkerExtensions
    {
        /// <summary>
        /// Gets a formatted string representation of the metrics summary.
        /// </summary>
        /// <param name="worker">The <see cref="MetricsAggregationWorker"/> instance.</param>
        /// <param name="minutes">The time period in minutes to analyze.</param>
        /// <returns>A formatted summary string, or a message indicating no data is available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <see langword="null"/>.</exception>
        public static string ToFormattedSummaryString(this MetricsAggregationWorker worker, int minutes = 10)
        {
            ArgumentNullException.ThrowIfNull(worker);

            return worker.GetMetricsSummary(minutes) switch
            {
                null => "No metrics data available.",
                var summary => string.Format(
                    CultureInfo.InvariantCulture,
                    "Period: {0}min, Snapshots: {1}, AvgMem: {2:F2}MB, MaxMem: {3:F2}MB, AvgLatency: {4:F2}ms, AvgSuccessRate: {5:F1}%",
                    summary.PeriodMinutes,
                    summary.SnapshotCount,
                    summary.AvgMemoryMb,
                    summary.MaxMemoryMb,
                    summary.AvgLatencyMs,
                    summary.AvgSuccessRate)
            };
        }

        /// <summary>
        /// Calculates the duration of the metrics aggregation period.
        /// </summary>
        /// <param name="worker">The <see cref="MetricsAggregationWorker"/> instance.</param>
        /// <param name="minutes">The time period in minutes to analyze.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the duration, or <see cref="TimeSpan.Zero"/> if no data is available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <see langword="null"/>.</exception>
        public static TimeSpan GetAggregationDuration(this MetricsAggregationWorker worker, int minutes = 10)
        {
            ArgumentNullException.ThrowIfNull(worker);

            return worker.GetMetricsSummary(minutes) switch
            {
                null => TimeSpan.Zero,
                var summary => summary.EndTime - summary.StartTime
            };
        }

        /// <summary>
        /// Checks if the average success rate is above the specified threshold.
        /// </summary>
        /// <param name="worker">The <see cref="MetricsAggregationWorker"/> instance.</param>
        /// <param name="threshold">The success rate threshold percentage (default: 95%).</param>
        /// <param name="minutes">The time period in minutes to analyze.</param>
        /// <returns>True if the success rate is above the threshold; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="worker"/> is <see langword="null"/>.</exception>
        public static bool IsSuccessRateHealthy(this MetricsAggregationWorker worker, double threshold = 95.0, int minutes = 10)
        {
            ArgumentNullException.ThrowIfNull(worker);

            return worker.GetMetricsSummary(minutes) is { } summary && summary.AvgSuccessRate >= threshold;
        }
    }
}