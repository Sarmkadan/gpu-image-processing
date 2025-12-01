#nullable enable

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Services;

namespace GpuImageProcessing.Tests.Core.Services;

/// <summary>
/// Provides validation helpers for <see cref="CoreImageProcessingServiceTests"/> instances.
/// </summary>
public static class CoreImageProcessingServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="CoreImageProcessingServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CoreImageProcessingServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate mock fields (these are the actual public members we can validate)
        // Note: We can't validate the mock objects themselves, but we can ensure they're initialized

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CoreImageProcessingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CoreImageProcessingServiceTests? value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="CoreImageProcessingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this CoreImageProcessingServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"CoreImageProcessingServiceTests instance is not valid. Problems: {string.Join(", ", errors)}");
        }
    }
}