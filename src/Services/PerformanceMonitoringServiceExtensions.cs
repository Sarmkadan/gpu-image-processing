#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Services;

/// <summary>
/// Extension methods for PerformanceMonitoringService providing additional functionality.
/// </summary>
public static class PerformanceMonitoringServiceExtensions
{
    /// <summary>
    /// Records multiple operations at once with a single call.
    /// </summary>
    /// <param name="service">The performance monitoring service</param>
    /// <param name="executionTimesMs">Array of execution times in milliseconds</param>
    /// <param name="success">Whether all operations were successful (default: true)</param>
    public static void RecordOperations(this PerformanceMonitoringService service, double[] executionTimesMs, bool success = true)
    {
        if (executionTimesMs == null || executionTimesMs.Length == 0)
            return;

        foreach (var time in executionTimesMs)
        {
            service.RecordOperation(time, success);
        }
    }

    /// <summary>
    /// Gets performance metrics with trend analysis comparing to previous snapshot.
    /// </summary>
    /// <param name="service">The performance monitoring service</param>
    /// <param name="previousMetrics">Previous metrics snapshot for comparison</param>
    /// <returns>PerformanceMetrics with trend indicators</returns>
    public static PerformanceMetricsWithTrends GetMetricsWithTrends(this PerformanceMonitoringService service, PerformanceMetrics? previousMetrics = null)
    {
        var current = service.GetCurrentMetrics();

        var trends = new PerformanceMetricsWithTrends
        {
            Current = current,
            Previous = previousMetrics,
            Timestamp = DateTime.UtcNow
        };

        if (previousMetrics != null)
        {
            trends.CpuChangePercent = CalculateChangePercent(previousMetrics.CpuUsagePercent, current.CpuUsagePercent);
            trends.GpuChangePercent = CalculateChangePercent(previousMetrics.GpuUtilizationPercent, current.GpuUtilizationPercent);
            trends.MemoryChangePercent = CalculateChangePercent(previousMetrics.GpuMemoryUsedBytes, current.GpuMemoryUsedBytes);
            trends.ThroughputChangePercent = CalculateChangePercent(previousMetrics.ThroughputMegabytesPerSecond, current.ThroughputMegabytesPerSecond);
            trends.ExecutionTimeChangePercent = CalculateChangePercent(previousMetrics.AverageExecutionTimeMs, current.AverageExecutionTimeMs);
        }
        else
        {
            trends.CpuChangePercent = 0;
            trends.GpuChangePercent = 0;
            trends.MemoryChangePercent = 0;
            trends.ThroughputChangePercent = 0;
            trends.ExecutionTimeChangePercent = 0;
        }

        return trends;
    }

    /// <summary>
    /// Gets performance metrics filtered by time range.
    /// </summary>
    /// <param name="service">The performance monitoring service</param>
    /// <param name="startTime">Start time for filtering</param>
    /// <param name="endTime">End time for filtering</param>
    /// <returns>Filtered list of performance metrics</returns>
    public static IEnumerable<PerformanceMetrics> GetMetricsInTimeRange(this PerformanceMonitoringService service, DateTime startTime, DateTime endTime)
    {
        var allMetrics = service.GetMetricsHistory();
        return allMetrics.Where(m => m.RecordedAt >= startTime && m.RecordedAt <= endTime);
    }

    /// <summary>
    /// Gets performance alerts based on configured thresholds.
    /// </summary>
    /// <param name="service">The performance monitoring service</param>
    /// <returns>List of performance alerts</returns>
    public static List<PerformanceAlert> GetPerformanceAlerts(this PerformanceMonitoringService service)
    {
        var current = service.GetCurrentMetrics();
        var alerts = new List<PerformanceAlert>();

        // Check CPU usage threshold (warn if > 80%)
        if (current.CpuUsagePercent > 80.0)
        {
            alerts.Add(new PerformanceAlert(
                AlertType.CpuThreshold,
                $"High CPU usage detected: {current.CpuUsagePercent:F2}% (threshold: 80%)",
                current.CpuUsagePercent,
                80.0
            ));
        }

        // Check GPU utilization threshold (warn if > 85%)
        if (current.GpuUtilizationPercent > 85.0)
        {
            alerts.Add(new PerformanceAlert(
                AlertType.GpuThreshold,
                $"High GPU utilization detected: {current.GpuUtilizationPercent:F2}% (threshold: 85%)",
                current.GpuUtilizationPercent,
                85.0
            ));
        }

        // Check GPU memory threshold
        if (current.IsMemoryWarningRequired())
        {
            alerts.Add(new PerformanceAlert(
                AlertType.MemoryThreshold,
                $"High GPU memory usage detected: {current.GpuMemoryUsedBytes / (1024 * 1024)} MB (threshold: {AppConstants.Memory.MemoryWarningThreshold / (1024 * 1024)} MB)",
                current.GetMemoryUsagePercent(),
                80.0 // Memory usage percentage threshold
            ));
        }

        // Check execution time threshold
        if (current.AverageExecutionTimeMs > AppConstants.Performance.SlowOperationThresholdMs)
        {
            alerts.Add(new PerformanceAlert(
                AlertType.ExecutionTimeThreshold,
                $"High average execution time detected: {current.AverageExecutionTimeMs:F2}ms (threshold: {AppConstants.Performance.SlowOperationThresholdMs}ms)",
                current.AverageExecutionTimeMs,
                AppConstants.Performance.SlowOperationThresholdMs
            ));
        }

        // Check success rate threshold (warn if < 95%)
        var successRate = current.GetSuccessRate();
        if (successRate < 95.0)
        {
            alerts.Add(new PerformanceAlert(
                AlertType.SuccessRateThreshold,
                $"Low success rate detected: {successRate:F2}% (threshold: 95%)",
                successRate,
                95.0
            ));
        }

        return alerts;
    }

    private static double CalculateChangePercent(double previousValue, double currentValue)
    {
        if (previousValue == 0)
            return currentValue > 0 ? 100.0 : 0;

        return ((currentValue - previousValue) / previousValue) * 100.0;
    }
}

/// <summary>
/// Container for performance metrics with trend analysis.
/// </summary>
public class PerformanceMetricsWithTrends
{
    public PerformanceMetrics Current { get; set; } = null!;
    public PerformanceMetrics? Previous { get; set; }
    public DateTime Timestamp { get; set; }
    public double CpuChangePercent { get; set; }
    public double GpuChangePercent { get; set; }
    public double MemoryChangePercent { get; set; }
    public double ThroughputChangePercent { get; set; }
    public double ExecutionTimeChangePercent { get; set; }
}

/// <summary>
/// Represents a performance alert with severity level.
/// </summary>
public class PerformanceAlert
{
    public AlertType Type { get; }
    public string Message { get; }
    public double CurrentValue { get; }
    public double Threshold { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public PerformanceAlert(AlertType type, string message, double currentValue, double threshold)
    {
        Type = type;
        Message = message;
        CurrentValue = currentValue;
        Threshold = threshold;
    }

    public override string ToString() => Message;
}

/// <summary>
/// Types of performance alerts.
/// </summary>
public enum AlertType
{
    CpuThreshold,
    GpuThreshold,
    MemoryThreshold,
    ExecutionTimeThreshold,
    SuccessRateThreshold
}