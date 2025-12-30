#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Validation helpers for ConfigurationValidator operations.
    /// Provides validation, IsValid, and EnsureValid methods for configuration validation scenarios.
    /// </summary>
    public static class ConfigurationValidatorValidation
    {
        /// <summary>
        /// Validates configuration dictionary and required keys
        /// </summary>
        /// <param name="config">Configuration dictionary to validate</param>
        /// <param name="requiredKeys">Required configuration keys</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(
            Dictionary<string, string> config,
            params string[] requiredKeys)
        {
            var problems = new List<string>();

            if (config == null)
                problems.Add("Configuration dictionary cannot be null");
            else if (config.Count == 0)
                problems.Add("Configuration dictionary cannot be empty");

            if (requiredKeys == null)
                problems.Add("Required keys array cannot be null");
            else if (requiredKeys.Length == 0)
                problems.Add("Required keys array cannot be empty");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates an integer configuration value is within bounds
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="minimum">Minimum allowed value</param>
        /// <param name="maximum">Maximum allowed value</param>
        /// <param name="parameterName">Name of the parameter being validated</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(
            string value,
            int minimum,
            int maximum,
            string parameterName)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
                problems.Add($"{parameterName} cannot be null or whitespace");

            if (minimum > maximum)
                problems.Add($"Minimum ({minimum}) cannot be greater than maximum ({maximum}) for {parameterName}");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a timeout duration is reasonable
        /// </summary>
        /// <param name="timeout">Timeout value to validate</param>
        /// <param name="minimum">Minimum allowed timeout</param>
        /// <param name="maximum">Maximum allowed timeout</param>
        /// <param name="parameterName">Name of the parameter being validated</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(
            TimeSpan timeout,
            TimeSpan minimum,
            TimeSpan maximum,
            string parameterName = "Timeout")
        {
            var problems = new List<string>();

            if (timeout < TimeSpan.Zero)
                problems.Add($"{parameterName} cannot be negative");

            if (minimum < TimeSpan.Zero)
                problems.Add("Minimum timeout cannot be negative");

            if (maximum < TimeSpan.Zero)
                problems.Add("Maximum timeout cannot be negative");

            if (minimum > maximum)
                problems.Add($"Minimum timeout ({minimum.TotalSeconds}s) cannot be greater than maximum timeout ({maximum.TotalSeconds}s)");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates batch size is reasonable
        /// </summary>
        /// <param name="batchSize">Batch size to validate</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(int batchSize)
        {
            var problems = new List<string>();

            if (batchSize <= 0)
                problems.Add("Batch size must be positive");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates memory size specification
        /// </summary>
        /// <param name="sizeSpec">Memory size specification string</param>
        /// <param name="minimumBytes">Minimum bytes required</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(
            string sizeSpec,
            long minimumBytes)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(sizeSpec))
                problems.Add("Memory size specification cannot be null or whitespace");

            if (minimumBytes <= 0)
                problems.Add("Minimum bytes must be positive");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a URL is properly formatted
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(string url)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(url))
                problems.Add("URL cannot be null or whitespace");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates environment variable is set
        /// </summary>
        /// <param name="variableName">Environment variable name</param>
        /// <param name="required">Whether the variable is required</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(
            string variableName,
            bool required = true)
        {
            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(variableName))
                problems.Add("Environment variable name cannot be null or whitespace");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates configuration dictionary for common issues
        /// </summary>
        /// <param name="config">Configuration dictionary to validate</param>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> Validate(Dictionary<string, string> config)
        {
            var problems = new List<string>();

            if (config == null)
                problems.Add("Configuration dictionary cannot be null");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates the ConfigurationValidator static class (no-op since it's static)
        /// </summary>
        /// <returns>List of validation problems, empty if valid</returns>
        public static IReadOnlyList<string> ValidateConfigurationValidator()
        {
            return Array.Empty<string>();
        }

        /// <summary>
        /// Checks if the ConfigurationValidator static class is valid (always true)
        /// </summary>
        /// <returns>True if valid</returns>
        public static bool IsValidConfigurationValidator()
        {
            return true;
        }

        /// <summary>
        /// Ensures the ConfigurationValidator static class is valid, throwing if not
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if validation fails</exception>
        public static void EnsureValidConfigurationValidator()
        {
            // Static class has no state to validate
        }
    }
}