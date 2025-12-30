#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="TextResultFormatter"/> instances.
    /// </summary>
    public static class TextResultFormatterValidation
    {
        /// <summary>
        /// Validates the specified <see cref="TextResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this TextResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate GetFileExtension()
            try
            {
                var extension = value.GetFileExtension();
                if (string.IsNullOrWhiteSpace(extension))
                {
                    errors.Add("GetFileExtension() returned null or whitespace.");
                }
                else if (!extension.StartsWith(".", StringComparison.Ordinal))
                {
                    errors.Add("GetFileExtension() did not return a valid file extension (must start with '.').");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"GetFileExtension() threw an exception: {ex.Message}");
            }

            // Validate GetMimeType()
            try
            {
                var mimeType = value.GetMimeType();
                if (string.IsNullOrWhiteSpace(mimeType))
                {
                    errors.Add("GetMimeType() returned null or whitespace.");
                }
                else if (!mimeType.Contains("/"))
                {
                    errors.Add("GetMimeType() did not return a valid MIME type (must contain '/').");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"GetMimeType() threw an exception: {ex.Message}");
            }

            // Validate FormatResult() - cannot test without a ProcessingResult instance
            // This method is validated at runtime when called with actual data

            // Validate FormatResults() - cannot test without a List<ProcessingResult> instance
            // This method is validated at runtime when called with actual data

            // Validate FormatJob() - cannot test without a ProcessingJob instance
            // This method is validated at runtime when called with actual data

            // Validate FormatDevice() - cannot test without a DeviceInfo instance
            // This method is validated at runtime when called with actual data

            // Validate FormatError() - cannot test without parameters
            // This method is validated at runtime when called with actual data

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="TextResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this TextResultFormatter value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="TextResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the validation errors.</exception>
        public static void EnsureValid(this TextResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"TextResultFormatter is not valid. Validation errors:\n  - {
                    string.Join("\n  - ", errors)
                }");
            }
        }
    }
}
