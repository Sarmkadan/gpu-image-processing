#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides validation helper methods for <see cref="ValidationResult"/>.
    /// </summary>
    public static class ValidationUtilitiesValidation
    {
        /// <summary>
        /// Retrieves a read-only list of validation error messages.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <returns>A read-only list of error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        public static IReadOnlyList<string> Validate(this ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.Errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the validation result indicates success.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        public static bool IsValid(this ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.IsValid;
        }

        /// <summary>
        /// Ensures the validation result is valid, throwing an exception if not.
        /// </summary>
        /// <param name="result">The validation result.</param>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        /// <exception cref="ArgumentException">Thrown if result is not valid.</exception>
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
