#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Services;

/// <summary>
/// Extension methods for <see cref="PerformanceMonitoringService"/> providing additional functionality for performance monitoring.
/// </summary>
public static class PerformanceMonitoringServiceExtensions
{
    /// <summary>
    /// Records multiple operations at once with a single call.
    /// </summary>
    /// <param name="service">The performance monitoring service. Cannot be null.</param>
    /// <param name="executionTimesMs">Array of execution times in milliseconds. Cannot be null or empty.</param>
    /// <param name="success">Whether all operations were successful (default: true)</param>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="executionTimesMs"/> is null.</exception>
    public static void RecordOperations(this PerformanceMonitoringService service, double[] executionTimesMs, bool success = true)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(executionTimesMs);

        if (executionTimesMs.Length == 0)
            return;

        foreach (var time in executionTimesMs)
        {
            service.RecordOperation(time, success);
        }
    }

    /// <summary>
    /// Gets performance metrics with trend analysis comparing to previous snapshot.
    /// </summary>
    /// <param name="service">The performance monitoring service. Cannot be null.</param>
    /// <param name="previousMetrics">Previous metrics snapshot for comparison.</param>
    /// <returns>PerformanceMetrics with trend indicators.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static PerformanceMetricsWithTrends GetMetricsWithTrends(this PerformanceMonitoringService service, PerformanceMetrics? previousMetrics = null)
    {
        ArgumentNullException.ThrowIfNull(service);

        var current = service.GetCurrentMetrics();

        var trends = new PerformanceMetricsWithTrends
        {
            Current = current,
            Previous = previousMetrics,
            Timestamp = DateTime.UtcNow
        };

        if (previousMetrics is not null)
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
    /// <param name="service">The performance monitoring service. Cannot be null.</param>
    /// <param name="startTime">Start time for filtering.</param>
    /// <param name="endTime">End time for filtering.</param>
    /// <returns>Filtered list of performance metrics.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static IEnumerable<PerformanceMetrics> GetMetricsInTimeRange(this PerformanceMonitoringService service, DateTime startTime, DateTime endTime)
    {
        ArgumentNullException.ThrowIfNull(service);

        var allMetrics = service.GetMetricsHistory();
        return allMetrics.Where(m => m.RecordedAt >= startTime && m.RecordedAt <= endTime);
    }

    /// <summary>
    /// Gets performance alerts based on configured thresholds.
    /// </summary>
    /// <param name="service">The performance monitoring service. Cannot be null.</param>
    /// <returns>List of performance alerts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static List<PerformanceAlert> GetPerformanceAlerts(this PerformanceMonitoringService service)
    {
        ArgumentNullException.ThrowIfNull(service);

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

    /// <summary>
    /// Calculates the percentage change between two values.
    /// </summary>
    /// <param name="previousValue">The previous value. If zero, returns 100% if current is positive, 0% otherwise.</param>
    /// <param name="currentValue">The current value.</param>
    /// <returns>The percentage change from previous to current value.</returns>
    private static double CalculateChangePercent(double previousValue, double currentValue)
    {
        if (previousValue == 0)
            return currentValue > 0 ? 100.0 : 0;

        return ((currentValue - previousValue) / previousValue) * 100.0;
    }
}

/// <summary>
/// Container for performance metrics with trend analysis comparing current and previous metrics.
/// </summary>
public sealed class PerformanceMetricsWithTrends
{
    /// <summary>Gets or sets the current performance metrics.</summary>
    public PerformanceMetrics Current { get; set; } = null!;

    /// <summary>Gets or sets the previous performance metrics for comparison. May be null if no previous metrics exist.</summary>
    public PerformanceMetrics? Previous { get; set; }

    /// <summary>Gets or sets the timestamp when the trend analysis was performed.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets or sets the percentage change in CPU usage.</summary>
    public double CpuChangePercent { get; set; }

    /// <summary>Gets or sets the percentage change in GPU utilization.</summary>
    public double GpuChangePercent { get; set; }

    /// <summary>Gets or sets the percentage change in GPU memory usage.</summary>
    public double MemoryChangePercent { get; set; }

    /// <summary>Gets or sets the percentage change in throughput.</summary>
    public double ThroughputChangePercent { get; set; }

    /// <summary>Gets or sets the percentage change in average execution time.</summary>
    public double ExecutionTimeChangePercent { get; set; }
}

/// <summary>
/// Represents a performance alert with severity level, generated when performance thresholds are exceeded.
/// </summary>
public sealed class PerformanceAlert
{
    /// <summary>Gets the type of alert.</summary>
    public AlertType Type { get; }

    /// <summary>Gets the alert message describing the issue.</summary>
    public string Message { get; }

    /// <summary>Gets the current value that triggered the alert.</summary>
    public double CurrentValue { get; }

    /// <summary>Gets the threshold value that was exceeded.</summary>
    public double Threshold { get; }

    /// <summary>Gets the timestamp when the alert was generated.</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceAlert"/> class.
    /// </summary>
    /// <param name="type">The type of alert.</param>
    /// <param name="message">The alert message.</param>
    /// <param name="currentValue">The current value that triggered the alert.</param>
    /// <param name="threshold">The threshold value that was exceeded.</param>
    public PerformanceAlert(AlertType type, string message, double currentValue, double threshold)
    {
        Type = type;
        Message = message;
        CurrentValue = currentValue;
        Threshold = threshold;
    }

    /// <summary>
    /// Returns the alert message.
    /// </summary>
    /// <returns>The alert message string.</returns>
    public override string ToString() => Message;
}

/// <summary>
/// Types of performance alerts that can be generated by the monitoring system.
/// </summary>
public enum AlertType
{
    /// <summary>Alert triggered when CPU usage exceeds the configured threshold.</summary>
    CpuThreshold,

    /// <summary>Alert triggered when GPU utilization exceeds the configured threshold.</summary>
    GpuThreshold,

    /// <summary>Alert triggered when GPU memory usage exceeds the configured threshold.</summary>
    MemoryThreshold,

    /// <summary>Alert triggered when average execution time exceeds the configured threshold.</summary>
    ExecutionTimeThreshold,

    /// <summary>Alert triggered when success rate falls below the configured threshold.</summary>
    SuccessRateThreshold
}