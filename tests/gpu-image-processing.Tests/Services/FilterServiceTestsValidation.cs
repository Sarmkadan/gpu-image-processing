#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Provides validation helpers for <see cref="FilterServiceTests"/> instances.
/// </summary>
public static class FilterServiceTestsValidation
{
    /// <summary>
    /// Validates a <see cref="FilterServiceTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The test fixture to validate</param>
    /// <returns>A read-only list of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this FilterServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // FilterServiceTests is a test fixture with no public state to validate
        // All validation is handled by argument validation in its methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="FilterServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test fixture to check</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/></returns>
    public static bool IsValid(this FilterServiceTests? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="FilterServiceTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The test fixture to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the test fixture is not valid</exception>
    public static void EnsureValid(this FilterServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"FilterServiceTests is not valid. Problems: {string.Join("; ", problems)}",
            nameof(value));
    }
}