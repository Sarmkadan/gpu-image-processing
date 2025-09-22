using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="SystemPerformanceMonitoringService"/>.
    /// </summary>
    public static class SystemPerformanceMonitoringServiceExtensions
    {
        /// <summary>
        /// Gets the total count of measurements across all monitored operations.
        /// </summary>
        /// <param name="service">The <see cref="SystemPerformanceMonitoringService"/> instance.</param>
        /// <returns>The total count of measurements.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public static long GetTotalMeasurementCount(this SystemPerformanceMonitoringService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var stats = service.GetAllStatistics();
            return stats.Values.Sum(s => (long)s.TotalMeasurements);
        }

        /// <summary>
        /// Gets all operations sorted by their average latency in descending order.
        /// </summary>
        /// <param name="service">The <see cref="SystemPerformanceMonitoringService"/> instance.</param>
        /// <returns>A read-only list of <see cref="OperationStatistics"/> sorted by average latency.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public static IReadOnlyList<OperationStatistics> GetOperationsByAverageLatency(this SystemPerformanceMonitoringService service)
        {
            ArgumentNullException.ThrowIfNull(service);

            var stats = service.GetAllStatistics();
            return stats.Values.OrderByDescending(s => s.AverageMs).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets details for operations that have an average latency exceeding the specified threshold.
        /// </summary>
        /// <param name="service">The <see cref="SystemPerformanceMonitoringService"/> instance.</param>
        /// <param name="thresholdMs">The latency threshold in milliseconds.</param>
        /// <returns>A read-only list of <see cref="OperationStatistics"/> for slow operations.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="thresholdMs"/> is negative.</exception>
        public static IReadOnlyList<OperationStatistics> GetSlowOperationsDetails(this SystemPerformanceMonitoringService service, long thresholdMs)
        {
            ArgumentNullException.ThrowIfNull(service);
            if (thresholdMs < 0)
                throw new ArgumentException("Threshold must be non-negative.", nameof(thresholdMs));

            var stats = service.GetAllStatistics();
            return stats.Values
                .Where(s => s.AverageMs > thresholdMs)
                .OrderByDescending(s => s.AverageMs)
                .ToList()
                .AsReadOnly();
        }
    }
}
