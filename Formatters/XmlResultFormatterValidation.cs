#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="XmlResultFormatter"/> instances.
    /// </summary>
    /// <remarks>
    /// This static class contains extension methods for validating <see cref="XmlResultFormatter"/> instances
    /// to ensure they return correct values from their methods and meet expected formatting standards.
    /// </remarks>
    public static class XmlResultFormatterValidation
    {
        /// <summary>
        /// Validates an <see cref="XmlResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this XmlResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate GetFileExtension() behavior
            var fileExtension = value.GetFileExtension();
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                problems.Add("GetFileExtension() returned null or whitespace");
            }
            else if (!fileExtension.StartsWith(".", StringComparison.Ordinal))
            {
                problems.Add("GetFileExtension() returned an extension without leading dot");
            }

            // Validate GetMimeType() behavior
            var mimeType = value.GetMimeType();
            if (string.IsNullOrWhiteSpace(mimeType))
            {
                problems.Add("GetMimeType() returned null or whitespace");
            }
            else if (!mimeType.Contains("/", StringComparison.Ordinal))
            {
                problems.Add("GetMimeType() returned an invalid MIME type format");
            }

            return problems;
        }

        /// <summary>
        /// Determines whether an <see cref="XmlResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter to check.</param>
        /// <returns>True if the formatter is valid; otherwise false.</returns>
        public static bool IsValid(this XmlResultFormatter? value) => value?.Validate().Count == 0;

        /// <summary>
        /// Ensures that an <see cref="XmlResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this XmlResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"XmlResultFormatter is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}