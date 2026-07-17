#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Provides extension methods for the <see cref="OpenCLException"/> class.
    /// </summary>
    public static class OpenCLExceptionExtensions
    {
        /// <summary>
        /// Enriches the exception message with additional context such as device name and OpenCL error code.
        /// </summary>
        /// <param name="exception">The <see cref="OpenCLException"/> to enrich.</param>
        /// <returns>A new string containing the enriched message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static string EnrichMessage(this OpenCLException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(exception.DeviceName))
            {
                parts.Add($"Device: {exception.DeviceName}");
            }

            if (exception.OpenCLErrorCode.HasValue)
            {
                parts.Add($"OpenCL Error Code: {exception.OpenCLErrorCode}");
            }

            return parts.Count > 0
                ? $"{exception.Message} ({string.Join(", ", parts)})"
                : exception.Message;
        }

        /// <summary>
        /// Gets a dictionary of structured error details from the exception.
        /// </summary>
        /// <param name="exception">The <see cref="OpenCLException"/> to extract details from.</param>
        /// <returns>An immutable <see cref="IReadOnlyDictionary{TKey, TValue}"/> containing error details.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static IReadOnlyDictionary<string, object> GetErrorDetails(this OpenCLException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var dictionary = new Dictionary<string, object>
            {
                { "Message", exception.Message },
                { "DeviceName", exception.DeviceName ?? string.Empty },
                { "OpenCLErrorCode", exception.OpenCLErrorCode ?? 0 }
            };

            dictionary.Add("IsKernelCompilationError", exception is KernelCompilationException);

            return dictionary;
        }

        /// <summary>
        /// Determines whether the exception is a <see cref="KernelCompilationException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="OpenCLException"/> to check.</param>
        /// <returns><see langword="true"/> if the exception is a <see cref="KernelCompilationException"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static bool IsKernelCompilationError(this OpenCLException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception is KernelCompilationException;
        }

        /// <summary>
        /// Gets the compilation log if the exception is a <see cref="KernelCompilationException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="OpenCLException"/> to extract the log from.</param>
        /// <returns>The compilation log, or <see langword="null"/> if not available.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
        public static string? GetCompilationLog(this OpenCLException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception switch
            {
                KernelCompilationException kernelException => kernelException.CompilationLog,
                _ => null
            };
        }
    }
}
