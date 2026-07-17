#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =======================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides validation helpers for the Transform class.
    /// </summary>
    public static class TransformValidation
    {
        /// <summary>
        /// Validates a Transform instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The Transform instance to validate.</param>
        /// <returns>List of validation errors; empty list if valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this Transform value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Id
            if (value.Id == Guid.Empty)
            {
                errors.Add("Transform Id cannot be empty (Guid.Empty)");
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Transform Name cannot be null or whitespace");
            }
            else if (value.Name.Length > 256)
            {
                errors.Add("Transform Name cannot exceed 256 characters");
            }

            // Validate Type
            // TransformType is an enum, so it's always valid by default

            // Validate Description
            if (value.Description?.Length > 1024)
            {
                errors.Add("Transform Description cannot exceed 1024 characters");
            }

            // Validate Parameters
            if (value.Parameters is null)
            {
                errors.Add("Transform Parameters dictionary cannot be null");
            }
            else
            {
                foreach (var kvp in value.Parameters)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        errors.Add("Transform Parameters contains an entry with null or whitespace key");
                        break;
                    }

                    // Validate parameter value range
                    if (float.IsNaN(kvp.Value) || float.IsInfinity(kvp.Value))
                    {
                        errors.Add($"Transform Parameters contains invalid value for key '{kvp.Key}': {kvp.Value}");
                    }
                }
            }

            // Validate IsActive - no specific constraints

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                errors.Add("Transform CreatedAt cannot be default (DateTime.MinValue)");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("Transform CreatedAt cannot be in the future");
            }

            // Validate ExecutionOrder
            if (value.ExecutionOrder < 0)
            {
                errors.Add("Transform ExecutionOrder cannot be negative");
            }

            // Validate ProcessingTimeMs
            if (value.ProcessingTimeMs < 0f)
            {
                errors.Add("Transform ProcessingTimeMs cannot be negative");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Checks if a Transform instance is valid.
        /// </summary>
        /// <param name="value">The Transform instance to validate.</param>
        /// <returns>True if valid; false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this Transform value)
            => value.Validate().Count == 0;

        /// <summary>
        /// Ensures that a Transform instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The Transform instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing the validation errors.</exception>
        public static void EnsureValid(this Transform value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"Transform is not valid. Validation errors:\n{string.Join("\n", errors)}",
                    nameof(value));
            }
        }
    }
}
