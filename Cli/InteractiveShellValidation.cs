#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides validation helpers for <see cref="InteractiveShell"/> instances.
    /// </summary>
    public static class InteractiveShellValidation
    {
        /// <summary>
        /// Validates an <see cref="InteractiveShell"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The InteractiveShell instance to validate</param>
        /// <returns>A list of validation problems; empty if valid</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static IReadOnlyList<string> Validate(this InteractiveShell value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // InteractiveShell has no writable properties to validate
            // The class is properly initialized in its constructor and maintains its own state

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether an <see cref="InteractiveShell"/> instance is valid.
        /// </summary>
        /// <param name="value">The InteractiveShell instance to check</param>
        /// <returns>True if valid; otherwise false</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static bool IsValid(this InteractiveShell value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return true;
        }

        /// <summary>
        /// Ensures that an <see cref="InteractiveShell"/> instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The InteractiveShell instance to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        /// <exception cref="ArgumentException">Thrown when value is not valid, containing a list of problems</exception>
        public static void EnsureValid(this InteractiveShell value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"InteractiveShell is not valid. Problems: {string.Join(", ", errors)}");
            }
        }
    }
}
