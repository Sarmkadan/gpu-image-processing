#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.BackgroundWorkers
{
    /// <summary>
    /// Provides validation helpers for <see cref="CacheMaintenanceWorker"/> instances.
    /// </summary>
    public static class CacheMaintenanceWorkerValidation
    {
        /// <summary>
        /// Validates the specified cache maintenance worker instance.
        /// </summary>
        /// <param name="value">The worker instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this CacheMaintenanceWorker value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate constructor parameters that might have been modified after construction
            if (value is { } worker)
            {
                // Validate cleanup interval is positive
                if (worker.GetCleanupInterval() <= TimeSpan.Zero)
                {
                    errors.Add("Cleanup interval must be greater than zero.");
                }

                // Validate memory warning threshold is within valid range
                var threshold = worker.GetMemoryWarningThreshold();
                if (threshold < 0 || threshold > 100)
                {
                    errors.Add("Memory warning threshold must be between 0 and 100.");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified cache maintenance worker is valid.
        /// </summary>
        /// <param name="value">The worker instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this CacheMaintenanceWorker value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified cache maintenance worker is valid.
        /// </summary>
        /// <param name="value">The worker instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if validation fails with all validation errors.</exception>
        public static void EnsureValid(this CacheMaintenanceWorker value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"CacheMaintenanceWorker is not valid:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", errors)}",
                    nameof(value));
            }
        }

        /// <summary>
        /// Gets the cleanup interval from the worker instance.
        /// </summary>
        /// <param name="worker">The worker instance.</param>
        /// <returns>The cleanup interval.</returns>
        private static TimeSpan GetCleanupInterval(this CacheMaintenanceWorker worker)
        {
            // Use reflection to access the private field since we can't modify the class
            var field = typeof(CacheMaintenanceWorker).GetField(
                "_cleanupInterval",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (TimeSpan)field?.GetValue(worker)!;
        }

        /// <summary>
        /// Gets the memory warning threshold from the worker instance.
        /// </summary>
        /// <param name="worker">The worker instance.</param>
        /// <returns>The memory warning threshold.</returns>
        private static float GetMemoryWarningThreshold(this CacheMaintenanceWorker worker)
        {
            // Use reflection to access the private field since we can't modify the class
            var field = typeof(CacheMaintenanceWorker).GetField(
                "_memoryWarningThreshold",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)field?.GetValue(worker)!;
        }
    }
}
