#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Provides validation extension methods for <see cref="FilterServiceTests"/> instances.
/// </summary>
public static class FilterServiceTestsValidation
{
    /// <summary>
    /// Validates a <see cref="FilterServiceTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <remarks>
    /// Currently, <see cref="FilterServiceTests"/> contains no public state that requires validation.
    /// All validation logic is handled by argument validation within individual test methods.
    /// This method exists to provide a consistent validation interface for test fixtures.
    /// </remarks>
    /// <param name="value">The test fixture to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this FilterServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="FilterServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test fixture to check.</param>
    /// <returns><see langword="true"/> if the instance is valid (or <see langword="null"/>); otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this FilterServiceTests? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="FilterServiceTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The test fixture to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the test fixture is not valid.</exception>
    public static void EnsureValid(this FilterServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.IsValid())
        {
            return;
        }

        throw new ArgumentException(
            "The FilterServiceTests instance is not valid.",
            nameof(value));
    }
}