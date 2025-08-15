#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides validation helpers for <see cref="SystemPerformanceMonitoringService"/> instances.
    /// </summary>
    public static class SystemPerformanceMonitoringServiceValidation
    {
        /// <summary>
        /// Validates a <see cref="SystemPerformanceMonitoringService"/> instance.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this SystemPerformanceMonitoringService? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate internal state consistency
            // Since we can't directly access private fields, we validate via public API
            try
            {
                int operationCount = value.GetMonitoredOperationCount();
                if (operationCount < 0)
                {
                    errors.Add("Monitored operation count cannot be negative.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to validate monitored operation count: {ex.Message}");
            }

            // Validate that GetAllStatistics returns valid data structure
            try
            {
                var allStats = value.GetAllStatistics();
                if (allStats == null)
                {
                    errors.Add("GetAllStatistics returned null dictionary.");
                }
                else
                {
                    foreach (var kvp in allStats)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Key))
                        {
                            errors.Add("Operation statistics dictionary contains entry with null or whitespace operation name.");
                        }

                        var stats = kvp.Value;
                        if (stats == null)
                        {
                            errors.Add($"Statistics for operation '{kvp.Key}' is null.");
                            continue;
                        }

                        if (stats.TotalMeasurements < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': TotalMeasurements cannot be negative.");
                        }

                        if (stats.MinMs < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': MinMs cannot be negative.");
                        }

                        if (stats.MaxMs < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': MaxMs cannot be negative.");
                        }

                        if (stats.AverageMs < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': AverageMs cannot be negative.");
                        }

                        if (stats.MedianMs < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': MedianMs cannot be negative.");
                        }

                        if (stats.P95Ms < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': P95Ms cannot be negative.");
                        }

                        if (stats.P99Ms < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': P99Ms cannot be negative.");
                        }

                        if (stats.ThroughputPerSecond < 0)
                        {
                            errors.Add($"Operation '{kvp.Key}': ThroughputPerSecond cannot be negative.");
                        }

                        // Validate measurement count consistency
                        if (stats.TotalMeasurements > 0 && stats.MinMs > stats.MaxMs)
                        {
                            errors.Add($"Operation '{kvp.Key}': MinMs ({stats.MinMs}) is greater than MaxMs ({stats.MaxMs}).");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to validate statistics collection: {ex.Message}");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="SystemPerformanceMonitoringService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        public static bool IsValid(this SystemPerformanceMonitoringService? value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="SystemPerformanceMonitoringService"/> instance is valid.
        /// </summary>
        /// <param name="value">The service instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
        public static void EnsureValid(this SystemPerformanceMonitoringService? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"SystemPerformanceMonitoringService validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
            }
        }
    }
}