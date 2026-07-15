using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides validation helpers for <see cref="TelemetryService"/> instances.
    /// </summary>
    public static class TelemetryServiceValidation
    {
        /// <summary>
        /// Validates a <see cref="TelemetryService"/> instance.
        /// </summary>
        /// <param name="value">The telemetry service to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this TelemetryService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate that the service instance is in a valid state
            // TelemetryService itself doesn't have direct public properties to validate
            // beyond basic null checks, so we validate based on its actual state
            try
            {
                // If we can access the service without issues, it's valid
                // TelemetryService is always valid as long as it's not null
                // since it has no required properties to set
            }
            catch (Exception ex)
            {
                problems.Add($"TelemetryService instance is in an invalid state: {ex.Message}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="TelemetryService"/> instance is valid.
        /// </summary>
        /// <param name="value">The telemetry service to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this TelemetryService value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="TelemetryService"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The telemetry service to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
        public static void EnsureValid(this TelemetryService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"TelemetryService is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}