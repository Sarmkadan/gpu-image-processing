using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="ImageProcessingException"/>.
    /// </summary>
    public static class ImageProcessingExceptionExtensions
    {
        /// <summary>
        /// Gets a list of error codes from a collection of <see cref="ImageProcessingException"/> instances.
        /// </summary>
        /// <param name="exceptions">The collection of exceptions.</param>
        /// <returns>A list of error codes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is null.</exception>
        public static IReadOnlyList<string> GetErrorCodes(this IEnumerable<ImageProcessingException> exceptions)
        {
            ArgumentNullException.ThrowIfNull(exceptions);
            return exceptions.Select(e => e.ErrorCode).ToList();
        }

        /// <summary>
        /// Determines whether an <see cref="ImageProcessingException"/> instance has a specific error code.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <param name="errorCode">The error code to check for.</param>
        /// <returns>True if the exception has the specified error code; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errorCode"/> is null or empty.</exception>
        public static bool HasErrorCode(this ImageProcessingException exception, string errorCode)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentException.ThrowIfNullOrEmpty(errorCode);
            return exception.ErrorCode == errorCode;
        }

        /// <summary>
        /// Gets a string representation of an <see cref="ImageProcessingException"/> instance, including its error code and occurrence time.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>A string representation of the exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static string GetDetailedMessage(this ImageProcessingException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return $"Error Code: {exception.ErrorCode}, Occurred At: {exception.OccurredAt:O}";
        }

        /// <summary>
        /// Determines whether an <see cref="ImageProcessingException"/> instance is related to a file operation.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>True if the exception is related to a file operation; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static bool IsFileRelated(this ImageProcessingException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception is ImageFileException;
        }

        /// <summary>
        /// Determines whether an <see cref="ImageProcessingException"/> instance is related to an invalid image.
        /// </summary>
        /// <param name="exception">The exception instance.</param>
        /// <returns>True if the exception is related to an invalid image; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static bool IsInvalidImage(this ImageProcessingException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception is InvalidImageException;
        }
    }
}