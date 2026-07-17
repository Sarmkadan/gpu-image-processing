#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="GpuPerformanceBenchmarks"/> benchmark configurations.
/// </summary>
public static class GpuPerformanceBenchmarksValidation
{
    /// <summary>
    /// Validates the state of the supplied <see cref="GpuPerformanceBenchmarks"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>
    /// A read‑only list of human‑readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this GpuPerformanceBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Image dimensions must be positive integers.
        if (value.ImageWidth <= 0)
        {
            problems.Add(
                $"ImageWidth must be greater than 0, but was {value.ImageWidth}.");
        }

        if (value.ImageHeight <= 0)
        {
            problems.Add(
                $"ImageHeight must be greater than 0, but was {value.ImageHeight}.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the supplied <see cref="GpuPerformanceBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this GpuPerformanceBenchmarks value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the supplied <see cref="GpuPerformanceBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance is invalid. The exception message contains a semicolon-separated list of problems.
    /// </exception>
    public static void EnsureValid(this GpuPerformanceBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"GpuPerformanceBenchmarks is invalid: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}
