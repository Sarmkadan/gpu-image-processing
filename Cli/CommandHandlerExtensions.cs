#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Extension methods that add convenient helpers for <see cref="CommandHandler"/> instances.
    /// </summary>
    public static class CommandHandlerExtensions
    {
        /// <summary>
        /// Retrieves an argument value and attempts to convert it to <typeparamref name="T"/>.
        /// If the argument is missing or cannot be converted, <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <typeparam name="T">The type to convert the argument to.</typeparam>
        /// <param name="handler">The command handler.</param>
        /// <param name="key">The argument key (case-insensitive).</param>
        /// <param name="defaultValue">The value to return when the argument is absent or conversion fails.</param>
        /// <returns>The converted argument value or <paramref name="defaultValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> or <paramref name="key"/> is <c>null</c>.</exception>
        public static T GetArgumentOrDefault<T>(this CommandHandler handler, string key, T defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var raw = handler.GetArgument(key, null);
            if (raw is null)
                return defaultValue;

            // Fast path for string
            if (typeof(T) == typeof(string))
                return (T)(object)raw!;

            // Fast path for bool
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(raw, out var boolResult))
                    return (T)(object)boolResult;
                return defaultValue;
            }

            // Enum handling
            if (typeof(T).IsEnum)
            {
                if (Enum.TryParse(typeof(T), raw, true, out var enumResult))
                    return (T)enumResult;
                return defaultValue;
            }

            try
            {
                var converted = (T)Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture);
                return converted;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Tries to retrieve an argument value and convert it to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The desired result type.</typeparam>
        /// <param name="handler">The command handler.</param>
        /// <param name="key">The argument key.</param>
        /// <param name="value">When this method returns, contains the converted value if the conversion succeeded; otherwise the default value of <typeparamref name="T"/>.</param>
        /// <returns><c>true</c> if the argument existed and was successfully converted; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> or <paramref name="key"/> is <c>null</c>.</exception>
        public static bool TryGetArgument<T>(this CommandHandler handler, string key, out T value)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentException.ThrowIfNullOrEmpty(key);

            var raw = handler.GetArgument(key, null);
            if (raw is null)
            {
                value = default!;
                return false;
            }

            // Fast path for string
            if (typeof(T) == typeof(string))
            {
                value = (T)(object)raw;
                return true;
            }

            // Fast path for bool
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(raw, out var boolResult))
                {
                    value = (T)(object)boolResult;
                    return true;
                }
                value = default!;
                return false;
            }

            // Enum handling
            if (typeof(T).IsEnum)
            {
                if (Enum.TryParse(typeof(T), raw, true, out var enumResult))
                {
                    value = (T)enumResult;
                    return true;
                }
                value = default!;
                return false;
            }

            try
            {
                value = (T)Convert.ChangeType(raw, typeof(T), CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                value = default!;
                return false;
            }
        }

        /// <summary>
        /// Sets the arguments on the handler using the supplied collection and returns the handler for fluent chaining.
        /// </summary>
        /// <param name="handler">The command handler.</param>
        /// <param name="args">The raw argument strings (including the command name at index 0).</param>
        /// <returns>The same <paramref name="handler"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> or <paramref name="args"/> is <c>null</c>.</exception>
        public static CommandHandler WithArguments(this CommandHandler handler, IEnumerable<string> args)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(args);

            handler.SetArguments(args.ToArray());
            return handler;
        }

        /// <summary>
        /// Writes the command's description and usage information to the console.
        /// </summary>
        /// <param name="handler">The command handler.</param>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <c>null</c>.</exception>
        public static void PrintHelp(this CommandHandler handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            Console.WriteLine(handler.GetDescription());
            Console.WriteLine();
            Console.WriteLine(handler.GetUsage());
        }
    }
}