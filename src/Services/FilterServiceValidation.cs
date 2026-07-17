#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;

namespace GpuImageProcessing.Services;

/// <summary>
/// Provides validation helpers for <see cref="FilterService"/> service.
/// </summary>
public static class FilterServiceValidation
{
    /// <summary>
    /// Validates a FilterService instance and returns a list of human-readable problems.
    /// </summary>
    /// <remarks>
    /// This validation checks that the service instance is not null. The FilterService constructor
    /// already validates its dependencies (repository and logger) via constructor injection,
    /// and the filter handlers dictionary is initialized during construction and cannot be null.
    /// </remarks>
    /// <param name="value">The FilterService to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this FilterService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified FilterService is valid.
    /// </summary>
    /// <param name="value">The FilterService to check.</param>
    /// <returns><see langword="true"/> if the service is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this FilterService? value) => value is not null;

    /// <summary>
    /// Ensures that the specified FilterService is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The FilterService to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this FilterService? value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}