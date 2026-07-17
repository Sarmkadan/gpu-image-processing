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
    /// Provides validation helpers for <see cref="CommandHandler"/> instances.
    /// </summary>
    public static class CommandHandlerValidation
    {
        /// <summary>
        /// Validates the command handler instance.
        /// </summary>
        /// <param name="value">The command handler to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="CommandHandler.GetDescription"/> or <see cref="CommandHandler.GetUsage"/> returns null.</exception>
        public static IReadOnlyList<string> Validate(this CommandHandler value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate command description
            string? description = value.GetDescription();
            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add("Command description cannot be null or whitespace.");
            }

            // Validate command usage
            string? usage = value.GetUsage();
            if (string.IsNullOrWhiteSpace(usage))
            {
                errors.Add("Command usage cannot be null or whitespace.");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Checks if the command handler is valid.
        /// </summary>
        /// <param name="value">The command handler to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this CommandHandler value) => value?.Validate().Count == 0;

        /// <summary>
        /// Ensures the command handler is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The command handler to validate.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this CommandHandler value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"CommandHandler is invalid:{Environment.NewLine} - {
                        string.Join(
                            $"{Environment.NewLine} - ",
                            errors
                        )
                    }");
            }
        }
    }
}
