#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Provides validation helpers for <see cref="CsvResultFormatter"/> instances.
    /// </summary>
    public static class CsvResultFormatterValidation
    {
        /// <summary>
        /// Validates the specified <see cref="CsvResultFormatter"/> instance.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <returns>A list of validation errors; empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this CsvResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate all public methods return non-null strings and handle null inputs correctly
            ValidateMethod(() => value.GetFileExtension(), nameof(value.GetFileExtension), errors);
            ValidateMethod(() => value.GetMimeType(), nameof(value.GetMimeType), errors);
            ValidateMethod(() => value.FormatResult(null), nameof(value.FormatResult), errors);
            ValidateMethod(() => value.FormatResults(null), nameof(value.FormatResults), errors);
            ValidateMethod(() => value.FormatJob(null), nameof(value.FormatJob), errors);
            ValidateMethod(() => value.FormatDevice(null), nameof(value.FormatDevice), errors);
            ValidateMethod(() => value.FormatError(null), nameof(value.FormatError), errors);
            ValidateMethod(() => value.FormatStatistics(null), nameof(value.FormatStatistics), errors);
            ValidateMethod(() => value.FormatStatistics(new Dictionary<string, object>()), nameof(value.FormatStatistics), errors);

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates that a parameterless method returns a non-null string.
        /// </summary>
        /// <param name="method">The method to validate.</param>
        /// <param name="methodName">Name of the method for error reporting.</param>
        /// <param name="errors">List to accumulate validation errors.</param>
        private static void ValidateMethod(Func<string> method, string methodName, List<string> errors)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentException.ThrowIfNullOrEmpty(methodName);

            try
            {
                string result = method();
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
        /// Validates that a method with a single parameter returns a non-null string.
        /// </summary>
        /// <typeparam name="T">Type of the parameter.</typeparam>
        /// <param name="method">The method to validate.</param>
        /// <param name="methodName">Name of the method for error reporting.</param>
        /// <param name="errors">List to accumulate validation errors.</param>
        private static void ValidateMethod<T>(Func<T, string> method, string methodName, List<string> errors)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentException.ThrowIfNullOrEmpty(methodName);

            try
            {
                string result = method(default);
                if (result == null)
                {
                    errors.Add($"{methodName} returned null when passed null parameter");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{methodName} threw an exception with null parameter: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates that a method with two parameters returns a non-null string.
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter.</typeparam>
        /// <typeparam name="T2">Type of the second parameter.</typeparam>
        /// <param name="method">The method to validate.</param>
        /// <param name="methodName">Name of the method for error reporting.</param>
        /// <param name="errors">List to accumulate validation errors.</param>
        private static void ValidateMethod<T1, T2>(Func<T1, T2, string> method, string methodName, List<string> errors)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentException.ThrowIfNullOrEmpty(methodName);

            try
            {
                string result = method(default, default);
                if (result == null)
                {
                    errors.Add($"{methodName} returned null when passed null parameters");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{methodName} threw an exception with null parameters: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="CsvResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this CsvResultFormatter value) => value.Validate().Count == 0;

        /// <summary>
        /// Ensures that the specified <see cref="CsvResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the formatter instance is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this CsvResultFormatter value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"CsvResultFormatter is invalid. Validation errors: {string.Join("; ", errors)}",
                    nameof(value));
            }
        }
    }
}