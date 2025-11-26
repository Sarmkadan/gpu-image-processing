#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Services;

/// <summary>
/// Validation helpers for <see cref="PerformanceMonitoringService"/>.
/// </summary>
public static class PerformanceMonitoringServiceValidation
{
    /// <summary>
    /// Validates a <see cref="PerformanceMonitoringService"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable problem descriptions.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PerformanceMonitoringService value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = [];

        // No specific validation rules for PerformanceMonitoringService

        return problems;
    }

    /// <summary>
    /// Checks if a <see cref="PerformanceMonitoringService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PerformanceMonitoringService value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a <see cref="PerformanceMonitoringService"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <exception cref="ArgumentException">If the instance is not valid.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this PerformanceMonitoringService value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException("Invalid PerformanceMonitoringService instance", nameof(value), string.Join(Environment.NewLine, problems));
        }
    }
}
