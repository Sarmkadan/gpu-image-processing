#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides validation helpers for <see cref="MetricsPublisher"/> instances.
    /// </summary>
    public static class MetricsPublisherValidation
    {
        /// <summary>
        /// Validates a <see cref="MetricsPublisher"/> instance.
        /// </summary>
        /// <param name="value">The metrics publisher to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MetricsPublisher value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate that at least one endpoint is registered
            var endpointCount = GetEndpointCount(value);
            if (endpointCount == 0)
            {
                errors.Add("MetricsPublisher must have at least one registered endpoint.");
            }

            // Validate buffer state
            var bufferSize = GetBufferSize(value);
            if (bufferSize <= 0)
            {
                errors.Add("MetricsPublisher buffer size must be positive.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="MetricsPublisher"/> instance is valid.
        /// </summary>
        /// <param name="value">The metrics publisher to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid(this MetricsPublisher value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="MetricsPublisher"/> instance is valid.
        /// </summary>
        /// <param name="value">The metrics publisher to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid.</exception>
        public static void EnsureValid(this MetricsPublisher value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"MetricsPublisher is not valid. Validation errors:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }

        private static int GetEndpointCount(this MetricsPublisher publisher)
        {
            var field = typeof(MetricsPublisher).GetField(
                "_endpoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                var endpoints = field.GetValue(publisher) as System.Collections.ICollection;
                return endpoints?.Count ?? 0;
            }

            return 0;
        }

        private static int GetBufferSize(this MetricsPublisher publisher)
        {
            var field = typeof(MetricsPublisher).GetField(
                "_bufferSize",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return (int?)field.GetValue(publisher) ?? 0;
            }

            return 0;
        }
    }
}
