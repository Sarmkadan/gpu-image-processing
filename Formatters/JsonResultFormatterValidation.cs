#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="JsonResultFormatter"/> instances.
    /// </summary>
    public static class JsonResultFormatterValidation
    {
        /// <summary>
        /// Validates a <see cref="JsonResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this JsonResultFormatter value)
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
        /// Determines whether a <see cref="JsonResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter to check.</param>
        /// <returns>True if the formatter is valid; otherwise false.</returns>
        public static bool IsValid(this JsonResultFormatter value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="JsonResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this JsonResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"JsonResultFormatter is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}