#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Domain;

/// <summary>
/// Provides validation helpers for <see cref="ImageDomainTests"/> instances.
/// </summary>
public static class ImageDomainTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ImageDomainTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ImageDomainTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ImageDomainTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ImageDomainTests? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ImageDomainTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The instance is not valid.</exception>
    public static void EnsureValid(this ImageDomainTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ImageDomainTests instance is not valid. Problems: {string.Join(", ", errors)}");
        }
    }
}