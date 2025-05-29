#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Service for tracking and analyzing system performance metrics.
    /// Collects throughput, latency, and resource utilization statistics.
    /// </summary>
    public class SystemPerformanceMonitoringService
    {
        private readonly ILogger<SystemPerformanceMonitoringService> _logger;
        private readonly Dictionary<string, PerformanceMetric> _metrics;
        private readonly object _lockObject = new object();

        public SystemPerformanceMonitoringService(ILogger<SystemPerformanceMonitoringService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = new Dictionary<string, PerformanceMetric>();
        }

        /// <summary>
        /// Records a performance measurement for a named operation.
        /// </summary>
        public void RecordMetric(string operationName, long durationMs)
        {
            if (string.IsNullOrWhiteSpace(operationName) || durationMs < 0)
                return;

            lock (_lockObject)
            {
                if (!_metrics.TryGetValue(operationName, out var metric))
                {
                    metric = new PerformanceMetric { OperationName = operationName };
                    _metrics[operationName] = metric;
                }

                metric.RecordMeasurement(durationMs);

                _logger.LogDebug(
                    "Metric recorded - Operation: {Operation}, Duration: {DurationMs}ms, Count: {Count}",
                    operationName,
                    durationMs,
                    metric.MeasurementCount);
            }
        }

        /// <summary>
        /// Gets performance statistics for a specific operation.
        /// </summary>
        public OperationStatistics GetStatistics(string operationName)
        {
            lock (_lockObject)
            {
                if (!_metrics.TryGetValue(operationName, out var metric))
                {
                    return null;
                }

                return metric.GetStatistics();
            }
        }

        /// <summary>
        /// Gets statistics for all monitored operations.
        /// </summary>
        public Dictionary<string, OperationStatistics> GetAllStatistics()
        {
            var result = new Dictionary<string, OperationStatistics>();

            lock (_lockObject)
            {
                foreach (var kvp in _metrics)
                {
                    result[kvp.Key] = kvp.Value.GetStatistics();
                }
            }

            return result;
        }

        /// <summary>
        /// Resets statistics for a specific operation.
        /// </summary>
        public bool ResetStatistics(string operationName)
        {
            lock (_lockObject)
            {
                if (_metrics.TryGetValue(operationName, out var metric))
                {
                    metric.Reset();
                    _logger.LogInformation("Statistics reset - Operation: {Operation}", operationName);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Resets all collected statistics.
        /// </summary>
        public void ResetAll()
        {
            lock (_lockObject)
            {
                foreach (var metric in _metrics.Values)
                {
                    metric.Reset();
                }

                _logger.LogInformation("All statistics reset");
            }
        }

        /// <summary>
        /// Gets the count of monitored operations.
        /// </summary>
        public int GetMonitoredOperationCount()
        {
            lock (_lockObject)
            {
                return _metrics.Count;
            }
        }

        /// <summary>
        /// Identifies performance bottlenecks based on threshold.
        /// </summary>
        public List<string> GetSlowOperations(long thresholdMs)
        {
            var slowOps = new List<string>();

            lock (_lockObject)
            {
                foreach (var kvp in _metrics)
                {
                    var stats = kvp.Value.GetStatistics();
                    if (stats.AverageMs > thresholdMs)
                    {
                        slowOps.Add(kvp.Key);
                    }
                }
            }

            return slowOps;
        }
    }

    /// <summary>
    /// Individual performance metric tracker.
    /// </summary>
    internal class PerformanceMetric
    {
        public string OperationName { get; set; }
        public List<long> Measurements { get; private set; }
        public int MeasurementCount => Measurements.Count;

        public PerformanceMetric()
        {
            Measurements = new List<long>();
        }

        public void RecordMeasurement(long durationMs)
        {
            Measurements.Add(durationMs);

            // Keep only last 10000 measurements to prevent unbounded growth
            if (Measurements.Count > 10000)
            {
                Measurements.RemoveRange(0, Measurements.Count - 10000);
            }
        }

        public OperationStatistics GetStatistics()
        {
            if (Measurements.Count == 0)
                return new OperationStatistics { OperationName = OperationName };

            var stats = new OperationStatistics
            {
                OperationName = OperationName,
                TotalMeasurements = Measurements.Count,
                MinMs = Measurements.Min(),
                MaxMs = Measurements.Max(),
                AverageMs = (long)Measurements.Average(),
                MedianMs = CalculateMedian(Measurements),
                P95Ms = CalculatePercentile(Measurements, 0.95),
                P99Ms = CalculatePercentile(Measurements, 0.99),
                ThroughputPerSecond = CalculateThroughput()
            };

            return stats;
        }

        public void Reset()
        {
            Measurements.Clear();
        }

        private long CalculateMedian(List<long> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            else
                return sorted[count / 2];
        }

        private long CalculatePercentile(List<long> values, double percentile)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int index = (int)Math.Ceiling((percentile / 100.0) * sorted.Count) - 1;
            return sorted[Math.Max(0, index)];
        }

        private double CalculateThroughput()
        {
            if (Measurements.Count == 0)
                return 0;

            long totalMs = Measurements.Sum();
            if (totalMs == 0)
                return 0;

            return (Measurements.Count / (totalMs / 1000.0));
        }
    }

    /// <summary>
    /// Statistics for a single operation.
    /// </summary>
    public class OperationStatistics
    {
        public string OperationName { get; set; }
        public int TotalMeasurements { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
        public long AverageMs { get; set; }
        public long MedianMs { get; set; }
        public long P95Ms { get; set; }
        public long P99Ms { get; set; }
        public double ThroughputPerSecond { get; set; }
    }
}
