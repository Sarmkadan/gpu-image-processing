#nullable enable
using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides extension methods for <see cref="ValidationResult"/> to simplify validation logic.
    /// </summary>
    public static class ValidationUtilitiesValidation
    {
        /// <summary>
        /// Retrieves a read-only list of validation error messages from the validation result.
        /// </summary>
        /// <param name="result">The validation result to extract errors from.</param>
        /// <returns>A read-only list of error messages. Empty if validation succeeded.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.Errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the validation result indicates success.
        /// </summary>
        /// <param name="result">The validation result to check.</param>
        /// <returns>True if the validation succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static bool IsValid(this ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsValid;
        }

        /// <summary>
        /// Ensures the validation result is valid, throwing an exception if validation failed.
        /// </summary>
        /// <param name="result">The validation result to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="result"/> indicates validation failure.</exception>
        public static void EnsureValid(this ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            if (!result.IsValid)
            {
                throw new ArgumentException($"Validation failed: {result.ErrorMessage}");
            }
        }
    }
}
