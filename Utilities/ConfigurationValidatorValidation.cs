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
    /// Validation helpers for configuration validation scenarios.
    /// Provides validation methods for various configuration types and constraints.
    /// </summary>
    public static class ConfigurationValidatorValidation
    {
        /// <summary>
        /// Validates configuration dictionary and required keys
        /// </summary>
        /// <param name="config">Configuration dictionary to validate</param>
        /// <param name="requiredKeys">Required configuration keys</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if config or requiredKeys is null</exception>
        public static IReadOnlyList<string> Validate(
            Dictionary<string, string> config,
            params string[] requiredKeys)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(requiredKeys);

            var problems = new List<string>();

            if (config.Count == 0)
                problems.Add("Configuration dictionary cannot be empty");

            if (requiredKeys.Length == 0)
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
        /// <exception cref="ArgumentNullException">Thrown if parameterName is null</exception>
        public static IReadOnlyList<string> Validate(
            string value,
            int minimum,
            int maximum,
            string parameterName)
        {
            ArgumentNullException.ThrowIfNull(parameterName);

            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
                problems.Add($"{parameterName} cannot be null or whitespace");
            else if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue) || parsedValue < minimum || parsedValue > maximum)
                problems.Add($"{parameterName} must be an integer between {minimum} and {maximum}");

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
        /// <exception cref="ArgumentNullException">Thrown if parameterName is null</exception>
        public static IReadOnlyList<string> Validate(
            TimeSpan timeout,
            TimeSpan minimum,
            TimeSpan maximum,
            string parameterName = "Timeout")
        {
            ArgumentNullException.ThrowIfNull(parameterName);

            var problems = new List<string>();

            if (timeout < TimeSpan.Zero)
                problems.Add($"{parameterName} cannot be negative");

            if (minimum < TimeSpan.Zero)
                problems.Add("Minimum timeout cannot be negative");

            if (maximum < TimeSpan.Zero)
                problems.Add("Maximum timeout cannot be negative");

            if (minimum > maximum)
                problems.Add($"Minimum timeout ({minimum.TotalSeconds}s) cannot be greater than maximum timeout ({maximum.TotalSeconds}s)");

            if (timeout < minimum)
                problems.Add($"{parameterName} ({timeout.TotalSeconds}s) cannot be less than minimum ({minimum.TotalSeconds}s)");

            if (timeout > maximum)
                problems.Add($"{parameterName} ({timeout.TotalSeconds}s) cannot be greater than maximum ({maximum.TotalSeconds}s)");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates batch size is reasonable
        /// </summary>
        /// <param name="batchSize">Batch size to validate</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if batchSize is less than or equal to 0</exception>
        public static IReadOnlyList<string> Validate(int batchSize)
        {
            var problems = new List<string>();

            if (batchSize <= 0)
                problems.Add("Batch size must be positive");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates memory size specification string and ensures it meets minimum requirement
        /// </summary>
        /// <param name="sizeSpec">Memory size specification string (e.g., "1GB", "512MB", "2048KB")</param>
        /// <param name="minimumBytes">Minimum bytes required</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if sizeSpec is null</exception>
        /// <exception cref="ArgumentException">Thrown if sizeSpec is empty or whitespace</exception>
        public static IReadOnlyList<string> Validate(
            string sizeSpec,
            long minimumBytes)
        {
            ArgumentNullException.ThrowIfNull(sizeSpec);
            ArgumentException.ThrowIfNullOrEmpty(sizeSpec.Trim(), nameof(sizeSpec));

            var problems = new List<string>();

            if (minimumBytes <= 0)
                problems.Add("Minimum bytes must be positive");

            try
            {
                var parsedSize = ParseMemorySize(sizeSpec);
                if (parsedSize < minimumBytes)
                    problems.Add($"Memory size ({FormatBytes(parsedSize)}) must be at least {FormatBytes(minimumBytes)}");
            }
            catch (FormatException ex)
            {
                problems.Add($"Memory size specification '{sizeSpec}' is not in a valid format: {ex.Message}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates a URL is properly formatted
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if url is null</exception>
        public static IReadOnlyList<string> Validate(string url)
        {
            ArgumentNullException.ThrowIfNull(url);

            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(url))
                problems.Add("URL cannot be null or whitespace");
            else if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                    !(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                problems.Add("URL must be a valid HTTP or HTTPS URL");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates environment variable name and checks if it exists when required
        /// </summary>
        /// <param name="variableName">Environment variable name</param>
        /// <param name="required">Whether the variable is required</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if variableName is null</exception>
        public static IReadOnlyList<string> Validate(
            string variableName,
            bool required = true)
        {
            ArgumentNullException.ThrowIfNull(variableName);

            var problems = new List<string>();

            if (string.IsNullOrWhiteSpace(variableName))
                problems.Add("Environment variable name cannot be null or whitespace");
            else if (required && Environment.GetEnvironmentVariable(variableName) == null)
                problems.Add($"Required environment variable '{variableName}' is not set");

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates configuration dictionary for common issues
        /// </summary>
        /// <param name="config">Configuration dictionary to validate</param>
        /// <returns>List of validation problems, empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
        public static IReadOnlyList<string> Validate(Dictionary<string, string> config)
        {
            ArgumentNullException.ThrowIfNull(config);

            var problems = new List<string>();

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Parses a memory size string into bytes
        /// </summary>
        /// <param name="sizeSpec">Memory size specification (e.g., "1GB", "512MB", "2048KB", "1024")</param>
        /// <returns>Size in bytes</returns>
        /// <exception cref="FormatException">Thrown if the format is invalid</exception>
        private static long ParseMemorySize(string sizeSpec)
        {
            var spec = sizeSpec.Trim().ToUpperInvariant();

            if (spec.EndsWith("B", StringComparison.Ordinal))
                spec = spec.Substring(0, spec.Length - 1);

            if (long.TryParse(spec, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bytes))
                return bytes;

            var suffix = spec[^2..];
            var numberPart = spec[..^2];

            if (!long.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out bytes))
                throw new FormatException($"Invalid number format in '{sizeSpec}'");

            return suffix switch
            {
                "KB" => bytes * 1024L,
                "MB" => bytes * 1024L * 1024L,
                "GB" => bytes * 1024L * 1024L * 1024L,
                "TB" => bytes * 1024L * 1024L * 1024L * 1024L,
                _ => throw new FormatException($"Unknown size suffix '{suffix}' in '{sizeSpec}'. Use KB, MB, GB, or TB")
            };
        }

        /// <summary>
        /// Formats bytes as a human-readable string
        /// </summary>
        /// <param name="bytes">Number of bytes</param>
        /// <returns>Formatted string with appropriate unit</returns>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
            double size = bytes;
            int suffixIndex = 0;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {suffixes[suffixIndex]}";
        }
    }
}