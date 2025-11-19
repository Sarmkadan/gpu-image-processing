#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Validation helpers for <see cref="BatchProcessingBenchmarks"/>.
/// </summary>
public static class BatchProcessingBenchmarksValidation
{
    /// <summary>
    /// Returns a read‑only list of validation problems for the supplied <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <returns>
    /// A read‑only list of human‑readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this BatchProcessingBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ImageCount is supplied via [Params] and must be a positive number.
        if (value.ImageCount <= 0)
        {
            problems.Add(
                $"ImageCount must be greater than 0 (actual: {value.ImageCount}).");
        }

        // The benchmark methods themselves do not expose mutable state that can be
        // validated without executing them, so only simple member checks are performed.
        // Additional checks could be added here if the public API exposed more data.

        return problems;
    }

    /// <summary>
    /// Determines whether the supplied <paramref name="value"/> passes all validation checks.
    /// </summary>
    /// <param name="value">The benchmark instance to test.</param>
    /// <returns><c>true</c> if no validation problems are found; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this BatchProcessingBenchmarks value) =>
        value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the supplied <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The benchmark instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message
    /// contains a semicolon‑separated list of the problems.
    /// </exception>
    public static void EnsureValid(this BatchProcessingBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BatchProcessingBenchmarks validation failed: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}
