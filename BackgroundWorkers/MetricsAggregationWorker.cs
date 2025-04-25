// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GpuImageProcessing.Services;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Background worker for aggregating and publishing performance metrics.
    /// Collects telemetry data and generates summary reports at regular intervals.
    /// </summary>
    public class MetricsAggregationWorker : BackgroundWorkerBase
    {
        private readonly TelemetryService _telemetryService;
        private readonly TimeSpan _aggregationInterval;
        private List<MetricsSnapshot> _snapshots;
        private readonly object _lockObject = new();

        public MetricsAggregationWorker(
            TelemetryService telemetryService,
            TimeSpan? aggregationInterval = null)
        {
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _aggregationInterval = aggregationInterval ?? TimeSpan.FromMinutes(1);
            _snapshots = new List<MetricsSnapshot>();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Collect system metrics
                    var systemStats = _telemetryService.GetSystemStats();
                    var snapshot = new MetricsSnapshot
                    {
                        Timestamp = systemStats.Timestamp,
                        ProcessId = systemStats.ProcessId,
                        MemoryUsageMb = systemStats.PrivateMemoryBytes / (1024.0 * 1024),
                        ThreadCount = systemStats.ThreadCount,
                        TotalOperations = systemStats.TotalOperations,
                        SuccessRate = systemStats.SuccessRate,
                        AverageLatencyMs = systemStats.AverageLatencyMs
                    };

                    lock (_lockObject)
                    {
                        _snapshots.Add(snapshot);

                        // Keep last 60 snapshots
                        if (_snapshots.Count > 60)
                            _snapshots.RemoveAt(0);
                    }

                    OnProgressUpdated($"Metrics snapshot: Memory={snapshot.MemoryUsageMb:F1}MB, " +
                                     $"Threads={snapshot.ThreadCount}, " +
                                     $"Success Rate={snapshot.SuccessRate:F1}%");

                    await Task.Delay(_aggregationInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnError($"Metrics aggregation error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets metrics summary for a time period
        /// </summary>
        public MetricsSummary GetMetricsSummary(int lastMinutes = 10)
        {
            lock (_lockObject)
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-lastMinutes);
                var relevantSnapshots = _snapshots.FindAll(s => s.Timestamp >= cutoffTime);

                if (relevantSnapshots.Count == 0)
                    return null;

                var avgMemory = 0.0;
                var avgLatency = 0.0;
                var avgSuccessRate = 0.0;
                var maxMemory = 0.0;
                var maxLatency = 0.0;

                foreach (var snapshot in relevantSnapshots)
                {
                    avgMemory += snapshot.MemoryUsageMb;
                    avgLatency += snapshot.AverageLatencyMs;
                    avgSuccessRate += snapshot.SuccessRate;
                    maxMemory = Math.Max(maxMemory, snapshot.MemoryUsageMb);
                    maxLatency = Math.Max(maxLatency, snapshot.AverageLatencyMs);
                }

                var count = relevantSnapshots.Count;
                return new MetricsSummary
                {
                    PeriodMinutes = lastMinutes,
                    SnapshotCount = count,
                    AvgMemoryMb = avgMemory / count,
                    MaxMemoryMb = maxMemory,
                    AvgLatencyMs = avgLatency / count,
                    MaxLatencyMs = maxLatency,
                    AvgSuccessRate = avgSuccessRate / count,
                    StartTime = relevantSnapshots[0].Timestamp,
                    EndTime = relevantSnapshots[count - 1].Timestamp
                };
            }
        }

        private class MetricsSnapshot
        {
            public DateTime Timestamp { get; set; }
            public int ProcessId { get; set; }
            public double MemoryUsageMb { get; set; }
            public int ThreadCount { get; set; }
            public long TotalOperations { get; set; }
            public double SuccessRate { get; set; }
            public double AverageLatencyMs { get; set; }
        }
    }

    public class MetricsSummary
    {
        public int PeriodMinutes { get; set; }
        public int SnapshotCount { get; set; }
        public double AvgMemoryMb { get; set; }
        public double MaxMemoryMb { get; set; }
        public double AvgLatencyMs { get; set; }
        public double MaxLatencyMs { get; set; }
        public double AvgSuccessRate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
