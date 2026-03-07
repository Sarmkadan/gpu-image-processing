#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Services;

/// <summary>
/// Service for monitoring and tracking performance metrics.
/// </summary>
public class PerformanceMonitoringService
{
    private readonly List<PerformanceMetrics> _metricsHistory = [];
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private PerformanceMetrics _currentMetrics;
    private readonly object _lockObject = new();
    private DateTime _lastCleanupTime = DateTime.UtcNow;

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentMetrics = new PerformanceMetrics();
    }

    /// <summary>
    /// Records an operation execution time and status.
    /// </summary>
    public void RecordOperation(double executionTimeMs, bool success = true)
    {
        lock (_lockObject)
        {
            _currentMetrics.RecordExecution(executionTimeMs);

            if (!success)
                _currentMetrics.FailedOperationsCount++;

            if (executionTimeMs > Constants.Performance.SlowOperationThresholdMs)
            {
                _logger.LogWarning("Slow operation detected: {ExecutionTime}ms (threshold: {Threshold}ms)",
                    executionTimeMs, Constants.Performance.SlowOperationThresholdMs);
            }
        }
    }

    /// <summary>
    /// Updates GPU and system metrics.
    /// </summary>
    public void UpdateSystemMetrics(double cpuPercent, long memoryBytes, long gpuMemoryBytes, double gpuUtilization)
    {
        lock (_lockObject)
        {
            _currentMetrics.CpuUsagePercent = cpuPercent;
            _currentMetrics.MemoryUsedBytes = memoryBytes;
            _currentMetrics.GpuMemoryUsedBytes = gpuMemoryBytes;
            _currentMetrics.GpuUtilizationPercent = gpuUtilization;
        }
    }

    /// <summary>
    /// Updates throughput metrics.
    /// </summary>
    public void UpdateThroughput(long pixelsPerSecond, double megabytesPerSecond)
    {
        lock (_lockObject)
        {
            _currentMetrics.ImagePixelsProcessedPerSecond = pixelsPerSecond;
            _currentMetrics.ThroughputMegabytesPerSecond = megabytesPerSecond;
        }
    }

    /// <summary>
    /// Gets current performance metrics.
    /// </summary>
    public PerformanceMetrics GetCurrentMetrics()
    {
        lock (_lockObject)
        {
            return new PerformanceMetrics
            {
                CpuUsagePercent = _currentMetrics.CpuUsagePercent,
                MemoryUsedBytes = _currentMetrics.MemoryUsedBytes,
                GpuMemoryUsedBytes = _currentMetrics.GpuMemoryUsedBytes,
                GpuUtilizationPercent = _currentMetrics.GpuUtilizationPercent,
                AverageExecutionTimeMs = _currentMetrics.AverageExecutionTimeMs,
                MaxExecutionTimeMs = _currentMetrics.MaxExecutionTimeMs,
                MinExecutionTimeMs = _currentMetrics.MinExecutionTimeMs,
                ImagePixelsProcessedPerSecond = _currentMetrics.ImagePixelsProcessedPerSecond,
                TotalOperationsCount = _currentMetrics.TotalOperationsCount,
                FailedOperationsCount = _currentMetrics.FailedOperationsCount,
                ThroughputMegabytesPerSecond = _currentMetrics.ThroughputMegabytesPerSecond
            };
        }
    }

    /// <summary>
    /// Snapshots current metrics and starts new measurement period.
    /// </summary>
    public PerformanceMetrics SnapshotAndReset()
    {
        lock (_lockObject)
        {
            var snapshot = new PerformanceMetrics
            {
                CpuUsagePercent = _currentMetrics.CpuUsagePercent,
                MemoryUsedBytes = _currentMetrics.MemoryUsedBytes,
                GpuMemoryUsedBytes = _currentMetrics.GpuMemoryUsedBytes,
                GpuUtilizationPercent = _currentMetrics.GpuUtilizationPercent,
                AverageExecutionTimeMs = _currentMetrics.AverageExecutionTimeMs,
                MaxExecutionTimeMs = _currentMetrics.MaxExecutionTimeMs,
                MinExecutionTimeMs = _currentMetrics.MinExecutionTimeMs,
                ImagePixelsProcessedPerSecond = _currentMetrics.ImagePixelsProcessedPerSecond,
                TotalOperationsCount = _currentMetrics.TotalOperationsCount,
                FailedOperationsCount = _currentMetrics.FailedOperationsCount,
                ThroughputMegabytesPerSecond = _currentMetrics.ThroughputMegabytesPerSecond
            };

            _metricsHistory.Add(snapshot);
            CleanupOldMetrics();

            _currentMetrics = new PerformanceMetrics();
            _logger.LogInformation("Metrics snapshot: {Metrics}", snapshot);

            return snapshot;
        }
    }

    /// <summary>
    /// Gets historical metrics.
    /// </summary>
    public IEnumerable<PerformanceMetrics> GetMetricsHistory(int? limit = null)
    {
        lock (_lockObject)
        {
            var history = _metricsHistory.OrderByDescending(m => m.RecordedAt).ToList();
            return limit.HasValue ? history.Take(limit.Value) : history;
        }
    }

    /// <summary>
    /// Gets average metrics over a time period.
    /// </summary>
    public Dictionary<string, double> GetAverageMetrics(int lastMinutes = 60)
    {
        lock (_lockObject)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-lastMinutes);
            var recentMetrics = _metricsHistory
                .Where(m => m.RecordedAt >= cutoffTime)
                .ToList();

            if (recentMetrics.Count == 0)
                return new Dictionary<string, double>();

            return new Dictionary<string, double>
            {
                { "AverageCpuPercent", recentMetrics.Average(m => m.CpuUsagePercent) },
                { "AverageMemoryBytes", recentMetrics.Average(m => m.MemoryUsedBytes) },
                { "AverageGpuMemoryBytes", recentMetrics.Average(m => m.GpuMemoryUsedBytes) },
                { "AverageGpuUtilization", recentMetrics.Average(m => m.GpuUtilizationPercent) },
                { "AverageExecutionTime", recentMetrics.Average(m => m.AverageExecutionTimeMs) },
                { "AverageThroughput", recentMetrics.Average(m => m.ThroughputMegabytesPerSecond) }
            };
        }
    }

    /// <summary>
    /// Gets performance report.
    /// </summary>
    public string GetPerformanceReport()
    {
        var current = GetCurrentMetrics();
        var report = new StringBuilder();

        report.AppendLine("=== Performance Report ===");
        report.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
        report.AppendLine($"Operations: {current.TotalOperationsCount} (Failed: {current.FailedOperationsCount})");
        report.AppendLine($"Success Rate: {current.GetSuccessRate():F2}%");
        report.AppendLine($"CPU Usage: {current.CpuUsagePercent:F2}%");
        report.AppendLine($"GPU Utilization: {current.GpuUtilizationPercent:F2}%");
        report.AppendLine($"GPU Memory: {current.GpuMemoryUsedBytes / (1024 * 1024)} MB");
        report.AppendLine($"Execution Time - Avg: {current.AverageExecutionTimeMs:F2}ms, " +
                         $"Min: {current.MinExecutionTimeMs:F2}ms, Max: {current.MaxExecutionTimeMs:F2}ms");
        report.AppendLine($"Throughput: {current.ThroughputMegabytesPerSecond:F2} MB/s");
        report.AppendLine($"Pixels Processed/s: {current.ImagePixelsProcessedPerSecond}");

        return report.ToString();
    }

    private void CleanupOldMetrics()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastCleanupTime).TotalMinutes < 10)
            return;

        var cutoffTime = now.AddMinutes(-Constants.Performance.MetricsRetentionPeriodMinutes);
        _metricsHistory.RemoveAll(m => m.RecordedAt < cutoffTime);
        _lastCleanupTime = now;

        _logger.LogDebug("Cleaned up old metrics, keeping {Count} records", _metricsHistory.Count);
    }
}
