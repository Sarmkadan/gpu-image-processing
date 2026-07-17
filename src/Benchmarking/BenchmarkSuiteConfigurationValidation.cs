#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpuImageProcessing.Benchmarking;

/// <summary>
/// Provides validation helpers for <see cref="BenchmarkSuiteConfiguration"/>.
/// </summary>
public static class BenchmarkSuiteConfigurationValidation
{
    /// <summary>
    /// Validates the configuration and returns a list of validation errors.
    /// An empty list means the configuration is ready to use.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>
    /// A read-only list of human-readable error strings.
    /// Empty when the configuration is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BenchmarkSuiteConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value.RunName))
            errors.Add($"{nameof(BenchmarkSuiteConfiguration.RunName)} must not be blank.");

        if (!value.IncludeFilterChainBenchmarks
            && !value.IncludeBatchProcessingBenchmarks
            && !value.IncludeFilterChainBuilderBenchmarks
            && !value.IncludeImageUtilitiesBenchmarks
            && !value.IncludeEnumerableExtensionsBenchmarks)
            errors.Add("At least one benchmark category must be enabled.");

        if (value.OutputDirectory is not null && value.OutputDirectory.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            errors.Add($"{nameof(BenchmarkSuiteConfiguration.OutputDirectory)} contains invalid path characters.");

        if (value.AccuracyLevel is not (BenchmarkAccuracyLevel.Quick or BenchmarkAccuracyLevel.Standard or BenchmarkAccuracyLevel.Thorough))
            errors.Add($"{nameof(BenchmarkSuiteConfiguration.AccuracyLevel)} must be within the valid range.");

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the configuration is valid.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <returns>
    /// <c>true</c> if the configuration is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this BenchmarkSuiteConfiguration value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the configuration is valid, throwing an exception if it's not.
    /// </summary>
    /// <param name="value">The configuration to validate.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this BenchmarkSuiteConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException($"Invalid configuration: {string.Join(", ", errors)}", nameof(value));
        }
    }
}
