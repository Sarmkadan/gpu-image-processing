using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="ValidationException"/>.
    /// </summary>
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// Retrieves all validation error messages contained in the exception.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{String}"/> of error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetErrorMessages(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.ValidationErrors?.Values.ToList() ?? [];
        }

        /// <summary>
        /// Returns the number of validation errors recorded in the exception.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns>The count of validation errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static int GetErrorCount(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.ValidationErrors?.Count ?? 0;
        }

        /// <summary>
        /// Retrieves the validation error message for a specific field, if present.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <param name="fieldName">The name of the field to look up.</param>
        /// <returns>The error message for the specified field, or <c>null</c> if none exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is <c>null</c> or empty.</exception>
        public static string? GetErrorForField(this ValidationException exception, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            return exception.ValidationErrors is { } errors && errors.TryGetValue(fieldName, out var value) ? value : null;
        }

        /// <summary>
        /// Determines whether the entity associated with the exception has passed validation.
        /// </summary>
        /// <param name="exception">The <see cref="ValidationException"/> instance.</param>
        /// <returns><c>true</c> if there are no validation errors; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is <c>null</c>.</exception>
        public static bool IsEntityValid(this ValidationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.ValidationErrors is null or { Count: 0 };
        }
    }
}
