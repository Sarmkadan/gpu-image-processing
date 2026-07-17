#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Validation helpers for <see cref="ConcurrencyAndConfigurationTestSettings"/>.
/// </summary>
public static class ConcurrencyAndConfigurationTestsValidation
{
    /// <summary>
    /// Validates the specified <paramref name="value"/> and returns a list of
    /// human-readable problem descriptions. An empty list indicates success.
    /// </summary>
    /// <param name="value">The settings instance to validate.</param>
    /// <returns>A list of problem descriptions; empty if the settings are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ConcurrencyAndConfigurationTestSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxConcurrentThreads < 1)
        {
            problems.Add($"{nameof(ConcurrencyAndConfigurationTestSettings.MaxConcurrentThreads)} must be at least 1, but was {value.MaxConcurrentThreads}.");
        }

        if (value.TestDurationSeconds < 1)
        {
            problems.Add($"{nameof(ConcurrencyAndConfigurationTestSettings.TestDurationSeconds)} must be at least 1, but was {value.TestDurationSeconds}.");
        }

        if (value.MemoryLimitMb < 1)
        {
            problems.Add($"{nameof(ConcurrencyAndConfigurationTestSettings.MemoryLimitMb)} must be at least 1, but was {value.MemoryLimitMb}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The settings instance to validate.</param>
    /// <returns><see langword="true"/> if the settings are valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ConcurrencyAndConfigurationTestSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.IsValid;
    }

    /// <summary>
    /// Ensures that the specified <paramref name="value"/> is valid, throwing an
    /// <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The settings instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid; the exception message lists the problems.</exception>
    public static void EnsureValid(this ConcurrencyAndConfigurationTestSettings value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!value.IsValid)
        {
            var problems = Validate(value);
            throw new ArgumentException(
                $"Invalid {nameof(ConcurrencyAndConfigurationTestSettings)}: {string.Join(", ", problems)}",
                nameof(value));
        }
    }
}