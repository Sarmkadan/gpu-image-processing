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
    /// Provides validation helpers for <see cref="ProcessingProfile"/> instances
    /// </summary>
    public static class ProcessingProfileValidation
    {
        /// <summary>
        /// Validates a <see cref="ProcessingProfile"/> instance and returns a list of human-readable validation errors
        /// </summary>
        /// <param name="value">The profile to validate</param>
        /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static IReadOnlyList<string> Validate(this ProcessingProfile? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate required string properties
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Name cannot be null or whitespace");
            }
            else if (value.Name.Length > 256)
            {
                errors.Add("Name cannot exceed 256 characters");
            }

            if (string.IsNullOrWhiteSpace(value.Description))
            {
                errors.Add("Description cannot be null or whitespace");
            }
            else if (value.Description.Length > 4000)
            {
                errors.Add("Description cannot exceed 4000 characters");
            }

            if (string.IsNullOrWhiteSpace(value.PrecisionFormat))
            {
                errors.Add("PrecisionFormat cannot be null or whitespace");
            }
            else if (value.PrecisionFormat.Length > 20)
            {
                errors.Add("PrecisionFormat cannot exceed 20 characters");
            }
            else if (value.PrecisionFormat is not ("float32" or "float16"))
            {
                errors.Add("PrecisionFormat must be either 'float32' or 'float16'");
            }

            // Validate numeric properties
            if (value.MaxParallelOperations <= 0)
            {
                errors.Add("MaxParallelOperations must be greater than 0");
            }
            else if (value.MaxParallelOperations > 128)
            {
                errors.Add("MaxParallelOperations cannot exceed 128");
            }

            if (value.BatchSize <= 0)
            {
                errors.Add("BatchSize must be greater than 0");
            }
            else if (value.BatchSize > 1000)
            {
                errors.Add("BatchSize cannot exceed 1000");
            }

            if (value.MaxMemoryUsageBytes <= 0)
            {
                errors.Add("MaxMemoryUsageBytes must be greater than 0");
            }
            else if (value.MaxMemoryUsageBytes > 16L * 1024 * 1024 * 1024) // 16GB
            {
                errors.Add("MaxMemoryUsageBytes cannot exceed 16GB (17179869184 bytes)");
            }

            if (value.TileSizePixels <= 0)
            {
                errors.Add("TileSizePixels must be greater than 0");
            }
            else if (value.TileSizePixels > 4096)
            {
                errors.Add("TileSizePixels cannot exceed 4096");
            }
            else if (value.EnableTiling && value.TileSizePixels < 64)
            {
                errors.Add("TileSizePixels must be at least 64 when tiling is enabled");
            }

            // Validate date properties
            if (value.CreatedAt == default)
            {
                errors.Add("CreatedAt must be set to a valid DateTime");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CreatedAt cannot be in the future");
            }

            if (value.ModifiedAt == default)
            {
                errors.Add("ModifiedAt must be set to a valid DateTime");
            }
            else if (value.ModifiedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("ModifiedAt cannot be in the future");
            }
            else if (value.CreatedAt != default && value.ModifiedAt < value.CreatedAt)
            {
                errors.Add("ModifiedAt cannot be earlier than CreatedAt");
            }

            // Validate optimization settings
            ArgumentNullException.ThrowIfNull(value.OptimizationSettings);

            foreach (var kvp in value.OptimizationSettings)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("OptimizationSettings contains an entry with null or empty key");
                    break;
                }

                if (kvp.Value < 0.1f || kvp.Value > 10.0f)
                {
                    errors.Add($"OptimizationSettings['{kvp.Key}'] value {kvp.Value} is out of range (0.1 to 10.0)");
                }
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="ProcessingProfile"/> instance is valid
        /// </summary>
        /// <param name="value">The profile to check</param>
        /// <returns>True if valid, false otherwise</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static bool IsValid(this ProcessingProfile? value) => Validate(value).Count == 0;

        /// <summary>
        /// Ensures that a <see cref="ProcessingProfile"/> instance is valid, throwing an exception if not
        /// </summary>
        /// <param name="value">The profile to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when the profile is invalid, containing validation errors</exception>
        public static void EnsureValid(this ProcessingProfile? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ProcessingProfile validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
            }
        }
    }
}