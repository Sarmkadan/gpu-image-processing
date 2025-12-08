#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Tests.Benchmarking;

/// <summary>
/// Provides validation helpers for <see cref="BenchmarkSuiteConfigurationTests"/> instances.
/// </summary>
public static class BenchmarkSuiteConfigurationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="BenchmarkSuiteConfigurationTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BenchmarkSuiteConfigurationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // BenchmarkSuiteConfigurationTests is a test class with no actual properties to validate
        // The validation simply ensures the test class instance is properly initialized
        // This mirrors the pattern used in other test validation classes in this project

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="BenchmarkSuiteConfigurationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this BenchmarkSuiteConfigurationTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="BenchmarkSuiteConfigurationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this BenchmarkSuiteConfigurationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BenchmarkSuiteConfigurationTests instance is not valid. Problems:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}