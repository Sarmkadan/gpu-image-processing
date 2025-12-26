#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// Provides validation helpers for <see cref="ProcessingCache"/> instances.
    /// </summary>
    public static class ProcessingCacheValidation
    {
        /// <summary>
        /// Validates a <see cref="ProcessingCache"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The cache instance to validate.</param>
        /// <returns>A list of validation problems; empty if the cache is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ProcessingCache value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate the cache statistics
            var stats = value.GetStatistics();

            // Validate EntryCount (should not be negative, should not exceed MaxEntries)
            if (stats.EntryCount < 0)
            {
                problems.Add(
                    $"Cache EntryCount is negative: {stats.EntryCount}.");
            }

            if (stats.EntryCount > stats.MaxEntries)
            {
                problems.Add(
                    $"Cache EntryCount ({stats.EntryCount}) exceeds MaxEntries ({stats.MaxEntries}).");
            }

            // Validate MaxEntries (should be positive)
            if (stats.MaxEntries <= 0)
            {
                problems.Add(
                    $"Cache MaxEntries must be positive, but was {stats.MaxEntries}.");
            }

            // Validate UtilizationPercent (should be between 0 and 100)
            if (stats.UtilizationPercent < 0 || stats.UtilizationPercent > 100)
            {
                problems.Add(
                    $"Cache UtilizationPercent must be between 0 and 100, but was {stats.UtilizationPercent}%.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ProcessingCache"/> instance is valid.
        /// </summary>
        /// <param name="value">The cache instance to check.</param>
        /// <returns><see langword="true"/> if the cache is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this ProcessingCache value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ProcessingCache"/> instance is valid.
        /// </summary>
        /// <param name="value">The cache instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the cache is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this ProcessingCache value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"ProcessingCache is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}