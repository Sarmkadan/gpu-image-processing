#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Provides validation helpers for <see cref="ImageRegisteredEvent"/>.
    /// </summary>
    public static class ImageRegisteredEventValidation
    {
        /// <summary>
        /// Validates an <see cref="ImageRegisteredEvent"/> and returns a list of human-readable validation problems.
        /// </summary>
        /// <param name="value">The image registered event to validate.</param>
        /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ImageRegisteredEvent? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate ImageId
            if (value.ImageId == Guid.Empty)
            {
                errors.Add("ImageId must be a non-empty Guid.");
            }

            // Validate ImagePath
            if (string.IsNullOrWhiteSpace(value.ImagePath))
            {
                errors.Add("ImagePath must not be null or whitespace.");
            }
            else if (!Path.IsPathRooted(value.ImagePath))
            {
                var firstChar = value.ImagePath[0];
                if (firstChar != '.' && firstChar != '/' && firstChar != '\\')
                {
                    errors.Add("ImagePath must be an absolute path or a valid relative path starting with '.', '/', or '\\'.");
                }
            }

            // Validate Width
            if (value.Width <= 0)
            {
                errors.Add("Width must be a positive integer.");
            }

            // Validate Height
            if (value.Height <= 0)
            {
                errors.Add("Height must be a positive integer.");
            }

            // Validate Description
            if (string.IsNullOrWhiteSpace(value.Description))
            {
                errors.Add("Description must not be null or whitespace.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified image registered event is valid.
        /// </summary>
        /// <param name="value">The image registered event to check.</param>
        /// <returns>True if the event is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ImageRegisteredEvent? value)
            => value is not null && value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the specified image registered event is valid, throwing an exception if it is not.
        /// </summary>
        /// <param name="value">The image registered event to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the event is invalid, with a detailed message listing all validation problems.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this ImageRegisteredEvent? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"ImageRegisteredEvent validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}",
                    nameof(value));
            }
        }
    }
}