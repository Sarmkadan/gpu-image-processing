// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Core.Exceptions;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Middleware for centralized error handling and recovery in the processing pipeline.
    /// Implements retry logic, exception translation, and error reporting.
    /// </summary>
    public class ErrorHandlingMiddleware : IProcessingMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger, int maxRetries = 3, int retryDelayMs = 100)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxRetries = maxRetries;
            _retryDelayMs = retryDelayMs;
        }

        /// <summary>
        /// Gets middleware name for pipeline identification.
        /// </summary>
        public string GetName()
        {
            return "ErrorHandlingMiddleware";
        }

        /// <summary>
        /// Gets execution order priority (lower executes first).
        /// Error handling executes first to wrap all subsequent operations.
        /// </summary>
        public int GetPriority()
        {
            return 50;
        }

        /// <summary>
        /// Executes middleware with error handling, retry logic, and recovery mechanisms.
        /// Catches exceptions, applies retry policy, and translates errors appropriately.
        /// </summary>
        public async Task<MiddlewareResult> ExecuteAsync(MiddlewareContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            int attempt = 0;
            Exception lastException = null;

            while (attempt < _maxRetries)
            {
                try
                {
                    attempt++;
                    _logger.LogDebug(
                        "Executing operation with error handling - Attempt {AttemptNumber}/{MaxRetries}",
                        attempt,
                        _maxRetries);

                    var result = await context.Next();
                    return result;
                }
                catch (OpenCLException ex)
                {
                    // OpenCL-specific errors - may be recoverable
                    lastException = ex;
                    _logger.LogWarning(
                        ex,
                        "OpenCL error occurred - Attempt {AttemptNumber}/{MaxRetries}: {ErrorMessage}",
                        attempt,
                        _maxRetries,
                        ex.Message);

                    if (attempt < _maxRetries && IsRecoverable(ex))
                    {
                        await Task.Delay(_retryDelayMs * attempt);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (ImageProcessingException ex)
                {
                    // Image processing errors - generally not retryable
                    lastException = ex;
                    _logger.LogError(
                        ex,
                        "Image processing error - Operation: {OperationType}, Error: {ErrorMessage}",
                        context.OperationType,
                        ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    // Unexpected errors - log and fail fast
                    lastException = ex;
                    _logger.LogError(
                        ex,
                        "Unexpected error during operation - Type: {OperationType}, Error: {ErrorMessage}",
                        context.OperationType,
                        ex.Message);
                    break;
                }
            }

            return CreateErrorResult(lastException, attempt);
        }

        /// <summary>
        /// Determines if an exception is potentially recoverable by retry.
        /// Some OpenCL errors are transient and may succeed on retry.
        /// </summary>
        private bool IsRecoverable(OpenCLException ex)
        {
            // List of recoverable error patterns
            var recoverablePatterns = new[]
            {
                "timeout",
                "device not available",
                "resource temporarily unavailable",
                "device memory",
            };

            string message = ex.Message?.ToLower() ?? string.Empty;
            foreach (var pattern in recoverablePatterns)
            {
                if (message.Contains(pattern))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates an error result with proper error messaging and context.
        /// </summary>
        private MiddlewareResult CreateErrorResult(Exception ex, int attempts)
        {
            string errorMessage = BuildErrorMessage(ex, attempts);
            var result = MiddlewareResult.Failure(errorMessage);
            result.AddMetric("exception_type", ex?.GetType().Name ?? "Unknown");
            result.AddMetric("attempts_made", attempts);
            result.AddMetric("error_timestamp", DateTime.UtcNow);

            return result;
        }

        /// <summary>
        /// Builds a detailed error message for reporting.
        /// Includes exception type, message, and retry information.
        /// </summary>
        private string BuildErrorMessage(Exception ex, int attempts)
        {
            if (ex == null)
                return "Operation failed with unknown error";

            string baseMessage = $"{ex.GetType().Name}: {ex.Message}";

            if (attempts > 1)
            {
                baseMessage += $" (failed after {attempts} attempts)";
            }

            if (ex.InnerException != null)
            {
                baseMessage += $" -> {ex.InnerException.Message}";
            }

            return baseMessage;
        }
    }
}
