#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="MarkdownResultFormatter"/> instances.
    /// </summary>
    public static class MarkdownResultFormatterValidation
    {
        /// <summary>
        /// Validates the specified <see cref="MarkdownResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this MarkdownResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate all public methods return non-null strings and don't throw unexpected exceptions
            ValidateMethod(() => value.GetFileExtension(), nameof(value.GetFileExtension), errors);
            ValidateMethod(() => value.GetMimeType(), nameof(value.GetMimeType), errors);
            ValidateMethod(() => value.FormatResult(new ProcessingResult()), nameof(value.FormatResult), errors);
            ValidateMethod(() => value.FormatResults(new List<ProcessingResult>()), nameof(value.FormatResults), errors);
            ValidateMethod(() => value.FormatJob(new ProcessingJob()), nameof(value.FormatJob), errors);
            ValidateMethod(() => value.FormatDevice(new DeviceInfo()), nameof(value.FormatDevice), errors);
            ValidateMethod(() => value.FormatError("Test"), nameof(value.FormatError), errors);
            ValidateMethod(() => value.Format(new List<ProcessingResult>()), nameof(value.Format), errors);

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates that a method returns a non-null string.
        /// </summary>
        private static void ValidateMethod(Func<string> method, string methodName, List<string> errors)
        {
            try
            {
                string? result = method();
                if (result == null)
                {
                    errors.Add($"{methodName} returned null instead of a valid string");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{methodName} threw an exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="MarkdownResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this MarkdownResultFormatter value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="MarkdownResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the formatter instance is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this MarkdownResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"MarkdownResultFormatter is invalid. Validation errors: {string.Join("; ", errors)}",
                    nameof(value));
            }
        }
    }
}
