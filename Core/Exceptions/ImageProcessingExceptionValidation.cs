#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Provides validation helpers for <see cref="ImageProcessingException"/> instances
    /// </summary>
    public static class ImageProcessingExceptionValidation
    {
        /// <summary>
        /// Validates an <see cref="ImageProcessingException"/> instance
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <returns>A list of validation problems; empty list if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static IReadOnlyList<string> Validate(this ImageProcessingException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate ErrorCode
            if (string.IsNullOrWhiteSpace(value.ErrorCode))
            {
                problems.Add("ErrorCode must not be null or whitespace");
            }

            // Validate ErrorCode_Numeric (if set)
            if (value.ErrorCode_Numeric is not null && value.ErrorCode_Numeric.Value < 0)
            {
                problems.Add("ErrorCode_Numeric must be a non-negative integer when set");
            }

            // Validate OccurredAt
            if (value.OccurredAt == default)
            {
                problems.Add("OccurredAt must be set to a non-default DateTime value");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether an <see cref="ImageProcessingException"/> instance is valid
        /// </summary>
        /// <param name="value">The exception to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static bool IsValid(this ImageProcessingException value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that an <see cref="ImageProcessingException"/> instance is valid
        /// </summary>
        /// <param name="value">The exception to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        /// <exception cref="ArgumentException">Thrown when value is not valid, containing the validation problems</exception>
        public static void EnsureValid(this ImageProcessingException value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ImageProcessingException is not valid. Problems:\n{string.Join("\n", problems)}",
                    nameof(value));
            }
        }
    }
}
