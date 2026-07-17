#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Models;
using ImageFormat = GpuImageProcessing.Core.Constants.ImageFormat;

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

            // Validate Name
            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Name must be a non-empty string.");
            }
            else if (value.Name.Length > 256)
            {
                errors.Add("Name must be 256 characters or less.");
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

            // Validate Format
            if (value.Format < ImageFormat.Jpeg || value.Format > ImageFormat.Unknown)
            {
                errors.Add("Format must be a valid ImageFormat value.");
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

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                errors.Add("CreatedAt must be a valid DateTime.");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddHours(1))
            {
                errors.Add("CreatedAt cannot be in the future.");
            }

            // Validate ModifiedAt
            if (value.ModifiedAt == default)
            {
                errors.Add("ModifiedAt must be a valid DateTime.");
            }
            else if (value.ModifiedAt > DateTime.UtcNow.AddHours(1))
            {
                errors.Add("ModifiedAt cannot be in the future.");
            }
            else if (value.ModifiedAt < value.CreatedAt)
            {
                errors.Add("ModifiedAt cannot be earlier than CreatedAt.");
            }

            // Validate Description
            if (value.Description != null && value.Description.Length > 2048)
            {
                errors.Add("Description must be 2048 characters or less.");
            }

            // Validate Metadata
            if (value.Metadata.Count > 100)
            {
                errors.Add("Metadata cannot contain more than 100 entries.");
            }
            else
            {
                foreach (var kvp in value.Metadata)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        errors.Add("Metadata keys must be non-empty strings.");
                        break;
                    }

                    if (kvp.Key.Length > 128)
                    {
                        errors.Add("Metadata keys must be 128 characters or less.");
                        break;
                    }

                    if (kvp.Value != null && kvp.Value.Length > 1024)
                    {
                        errors.Add("Metadata values must be 1024 characters or less.");
                        break;
                    }
                }
            }

            // Validate ParentImageId
            if (value.ParentImageId.HasValue && value.ParentImageId.Value == Guid.Empty)
            {
                errors.Add("ParentImageId must be a non-empty GUID if specified.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Image"/> instance is valid.
        /// </summary>
        /// <param name="value">The image instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this Image value) => Validate(value).Count == 0;

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
                $"Image validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}",
                nameof(value));
        }
    }
}