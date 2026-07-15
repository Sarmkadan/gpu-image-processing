#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// Provides validation helpers for <see cref="Image"/> instances.
    /// </summary>
    public static class ImageProcessingControllerValidation
    {
        /// <summary>
        /// Validates the specified <see cref="Image"/> instance.
        /// </summary>
        /// <param name="value">The image instance to validate.</param>
        /// <returns>A list of validation error messages. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this Image value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate Id
            if (value.Id == Guid.Empty)
            {
                errors.Add("Id must be a non-empty GUID.");
            }

            // Validate Path/FilePath
            if (string.IsNullOrWhiteSpace(value.FilePath))
            {
                errors.Add("Path must be a non-empty string.");
            }
            else if (value.FilePath.Length > 1024)
            {
                errors.Add("Path must be 1024 characters or less.");
            }

            // Validate Width
            if (value.Width <= 0)
            {
                errors.Add("Width must be a positive integer.");
            }
            else if (value.Width > 32768)
            {
                errors.Add("Width must be 32768 pixels or less.");
            }

            // Validate Height
            if (value.Height <= 0)
            {
                errors.Add("Height must be a positive integer.");
            }
            else if (value.Height > 32768)
            {
                errors.Add("Height must be 32768 pixels or less.");
            }

            // Validate Channels
            if (value.Channels <= 0)
            {
                errors.Add("Channels must be a positive integer.");
            }
            else if (value.Channels > 16)
            {
                errors.Add("Channels must be 16 or less.");
            }

            // Validate FileSizeBytes
            if (value.FileSizeBytes <= 0)
            {
                errors.Add("FileSizeBytes must be a positive integer.");
            }
            else if (value.FileSizeBytes > 1073741824) // 1GB
            {
                errors.Add("FileSizeBytes must be 1GB or less.");
            }

            // Validate Description
            if (value.Description != null && value.Description.Length > 2048)
            {
                errors.Add("Description must be 2048 characters or less.");
            }

            // Validate RegisteredAt/CreatedAt
            if (value.RegisteredAt == default)
            {
                errors.Add("RegisteredAt must be a valid DateTime.");
            }
            else if (value.RegisteredAt > DateTime.UtcNow.AddHours(1))
            {
                errors.Add("RegisteredAt cannot be in the future.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Image"/> instance is valid.
        /// </summary>
        /// <param name="value">The image instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this Image value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="Image"/> instance is valid.
        /// </summary>
        /// <param name="value">The image instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.
        /// The exception message contains all validation errors.</exception>
        public static void EnsureValid(this Image value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"Image validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}