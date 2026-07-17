#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides validation helpers for <see cref="InteractiveShell"/> instances.
    /// </summary>
    public static class InteractiveShellValidation
    {
        private static readonly FieldInfo _parserField = typeof(InteractiveShell).GetField("_parser", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("InteractiveShell._parser field not found");
        private static readonly FieldInfo _handlersField = typeof(InteractiveShell).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("InteractiveShell._handlers field not found");
        private static readonly FieldInfo _historyField = typeof(InteractiveShell).GetField("_history", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("InteractiveShell._history field not found");
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

            if (value.GetParser() is null)
            {
                errors.Add("InteractiveShell parser is not initialized");
            }

            if (value.GetHandlers() is null)
            {
                errors.Add("InteractiveShell command handlers dictionary is not initialized");
            }

            if (value.GetHistory() is null)
            {
                errors.Add("InteractiveShell command history is not initialized");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Gets the parser from an <see cref="InteractiveShell"/> instance.
        /// </summary>
        /// <param name="shell">The InteractiveShell instance</param>
        /// <returns>The CliParser instance</returns>
        private static CliParser? GetParser(this InteractiveShell shell)
        {
            return _parserField.GetValue(shell) as CliParser;
        }

        /// <summary>
        /// Gets the handlers dictionary from an <see cref="InteractiveShell"/> instance.
        /// </summary>
        /// <param name="shell">The InteractiveShell instance</param>
        /// <returns>The handlers dictionary</returns>
        private static Dictionary<string, Func<ParsedCommand, Task>>? GetHandlers(this InteractiveShell shell)
        {
            return _handlersField.GetValue(shell) as Dictionary<string, Func<ParsedCommand, Task>>;
        }

        /// <summary>
        /// Gets the history from an <see cref="InteractiveShell"/> instance.
        /// </summary>
        /// <param name="shell">The InteractiveShell instance</param>
        /// <returns>The command history</returns>
        private static object? GetHistory(this InteractiveShell shell)
        {
            return _historyField.GetValue(shell);
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
            return value.Validate().Count == 0;
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