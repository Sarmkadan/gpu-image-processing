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

            // Validate all public methods return non-null strings
            ValidateMethod(value.GetFileExtension, nameof(value.GetFileExtension), errors);
            ValidateMethod(value.GetMimeType, nameof(value.GetMimeType), errors);
            ValidateMethod(() => value.FormatResult(new()), nameof(value.FormatResult), errors);
            ValidateMethod(() => value.FormatResults(new()), nameof(value.FormatResults), errors);
            ValidateMethod(() => value.FormatJob(new()), nameof(value.FormatJob), errors);
            ValidateMethod(() => value.FormatDevice(new()), nameof(value.FormatDevice), errors);
            ValidateMethod(() => value.FormatError("test"), nameof(value.FormatError), errors);
            ValidateMethod(() => value.FormatStatistics(new()), nameof(value.FormatStatistics), errors);

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Validates that a method returns a non-null string.
        /// </summary>
        private static void ValidateMethod(Func<string> method, string methodName, List<string> errors)
        {
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
        /// Validates that a method with parameters returns a non-null string.
        /// </summary>
        private static void ValidateMethod<T>(Func<T, string> method, string methodName, List<string> errors)
            where T : class
        {
            try
            {
                string result = method(null);
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
        /// Validates that a method with multiple parameters returns a non-null string.
        /// </summary>
        private static void ValidateMethod<T1, T2>(Func<T1, T2, string> method, string methodName, List<string> errors)
            where T1 : class
            where T2 : class
        {
            try
            {
                string result = method(null, null);
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
        /// Validates that a method with a dictionary parameter returns a non-null string.
        /// </summary>
        private static void ValidateMethod(Func<Dictionary<string, object>, string> method, string methodName, List<string> errors)
        {
            try
            {
                string result = method(null);
                if (result == null)
                {
                    errors.Add($"{methodName} returned null when passed null dictionary");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{methodName} threw an exception with null dictionary: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="CsvResultFormatter"/> instance is valid.
        /// </summary>
        /// <param name="value">The formatter instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(this CsvResultFormatter value)
        {
            return value.Validate().Count == 0;
        }

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