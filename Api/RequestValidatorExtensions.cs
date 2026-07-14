#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// Extension methods that enhance <see cref="RequestValidator"/> with common validation scenarios.
    /// </summary>
    public static class RequestValidatorExtensions
    {
        /// <summary>
        /// Adds a rule that ensures the specified field contains a non‑empty string.
        /// </summary>
        /// <param name="validator">The validator to extend.</param>
        /// <param name="fieldName">The name of the field to validate.</param>
        /// <returns>The same <see cref="RequestValidator"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validator"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fieldName"/> is <c>null</c> or empty.</exception>
        public static RequestValidator RequireNonEmptyString(this RequestValidator validator, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            return validator.RequireField(fieldName, v => v is string s && !string.IsNullOrWhiteSpace(s));
        }

        /// <summary>
        /// Adds a rule that validates the specified field contains a value that can be parsed as a <see cref="Guid"/>.
        /// </summary>
        /// <param name="validator">The validator to extend.</param>
        /// <param name="fieldName">The name of the field to validate.</param>
        /// <returns>The same <see cref="RequestValidator"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validator"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fieldName"/> is <c>null</c> or empty.</exception>
        public static RequestValidator ValidateGuid(this RequestValidator validator, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            return validator.AddRule(
                fieldName,
                v => Guid.TryParse(v?.ToString(), out _),
                $"{fieldName} must be a valid GUID");
        }

        /// <summary>
        /// Adds a rule that validates the specified field contains a value that matches a simple e‑mail pattern.
        /// </summary>
        /// <param name="validator">The validator to extend.</param>
        /// <param name="fieldName">The name of the field to validate.</param>
        /// <returns>The same <see cref="RequestValidator"/> instance for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validator"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fieldName"/> is <c>null</c> or empty.</exception>
        public static RequestValidator ValidateEmail(this RequestValidator validator, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            // RFC‑5322‑compatible but deliberately simple for performance.
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

            return validator.AddRule(
                fieldName,
                v => v is string s && regex.IsMatch(s),
                $"{fieldName} must be a valid e‑mail address");
        }

        /// <summary>
        /// Sanitizes the supplied request data and validates it in a single step.
        /// </summary>
        /// <param name="validator">The validator to use for validation.</param>
        /// <param name="requestData">The raw request data.</param>
        /// <returns>A <see cref="ValidationResult"/> representing the outcome of the validation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validator"/> or <paramref name="requestData"/> is <c>null</c>.</exception>
        public static ValidationResult ValidateAndSanitize(this RequestValidator validator, Dictionary<string, object> requestData)
        {
            ArgumentNullException.ThrowIfNull(validator);
            ArgumentNullException.ThrowIfNull(requestData);

            var sanitized = RequestValidator.SanitizeRequest(requestData);
            return validator.Validate(sanitized);
        }
    }
}
