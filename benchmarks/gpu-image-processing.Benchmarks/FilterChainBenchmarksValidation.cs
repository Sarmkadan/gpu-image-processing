#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="FilterChainBenchmarks"/> instances.
/// </summary>
public static class FilterChainBenchmarksValidation
{
    /// <summary>
    /// Validates the state of the supplied <see cref="FilterChainBenchmarks"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>
    /// A read-only list of human-readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this FilterChainBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // FilterChainBenchmarks doesn't expose public state through properties.
        // The benchmark methods themselves validate their internal state when executed.
        // This validation is a no-op placeholder to match the expected API pattern.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="FilterChainBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this FilterChainBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true; // Placeholder - benchmarks don't expose mutable state
    }

    /// <summary>
    /// Ensures that the supplied <see cref="FilterChainBenchmarks"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance is invalid. The exception message contains a list of the problems.
    /// </exception>
    public static void EnsureValid(this FilterChainBenchmarks value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Placeholder - benchmarks don't expose mutable state to validate
    }
}
