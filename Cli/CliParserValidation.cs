#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides validation helpers for <see cref="CliParser"/> instances.
    /// </summary>
    internal static class CliParserValidation
    {
        /// <summary>
        /// Validates a <see cref="CliParser"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The CliParser instance to validate</param>
        /// <returns>A list of validation problems; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static IReadOnlyList<string> Validate(this CliParser value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // CliParser itself has no writable properties to validate
            // Validation happens at parse time through the parsing logic

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="CliParser"/> instance is valid.
        /// </summary>
        /// <param name="value">The CliParser instance to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static bool IsValid(this CliParser value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return true;
        }

        /// <summary>
        /// Ensures that a <see cref="CliParser"/> instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The CliParser instance to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        /// <exception cref="ArgumentException">Thrown when value is not valid, containing a list of problems</exception>
        public static void EnsureValid(this CliParser value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    "CliParser is not valid." + Environment.NewLine + " - " +
                    string.Join(
                        Environment.NewLine + " - ",
                        errors
                    )
                );
            }
        }
    }
}
