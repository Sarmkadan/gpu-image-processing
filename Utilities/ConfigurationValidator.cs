#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Validates configuration values and provides default settings.
    /// Ensures application configuration is valid before runtime use.
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Validates required configuration keys are present
        /// </summary>
        public static ValidationResult ValidateConfiguration(Dictionary<string, string> config, params string[] requiredKeys)
        {
            var missingKeys = new List<string>();

            foreach (var key in requiredKeys)
            {
                if (!config.ContainsKey(key) || string.IsNullOrWhiteSpace(config[key]))
                    missingKeys.Add(key);
            }

            if (missingKeys.Count > 0)
            {
                return ValidationResult.Failure(
                    $"Missing required configuration keys: {string.Join(", ", missingKeys)}"
                );
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates an integer configuration value is within bounds
        /// </summary>
        public static ValidationResult ValidateIntegerRange(
            string value,
            int minimum,
            int maximum,
            string parameterName)
        {
            if (!int.TryParse(value, out var intValue))
                return ValidationResult.Failure($"{parameterName} must be a valid integer");

            if (intValue < minimum || intValue > maximum)
                return ValidationResult.Failure(
                    $"{parameterName} must be between {minimum} and {maximum}, got {intValue}"
                );

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates a timeout duration is reasonable
        /// </summary>
        public static ValidationResult ValidateTimeout(
            TimeSpan timeout,
            TimeSpan minimum,
            TimeSpan maximum,
            string parameterName = "Timeout")
        {
            if (timeout < minimum)
                return ValidationResult.Failure(
                    $"{parameterName} ({timeout.TotalSeconds:F1}s) is below minimum ({minimum.TotalSeconds:F1}s)"
                );

            if (timeout > maximum)
                return ValidationResult.Failure(
                    $"{parameterName} ({timeout.TotalSeconds:F1}s) exceeds maximum ({maximum.TotalSeconds:F1}s)"
                );

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates batch size is reasonable
        /// </summary>
        public static ValidationResult ValidateBatchSize(int batchSize)
        {
            if (batchSize <= 0)
                return ValidationResult.Failure("Batch size must be positive");

            if (batchSize > 10000)
                return ValidationResult.Failure("Batch size exceeds maximum of 10000");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates memory size specification
        /// </summary>
        public static ValidationResult ValidateMemorySize(string sizeSpec, long minimumBytes)
        {
            try
            {
                var bytes = DataConversionUtilities.ParseFileSize(sizeSpec);
                if (bytes < minimumBytes)
                    return ValidationResult.Failure(
                        $"Memory size ({sizeSpec}) is below minimum ({DataConversionUtilities.FormatFileSize(minimumBytes)})"
                    );

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Invalid memory size specification: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates a URL is properly formatted
        /// </summary>
        public static ValidationResult ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return ValidationResult.Failure("URL cannot be empty");

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return ValidationResult.Failure($"Invalid URL format: {url}");

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return ValidationResult.Failure("URL must use HTTP or HTTPS protocol");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates environment variable is set
        /// </summary>
        public static ValidationResult ValidateEnvironmentVariable(string variableName, bool required = true)
        {
            var value = Environment.GetEnvironmentVariable(variableName);

            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    return ValidationResult.Failure($"Required environment variable '{variableName}' is not set");

                return ValidationResult.Success();
            }

            return ValidationResult.Success();
        }

        /// <summary>
        /// Gets a configuration value with fallback to default
        /// </summary>
        public static T GetConfigurationValue<T>(
            Dictionary<string, string> config,
            string key,
            T defaultValue)
        {
            if (!config.TryGetValue(key, out var value))
                return defaultValue;

            try
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)int.Parse(value);

                if (typeof(T) == typeof(bool))
                    return (T)(object)bool.Parse(value);

                if (typeof(T) == typeof(double))
                    return (T)(object)double.Parse(value);

                if (typeof(T) == typeof(string))
                    return (T)(object)value;

                if (typeof(T).IsEnum)
                    return (T)Enum.Parse(typeof(T), value);

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Validates all configuration values are syntactically correct
        /// </summary>
        public static List<ConfigurationValidationError> ValidateAllConfiguration(Dictionary<string, string> config)
        {
            var errors = new List<ConfigurationValidationError>();

            // Validate common configuration patterns
            var validationRules = new[]
            {
                ("MaxBatchSize", (string v) => int.TryParse(v, out var i) && i > 0 && i <= 10000),
                ("TimeoutSeconds", (string v) => int.TryParse(v, out var i) && i > 0 && i <= 3600),
                ("MaxRetries", (string v) => int.TryParse(v, out var i) && i >= 0 && i <= 10),
                ("LogLevel", (string v) => new[] { "Debug", "Info", "Warning", "Error" }.Contains(v))
            };

            foreach (var (key, validator) in validationRules)
            {
                if (config.TryGetValue(key, out var value))
                {
                    if (!validator(value))
                    {
                        errors.Add(new ConfigurationValidationError
                        {
                            Key = key,
                            Value = value,
                            Message = $"Invalid value for {key}: {value}"
                        });
                    }
                }
            }

            return errors;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failure(string message) => new() { IsValid = false, Message = message };
    }

    public class ConfigurationValidationError
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
    }
}
