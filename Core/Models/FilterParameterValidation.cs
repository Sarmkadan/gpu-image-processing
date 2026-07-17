#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides validation helpers for <see cref="FilterParameter"/> instances
    /// </summary>
    public static class FilterParameterValidation
    {
        /// <summary>
        /// Validates a <see cref="FilterParameter"/> instance and returns a list of human-readable validation errors
        /// </summary>
        /// <param name="value">The <see cref="FilterParameter"/> to validate</param>
        /// <returns>List of validation error messages; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this FilterParameter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Name
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Name cannot be null or whitespace.");
            }
            else if (value.Name.Length > 256)
            {
                errors.Add("Name cannot exceed 256 characters.");
            }

            // Validate Min and Max
            if (float.IsNaN(value.Min))
            {
                errors.Add("Min cannot be NaN.");
            }
            else if (float.IsInfinity(value.Min))
            {
                errors.Add("Min cannot be infinite.");
            }

            if (float.IsNaN(value.Max))
            {
                errors.Add("Max cannot be NaN.");
            }
            else if (float.IsInfinity(value.Max))
            {
                errors.Add("Max cannot be infinite.");
            }

            if (value.Min > value.Max)
            {
                errors.Add("Min cannot be greater than Max.");
            }

            // Validate Value is within range
            if (!float.IsNaN(value.Value) && !float.IsInfinity(value.Value))
            {
                if (value.Value < value.Min)
                {
                    errors.Add($"Value {value.Value.ToString(CultureInfo.InvariantCulture)} is less than Min {value.Min.ToString(CultureInfo.InvariantCulture)}.");
                }
                else if (value.Value > value.Max)
                {
                    errors.Add($"Value {value.Value.ToString(CultureInfo.InvariantCulture)} is greater than Max {value.Max.ToString(CultureInfo.InvariantCulture)}.");
                }
            }

            // Validate Type
            if (string.IsNullOrWhiteSpace(value.Type))
            {
                errors.Add("Type cannot be null or whitespace.");
            }
            else if (value.Type.Length > 128)
            {
                errors.Add("Type cannot exceed 128 characters.");
            }

            // Validate Unit (optional but must be valid if provided)
            if (value.Unit is not null)
            {
                if (value.Unit.Length > 64)
                {
                    errors.Add("Unit cannot exceed 64 characters.");
                }
                else if (ContainsWhitespace(value.Unit))
                {
                    errors.Add("Unit cannot contain whitespace.");
                }
            }

            // Validate Description (optional but must be valid if provided)
            if (value.Description is not null)
            {
                if (value.Description.Length > 1024)
                {
                    errors.Add("Description cannot exceed 1024 characters.");
                }
            }

            // Validate that required parameters have values
            if (value.IsRequired)
            {
                if (string.IsNullOrWhiteSpace(value.Name))
                {
                    errors.Add("Required parameter must have a non-empty Name.");
                }

                if (float.IsNaN(value.Value) || float.IsInfinity(value.Value))
                {
                    errors.Add("Required parameter must have a valid Value.");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Checks if a <see cref="FilterParameter"/> instance is valid
        /// </summary>
        /// <param name="value">The <see cref="FilterParameter"/> to check</param>
        /// <returns>True if valid; false otherwise</returns>
        public static bool IsValid(this FilterParameter value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures a <see cref="FilterParameter"/> instance is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The <see cref="FilterParameter"/> to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid with detailed error messages</exception>
        public static void EnsureValid(this FilterParameter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"FilterParameter validation failed:{Environment.NewLine}- {string.Join(Environment.NewLine + "- ", errors)}");
            }
        }

        /// <summary>
        /// Checks if a string contains any whitespace characters
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>True if the string contains whitespace; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="str"/> is null</exception>
        private static bool ContainsWhitespace(string str)
        {
            ArgumentNullException.ThrowIfNull(str);
            foreach (var c in str)
            {
                if (char.IsWhiteSpace(c))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
