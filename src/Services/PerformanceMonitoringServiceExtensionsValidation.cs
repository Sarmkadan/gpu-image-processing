using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Services;

/// <summary>
/// Provides validation helpers for <see cref="PerformanceMetricsWithTrends"/> and <see cref="PerformanceAlert"/>.
/// </summary>
public static class PerformanceMonitoringServiceExtensionsValidation
{
    /// <summary>
    /// Validates <see cref="PerformanceMetricsWithTrends"/>.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems, if any.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceMetricsWithTrends value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = new List<string>();
        if (value.Current == null) problems.Add("Current metrics cannot be null.");
        if (value.Timestamp == default(DateTime)) problems.Add("Timestamp cannot be default.");
        if (!double.IsFinite(value.CpuChangePercent)) problems.Add("CpuChangePercent must be finite.");
        if (!double.IsFinite(value.GpuChangePercent)) problems.Add("GpuChangePercent must be finite.");
        if (!double.IsFinite(value.MemoryChangePercent)) problems.Add("MemoryChangePercent must be finite.");
        if (!double.IsFinite(value.ThroughputChangePercent)) problems.Add("ThroughputChangePercent must be finite.");
        if (!double.IsFinite(value.ExecutionTimeChangePercent)) problems.Add("ExecutionTimeChangePercent must be finite.");
        return problems;
    }

    /// <summary>
    /// Checks if <see cref="PerformanceMetricsWithTrends"/> is valid.
    /// </summary>
    public static bool IsValid(this PerformanceMetricsWithTrends value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures <see cref="PerformanceMetricsWithTrends"/> is valid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if not valid.</exception>
    public static void EnsureValid(this PerformanceMetricsWithTrends value)
    {
        var problems = value.Validate();
        if (problems.Count > 0)
            throw new ArgumentException($"Invalid PerformanceMetricsWithTrends: {string.Join(", ", problems)}");
    }

    /// <summary>
    /// Validates <see cref="PerformanceAlert"/>.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems, if any.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceAlert value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(value.Message)) problems.Add("Message cannot be null or whitespace.");
        if (value.Timestamp == default(DateTime)) problems.Add("Timestamp cannot be default.");
        if (!double.IsFinite(value.CurrentValue)) problems.Add("CurrentValue must be finite.");
        if (!double.IsFinite(value.Threshold)) problems.Add("Threshold must be finite.");
        return problems;
    }

    /// <summary>
    /// Checks if <see cref="PerformanceAlert"/> is valid.
    /// </summary>
    public static bool IsValid(this PerformanceAlert value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures <see cref="PerformanceAlert"/> is valid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if not valid.</exception>
    public static void EnsureValid(this PerformanceAlert value)
    {
        var problems = value.Validate();
        if (problems.Count > 0)
            throw new ArgumentException($"Invalid PerformanceAlert: {string.Join(", ", problems)}");
    }
}
