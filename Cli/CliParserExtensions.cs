#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Extension methods for <see cref="CliParser"/> that provide additional parsing and validation functionality.
    /// This class cannot be inherited.
    /// </summary>
    public static class CliParserExtensions
    {
        /// <summary>
        /// Parses command-line arguments and returns a parsed command context.
        /// If parsing fails due to unknown command, returns an empty parsed command instead of throwing.
        /// </summary>
        /// <param name="parser">The parser instance</param>
        /// <param name="args">Command-line arguments</param>
        /// <returns>A parsed command context, or empty if parsing fails</returns>
        /// <exception cref="ArgumentNullException">Thrown if parser or args is null</exception>
        public static ParsedCommand ParseSafely(this CliParser parser, string[] args)
        {
            ArgumentNullException.ThrowIfNull(parser);
            ArgumentNullException.ThrowIfNull(args);

            try
            {
                return parser.Parse(args);
            }
            catch (CliParsingException)
            {
                return ParsedCommand.Empty();
            }
        }

        /// <summary>
        /// Gets the value of an option as an integer, with optional default value.
        /// </summary>
        /// <param name="parsedCommand">The parsed command context</param>
        /// <param name="optionName">The option name (long form)</param>
        /// <param name="defaultValue">Default value if option is not present or invalid</param>
        /// <returns>The parsed integer value or default</returns>
        /// <exception cref="ArgumentNullException">Thrown if parsedCommand or optionName is null</exception>
        public static int GetIntegerOption(this ParsedCommand parsedCommand, string optionName, int defaultValue = 0)
        {
            ArgumentNullException.ThrowIfNull(parsedCommand);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            if (parsedCommand.Options.TryGetValue(optionName, out var valueString))
            {
                if (int.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the value of an option as a boolean, with optional default value.
        /// Treats "true", "1", "yes", "on" as true; everything else as false.
        /// </summary>
        /// <param name="parsedCommand">The parsed command context</param>
        /// <param name="optionName">The option name (long form)</param>
        /// <param name="defaultValue">Default value if option is not present</param>
        /// <returns>The parsed boolean value or default</returns>
        /// <exception cref="ArgumentNullException">Thrown if parsedCommand or optionName is null</exception>
        public static bool GetBooleanOption(this ParsedCommand parsedCommand, string optionName, bool defaultValue = false)
        {
            ArgumentNullException.ThrowIfNull(parsedCommand);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            if (parsedCommand.Options.TryGetValue(optionName, out var valueString))
            {
                return valueString.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                       valueString.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                       valueString.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                       valueString.Equals("on", StringComparison.OrdinalIgnoreCase);
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the value of an option as a double, with optional default value.
        /// </summary>
        /// <param name="parsedCommand">The parsed command context</param>
        /// <param name="optionName">The option name (long form)</param>
        /// <param name="defaultValue">Default value if option is not present or invalid</param>
        /// <returns>The parsed double value or default</returns>
        /// <exception cref="ArgumentNullException">Thrown if parsedCommand or optionName is null</exception>
        public static double GetDoubleOption(this ParsedCommand parsedCommand, string optionName, double defaultValue = 0.0)
        {
            ArgumentNullException.ThrowIfNull(parsedCommand);
            ArgumentException.ThrowIfNullOrEmpty(optionName);

            if (parsedCommand.Options.TryGetValue(optionName, out var valueString))
            {
                if (double.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets positional arguments as a read-only list, with optional count validation.
        /// </summary>
        /// <param name="parsedCommand">The parsed command context</param>
        /// <param name="expectedCount">Expected number of positional arguments, or null for any count</param>
        /// <returns>Read-only list of positional arguments</returns>
        /// <exception cref="ArgumentNullException">Thrown if parsedCommand is null</exception>
        /// <exception cref="ArgumentException">Thrown if expectedCount is negative or count validation fails</exception>
        public static IReadOnlyList<string> GetPositionalArguments(this ParsedCommand parsedCommand, int? expectedCount = null)
        {
            ArgumentNullException.ThrowIfNull(parsedCommand);

            if (expectedCount.GetValueOrDefault(-1) < 0)
            {
                throw new ArgumentException(
                    "Expected count must be non-negative or null",
                    nameof(expectedCount));
            }

            if (expectedCount.HasValue && parsedCommand.PositionalArguments.Count != expectedCount.Value)
            {
                throw new ArgumentException(
                    $"Expected {expectedCount.Value} positional argument(s), but got {parsedCommand.PositionalArguments.Count}",
                    nameof(expectedCount));
            }

            return parsedCommand.PositionalArguments.AsReadOnly();
        }
    }
}
