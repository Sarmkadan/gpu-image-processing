#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides validation helpers for the <see cref="Filter"/> class
    /// </summary>
    public static class FilterValidation
    {
        /// <summary>
        /// Validates a <see cref="Filter"/> instance and returns a list of human-readable problems
        /// </summary>
        /// <param name="value">The <see cref="Filter"/> instance to validate</param>
        /// <returns>List of validation problems; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this Filter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Id
            if (value.Id == Guid.Empty)
            {
                errors.Add("Filter.Id cannot be empty (Guid.Empty)");
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Filter.Name cannot be null or whitespace");
            }
            else if (value.Name.Length > 255)
            {
                errors.Add("Filter.Name cannot exceed 255 characters");
            }

            // Validate Type
            if (!Enum.IsDefined(typeof(GpuImageProcessing.Core.Constants.FilterType), value.Type))
            {
                errors.Add("Filter.Type must be a valid FilterType enum value");
            }

            // Validate Description
            if (value.Description?.Length > 1000)
            {
                errors.Add("Filter.Description cannot exceed 1000 characters");
            }

            // Validate Parameters
            if (value.Parameters is null)
            {
                errors.Add("Filter.Parameters cannot be null");
            }
            else
            {
                if (value.Parameters.Count > 100)
                {
                    errors.Add("Filter.Parameters cannot contain more than 100 parameters");
                }

                for (int i = 0; i < value.Parameters.Count; i++)
                {
                    var param = value.Parameters[i];
                    if (param is null)
                    {
                        errors.Add($"Filter.Parameters[{i}] cannot be null");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(param.Name))
                    {
                        errors.Add($"Filter.Parameters[{i}].Name cannot be null or whitespace");
                    }
                    else if (param.Name.Length > 100)
                    {
                        errors.Add($"Filter.Parameters[{i}].Name cannot exceed 100 characters");
                    }

                    if (param.Value < param.Min || param.Value > param.Max)
                    {
                        errors.Add($"Filter.Parameters[{i}].Value ({param.Value}) is outside valid range [{param.Min}, {param.Max}]");
                    }

                    if (param.Min >= param.Max)
                    {
                        errors.Add($"Filter.Parameters[{i}].Min ({param.Min}) must be less than Max ({param.Max})");
                    }
                }
            }

            // Validate IsActive
            // No validation needed for boolean

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                errors.Add("Filter.CreatedAt cannot be default(DateTime)");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("Filter.CreatedAt cannot be in the future");
            }

            // Validate KernelCode
            if (value.KernelCode?.Length > 10000)
            {
                errors.Add("Filter.KernelCode cannot exceed 10000 characters");
            }

            // Validate ProcessingOrder
            if (value.ProcessingOrder is < 0 or > 1000)
            {
                errors.Add("Filter.ProcessingOrder must be between 0 and 1000");
            }

            // Validate AppliedSettings
            if (value.AppliedSettings is null)
            {
                errors.Add("Filter.AppliedSettings cannot be null");
            }
            else
            {
                if (value.AppliedSettings.Count > 50)
                {
                    errors.Add("Filter.AppliedSettings cannot contain more than 50 entries");
                }

                foreach (var kvp in value.AppliedSettings)
                {
                    if (kvp.Key is null)
                    {
                        errors.Add("Filter.AppliedSettings contains a null key");
                        continue;
                    }

                    if (kvp.Key.Length > 100)
                    {
                        errors.Add($"Filter.AppliedSettings key '{kvp.Key}' cannot exceed 100 characters");
                    }

                    if (kvp.Value is null)
                    {
                        errors.Add($"Filter.AppliedSettings['{kvp.Key}'] cannot be null");
                    }
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Filter"/> instance is valid
        /// </summary>
        /// <param name="value">The <see cref="Filter"/> instance to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        public static bool IsValid(this Filter value) => value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="Filter"/> instance is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The <see cref="Filter"/> instance to validate</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing all validation errors</exception>
        public static void EnsureValid(this Filter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                var errorMessage = string.Join("\n- ", errors);
                throw new ArgumentException($"Filter validation failed:\n- {errorMessage}");
            }
        }
    }
}
