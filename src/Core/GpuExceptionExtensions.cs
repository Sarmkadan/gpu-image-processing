using System;
using System.Text;

namespace GpuImageProcessing.Core
{
    /// <summary>
    /// Extension methods for <see cref="GpuException"/> providing additional utility functionality.
    /// </summary>
    public static class GpuExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception represents a timeout error.
        /// </summary>
        /// <param name="exception">The GPU exception to check.</param>
        /// <returns>True if the error code indicates a timeout; otherwise, false.</returns>
        public static bool IsTimeoutError(this GpuException exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return exception.ErrorCode == -1 || exception.ErrorCode == 0x80000001;
        }

        /// <summary>
        /// Determines whether the exception represents a memory-related error.
        /// </summary>
        /// <param name="exception">The GPU exception to check.</param>
        /// <returns>True if the error code indicates a memory issue; otherwise, false.</returns>
        public static bool IsMemoryError(this GpuException exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return exception.ErrorCode >= 0x00000002 && exception.ErrorCode <= 0x0000001F;
        }

        /// <summary>
        /// Determines whether the exception represents a compute pipeline error.
        /// </summary>
        /// <param name="exception">The GPU exception to check.</param>
        /// <returns>True if the error code indicates a compute pipeline issue; otherwise, false.</returns>
        public static bool IsComputePipelineError(this GpuException exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            return exception.ErrorCode >= 0x00000020 && exception.ErrorCode <= 0x0000003F;
        }

        /// <summary>
        /// Formats the exception details as a multi-line string for logging purposes.
        /// </summary>
        /// <param name="exception">The GPU exception to format.</param>
        /// <returns>A formatted string containing all exception details.</returns>
        public static string FormatForLogging(this GpuException exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var builder = new StringBuilder();
            builder.AppendLine("GPU Exception Details:");
            builder.AppendLine($"  Device: {exception.DeviceName ?? "Unknown"}");
            builder.AppendLine($"  Error Code: 0x{exception.ErrorCode?.ToString("X8") ?? "N/A"}");
            builder.AppendLine($"  Occurred At: {exception.OccurredAt:yyyy-MM-dd HH:mm:ss.fff}");
            builder.AppendLine($"  Message: {exception}");

            return builder.ToString();
        }
    }
}