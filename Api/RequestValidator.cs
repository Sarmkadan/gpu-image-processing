#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// Validates API request parameters and payloads for correctness and security.
    /// Provides fluent validation API for building validation rules.
    /// </summary>
    public class RequestValidator
    {
        private readonly List<ValidationRule> _rules;

        public RequestValidator()
        {
            _rules = new List<ValidationRule>();
        }

        /// <summary>
        /// Adds a validation rule for a required field
        /// </summary>
        public RequestValidator RequireField(string fieldName, Func<object, bool> validator = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(fieldName);
            _rules.Add(new ValidationRule
            {
                FieldName = fieldName,
                Validator = validator ?? (v => v != null),
                ErrorMessage = $"{fieldName} is required"
            });
            return this;
        }

        /// <summary>
        /// Adds a validation rule for string length
        /// </summary>
        public RequestValidator ValidateStringLength(
            string fieldName,
            int minLength,
            int maxLength)
        {
            ArgumentException.ThrowIfNullOrEmpty(fieldName);
            _rules.Add(new ValidationRule
            {
                FieldName = fieldName,
                Validator = v =>
                {
                    // Fix: Explicitly handle null or non-string values.
                    // If the field is not required, null or non-string values should pass this length validation.
                    if (v == null || v is not string str)
                        return true;
                    return str.Length >= minLength && str.Length <= maxLength;
                },
                ErrorMessage = $"{fieldName} must be between {minLength} and {maxLength} characters"
            });
            return this;
        }

        /// <summary>
        /// Adds a validation rule for numeric range
        /// </summary>
        public RequestValidator ValidateRange(
            string fieldName,
            double minimum,
            double maximum)
        {
            ArgumentException.ThrowIfNullOrEmpty(fieldName);
            if (minimum > maximum)
                throw new ArgumentException($"Minimum value {minimum} cannot be greater than maximum value {maximum}", nameof(minimum));
            _rules.Add(new ValidationRule
            {
                FieldName = fieldName,
                Validator = v =>
                {
                    // Fix: Explicitly handle null values.
                    // If the field is not required, null values should pass this range validation.
                    if (v == null)
                        return true;

                    if (double.TryParse(v.ToString(), out var d))
                        return d >= minimum && d <= maximum;
                    return false;
                },
                ErrorMessage = $"{fieldName} must be between {minimum} and {maximum}"
            });
            return this;
        }

        /// <summary>
        /// Adds a validation rule for regex pattern
        /// </summary>
        public RequestValidator ValidatePattern(
            string fieldName,
            string regexPattern)
        {
            ArgumentException.ThrowIfNullOrEmpty(fieldName);
            ArgumentException.ThrowIfNullOrEmpty(regexPattern);
            _rules.Add(new ValidationRule
            {
                FieldName = fieldName,
                Validator = v =>
                {
                    // Fix: Explicitly handle null or non-string values.
                    // If the field is not required, null or non-string values should pass this pattern validation.
                    if (v == null || v is not string str)
                        return true;
                    var regex = new Regex(regexPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                    return regex.IsMatch(str);
                },
                ErrorMessage = $"{fieldName} must match the pattern {regexPattern}"
            });
            return this;
        }

        /// <summary>
        /// Adds a validation rule to check if value is one of the allowed values
        /// </summary>
        public RequestValidator ValidateOneOf(
            string fieldName,
            params object[] allowedValues)
        {
            ArgumentException.ThrowIfNullOrEmpty(fieldName);
            if (allowedValues == null || allowedValues.Length == 0)
                throw new ArgumentException("Allowed values array must not be null or empty", nameof(allowedValues));
            _rules.Add(new ValidationRule
            {
                FieldName = fieldName,
                Validator = v =>
                {
                    // Fix: Explicitly handle null values.
                    // If the field is not required, null values should pass this validation.
                    if (v == null)
                        return true;
                    return allowedValues.Contains(v);
                },
                ErrorMessage = $"{fieldName} must be one of the allowed values: {string.Join(", ", allowedValues)}"
            });
            return this;
        }

        
        /// <summary>
        /// Adds a custom validation rule
        /// </summary>
        public RequestValidator AddRule(
            string fieldName,
            Func<object, bool> validator,
            string errorMessage)
        {
            ArgumentException.ThrowIfNullOrEmpty(fieldName);
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentException.ThrowIfNullOrEmpty(errorMessage);
            _rules.Add(new ValidationRule
            {
                FieldName = fieldName,
                Validator = validator,
                ErrorMessage = errorMessage
            });
            return this;
        }

        /// <summary>
        /// Validates a request object against all registered rules
        /// </summary>
        public ValidationResult Validate(Dictionary<string, object> requestData)
        {
            ArgumentNullException.ThrowIfNull(requestData);
            var errors = new List<string>();

            foreach (var rule in _rules)
            {
                if (!requestData.TryGetValue(rule.FieldName, out var value))
                {
                    errors.Add($"Missing field: {rule.FieldName}");
                    continue;
                }

                try
                {
                    if (!rule.Validator(value))
                        errors.Add(rule.ErrorMessage);
                }
                catch (Exception ex)
                {
                    errors.Add($"Validation error in {rule.FieldName}: {ex.Message}");
                }
            }

            if (errors.Count > 0)
                return ValidationResult.Failure(errors);

            return ValidationResult.Success();
        }

        /// <summary>
        /// Sanitizes request data to prevent injection attacks
        /// </summary>
        public static Dictionary<string, object> SanitizeRequest(Dictionary<string, object> requestData)
        {
            ArgumentNullException.ThrowIfNull(requestData);
            var sanitized = new Dictionary<string, object>();

            foreach (var kvp in requestData)
            {
                if (kvp.Value is string str)
                {
                    sanitized[kvp.Key] = SanitizeString(str);
                }
                else if (kvp.Value is Dictionary<string, object> nested)
                {
                    sanitized[kvp.Key] = SanitizeRequest(nested);
                }
                else if (kvp.Value is List<object> list)
                {
                    var sanitizedList = new List<object>();
                    foreach (var item in list)
                    {
                        if (item is string itemStr)
                            sanitizedList.Add(SanitizeString(itemStr));
                        else if (item is Dictionary<string, object> itemDict)
                            sanitizedList.Add(SanitizeRequest(itemDict));
                        else
                            sanitizedList.Add(item);
                    }
                    sanitized[kvp.Key] = sanitizedList;
                }
                else
                {
                    sanitized[kvp.Key] = kvp.Value;
                }
            }

            return sanitized;
        }

        private static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remove potentially dangerous characters
            var dangerous = new[] { '<', '>', '"', '\'', '&', ';', '|', '`', '$' };
            var sanitized = input;

            foreach (var ch in dangerous)
                sanitized = sanitized.Replace(ch.ToString(), "");

            return sanitized.Trim();
        }

        private class ValidationRule
        {
            public string FieldName { get; set; }
            public Func<object, bool> Validator { get; set; }
            public string ErrorMessage { get; set; }
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }

        public static ValidationResult Success() => new()
        {
            IsValid = true,
            Errors = new List<string>()
        };

        public static ValidationResult Failure(List<string> errors) => new()
        {
            IsValid = false,
            Errors = errors
        };
    }
}
