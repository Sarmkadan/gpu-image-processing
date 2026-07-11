#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides validation helpers for <see cref="CommandDispatcher"/> instances.
    /// Validates command registration, dispatch parameters, and command discovery.
    /// </summary>
    public static class CommandDispatcherValidation
    {
        /// <summary>
        /// Validates that a <see cref="CommandDispatcher"/> instance is in a valid state.
        /// </summary>
        /// <param name="value">The dispatcher instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this CommandDispatcher value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate internal state consistency
            if (value.GetAvailableCommands() is null)
            {
                problems.Add("GetAvailableCommands() returned null");
            }

            // Validate that we can get descriptions for registered commands
            foreach (var commandName in value.GetAvailableCommands() ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    problems.Add("GetAvailableCommands() returned null or whitespace command name");
                    continue;
                }

                var description = value.GetCommandDescription(commandName);
                if (description is null)
                {
                    problems.Add($"GetCommandDescription(\"{commandName}\") returned null");
                }
                else if (string.IsNullOrWhiteSpace(description))
                {
                    problems.Add($"GetCommandDescription(\"{commandName}\") returned empty string");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="CommandDispatcher"/> instance is in a valid state.
        /// </summary>
        /// <param name="value">The dispatcher instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this CommandDispatcher value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="CommandDispatcher"/> instance is in a valid state.
        /// </summary>
        /// <param name="value">The dispatcher instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the dispatcher is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this CommandDispatcher value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"CommandDispatcher is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}