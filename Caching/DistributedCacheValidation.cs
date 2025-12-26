#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// Provides validation helpers for <see cref="DistributedCache"/> instances.
    /// </summary>
    public static class DistributedCacheValidation
    {
        /// <summary>
        /// Validates the cache instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The cache instance to validate</param>
        /// <returns>List of validation problems; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static IReadOnlyList<string> Validate(this DistributedCache value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate key-related properties
            var stats = value.GetStats();

            if (stats.ItemCount < 0)
            {
                problems.Add($"ItemCount is negative: {stats.ItemCount}");
            }

            if (stats.UsedMemoryBytes < 0)
            {
                problems.Add($"UsedMemoryBytes is negative: {stats.UsedMemoryBytes}");
            }

            if (stats.MaxMemoryBytes <= 0)
            {
                problems.Add($"MaxMemoryBytes must be positive, but was: {stats.MaxMemoryBytes}");
            }

            if (stats.MemoryUsagePercent < 0 || stats.MemoryUsagePercent > 100)
            {
                problems.Add($"MemoryUsagePercent must be between 0 and 100, but was: {stats.MemoryUsagePercent}");
            }

            if (stats.AverageItemSize < 0)
            {
                problems.Add($"AverageItemSize is negative: {stats.AverageItemSize}");
            }

            if (stats.TotalAccesses < 0)
            {
                problems.Add($"TotalAccesses is negative: {stats.TotalAccesses}");
            }

            // Validate temporal properties
            var now = DateTime.UtcNow;

            if (stats.ItemCount > 0)
            {
                // These would require reflection to validate properly since they're not directly accessible
                // We'll skip them as they're internal implementation details
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the cache instance is valid.
        /// </summary>
        /// <param name="value">The cache instance to check</param>
        /// <returns>True if valid; false otherwise</returns>
        public static bool IsValid(this DistributedCache value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures the cache instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The cache instance to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
        public static void EnsureValid(this DistributedCache value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();

            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"DistributedCache validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
            }
        }
    }
}