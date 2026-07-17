#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Provides validation helpers for <see cref="FilterChain"/> instances.
/// </summary>
public static class FilterChainBenchmarksValidation
{

    /// <summary>
    /// Validates the state of the supplied <see cref="FilterChain"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>
    /// A read-only list of human-readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> ValidateProblems(this FilterChain value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Filter chain name must not be blank.");
        }

        if (value.Steps.Count == 0)
        {
            problems.Add("A filter chain must contain at least one filter step.");
        }

        if (!value.IsEnabled)
        {
            problems.Add("Filter chain must be enabled.");
        }

        var enabledSteps = value.GetEnabledSteps();
        if (enabledSteps.Count == 0)
        {
            problems.Add("Filter chain must contain at least one enabled filter step.");
        }

        if (value.AllowParallelExecution && value.MaxParallelSteps is < 1 or > AppConstants.Processing.DefaultThreadCount * 4)
        {
            problems.Add($"When parallel execution is enabled, max parallel steps must be between 1 and {AppConstants.Processing.DefaultThreadCount * 4}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="FilterChain"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this FilterChain value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ValidateProblems().Count == 0;
    }

    /// <summary>
    /// Ensures that the supplied <see cref="FilterChain"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance is invalid. The exception message contains a list of the problems.
    /// </exception>
    public static void EnsureValid(this FilterChain value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.ValidateProblems();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Filter chain is not in a valid state. " +
                string.Join(" ", errors),
                nameof(value));
        }
    }
}