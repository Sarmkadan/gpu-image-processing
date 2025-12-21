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

            // CacheMaintenanceWorker has no public properties to validate beyond null check
            // All constructor parameter validation is performed in the constructor itself

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified cache maintenance worker is valid.
        /// </summary>
        /// <param name="value">The worker instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this CacheMaintenanceWorker value)
        {
            return value.Validate().Count == 0;
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
    }
}
