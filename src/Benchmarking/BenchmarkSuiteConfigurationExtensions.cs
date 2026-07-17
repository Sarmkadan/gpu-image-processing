using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GpuImageProcessing.Benchmarking
{
    /// <summary>
    /// Extension methods for <see cref="BenchmarkSuiteConfiguration"/> validation and utility operations.
    /// </summary>
    public static class BenchmarkSuiteConfigurationExtensions
    {
        /// <summary>
        /// Validates the configuration and throws appropriate exceptions if invalid.
        /// </summary>
        /// <param name="config">The benchmark suite configuration to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when required properties are invalid.</exception>
        public static void MaybeValidateAndThrow(this BenchmarkSuiteConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            if (string.IsNullOrWhiteSpace(config.RunName))
            {
                throw new ArgumentException("RunName must not be blank", nameof(config.RunName));
            }

            if (!config.GetEnabledCategories().Any())
            {
                throw new ArgumentException("At least one benchmark category must be enabled");
            }
        }

        /// <summary>
        /// Generates a summary of enabled benchmark categories.
        /// </summary>
        /// <param name="config">The benchmark suite configuration.</param>
        /// <returns>Formatted string listing enabled categories.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
        public static string GetEnabledCategorySummary(this BenchmarkSuiteConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            return config.GetEnabledCategories() switch
            {
                var categories when categories.Count == 0 => "No benchmarks enabled",
                var categories when categories.Count == 1 => $"1 benchmark category enabled: {categories[0]}",
                var categories => $"{categories.Count} benchmark categories enabled: {string.Join(", ", categories)}"
            };
        }

        /// <summary>
        /// Ensures the output directory exists, creating it if necessary.
        /// </summary>
        /// <param name="config">The benchmark suite configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="config.OutputDirectory"/> is an invalid path.</exception>
        /// <exception cref="IOException">Thrown when directory creation fails.</exception>
        public static void MaybeEnsureOutputDirectory(this BenchmarkSuiteConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            if (config.OutputDirectory is { Length: > 0 } outputDir)
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PathTooLongException)
                {
                    throw new IOException($"Failed to create output directory '{outputDir}': {ex.Message}", ex);
                }
            }
        }
    }
}