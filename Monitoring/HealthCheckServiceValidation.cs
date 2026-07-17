#nullable enable

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Monitoring
{
    /// <summary>
    /// Provides validation helpers for health monitoring types
    /// </summary>
    public static class HealthCheckServiceValidation
    {
        /// <summary>
        /// Validates a <see cref="ComponentHealth"/> instance
        /// </summary>
        /// <param name="value">The component health to validate</param>
        /// <returns>List of validation errors; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static IReadOnlyList<string> Validate(this ComponentHealth? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            if (value.Status == default || value.Status == HealthStatus.Unknown)
                errors.Add("ComponentHealth.Status must be set to a valid HealthStatus value other than Unknown");

            if (string.IsNullOrWhiteSpace(value.Message))
                errors.Add("ComponentHealth.Message cannot be null or empty");

            if (value.CheckedAt == default)
                errors.Add("ComponentHealth.CheckedAt must be set to a valid DateTime");

            if (value.Details == null)
                errors.Add("ComponentHealth.Details cannot be null");

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates a <see cref="HealthCheckResult"/> instance
        /// </summary>
        /// <param name="value">The health check result to validate</param>
        /// <returns>List of validation errors; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static IReadOnlyList<string> Validate(this HealthCheckResult? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            if (value.Status == default)
                errors.Add("HealthCheckResult.Status must be set to a valid HealthStatus value");

            if (value.Timestamp == default)
                errors.Add("HealthCheckResult.Timestamp must be set to a valid DateTime");

            if (value.Components == null)
                errors.Add("HealthCheckResult.Components cannot be null");

            if (string.IsNullOrWhiteSpace(value.Summary))
                errors.Add("HealthCheckResult.Summary cannot be null or empty");

            if (value.Components != null)
            {
                foreach (var kvp in value.Components)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                        errors.Add("HealthCheckResult.Components contains an entry with null or empty key");

                    if (kvp.Value == null)
                        errors.Add($"HealthCheckResult.Components['{kvp.Key}'] cannot be null");
                    else
                    {
                        // Validate each component health
                        var componentErrors = kvp.Value.Validate();
                        if (componentErrors.Count > 0)
                            errors.AddRange(componentErrors);
                    }
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// MemoryHealthCheck does not expose its thresholds publicly, so only basic null validation is performed
        /// </summary>
        /// <param name="value">The memory health check to validate</param>
        /// <returns>List of validation errors; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static IReadOnlyList<string> Validate(this MemoryHealthCheck? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Array.Empty<string>();
        }

        /// <summary>
        /// ResponseTimeHealthCheck does not expose its threshold publicly, so only basic null validation is performed
        /// </summary>
        /// <param name="value">The response time health check to validate</param>
        /// <returns>List of validation errors; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static IReadOnlyList<string> Validate(this ResponseTimeHealthCheck? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ComponentHealth"/> is valid
        /// </summary>
        /// <param name="value">The component health to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static bool IsValid(this ComponentHealth? value)
        {
            return value?.Validate() is { Count: 0 };
        }

        /// <summary>
        /// Determines whether the specified <see cref="HealthCheckResult"/> is valid
        /// </summary>
        /// <param name="value">The health check result to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static bool IsValid(this HealthCheckResult? value)
        {
            return value?.Validate() is { Count: 0 };
        }

        /// <summary>
        /// Determines whether the specified <see cref="MemoryHealthCheck"/> is valid
        /// </summary>
        /// <param name="value">The memory health check to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static bool IsValid(this MemoryHealthCheck? value)
        {
            return value != null;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ResponseTimeHealthCheck"/> is valid
        /// </summary>
        /// <param name="value">The response time health check to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static bool IsValid(this ResponseTimeHealthCheck? value)
        {
            return value != null;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ComponentHealth"/> is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The component health to validate</param>
        /// <exception cref="ArgumentException">Thrown if value is not valid</exception>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static void EnsureValid(this ComponentHealth? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ComponentHealth is not valid. Problems: {string.Join("; ", errors)}",
                    nameof(value));
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="HealthCheckResult"/> is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The health check result to validate</param>
        /// <exception cref="ArgumentException">Thrown if value is not valid</exception>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static void EnsureValid(this HealthCheckResult? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"HealthCheckResult is not valid. Problems: {string.Join("; ", errors)}",
                    nameof(value));
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="MemoryHealthCheck"/> is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The memory health check to validate</param>
        /// <exception cref="ArgumentException">Thrown if value is not valid</exception>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static void EnsureValid(this MemoryHealthCheck? value)
        {
            ArgumentNullException.ThrowIfNull(value);
        }

        /// <summary>
        /// Ensures that the specified <see cref="ResponseTimeHealthCheck"/> is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The response time health check to validate</param>
        /// <exception cref="ArgumentException">Thrown if value is not valid</exception>
        /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
        public static void EnsureValid(this ResponseTimeHealthCheck? value)
        {
            ArgumentNullException.ThrowIfNull(value);
        }
    }
}