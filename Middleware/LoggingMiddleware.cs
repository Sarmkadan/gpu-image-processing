// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Middleware for comprehensive logging of image processing operations.
    /// Tracks execution time, input parameters, and operation results.
    /// </summary>
    public class LoggingMiddleware : IProcessingMiddleware
    {
        private readonly ILogger<LoggingMiddleware> _logger;
        private readonly bool _enableDetailedLogging;

        public LoggingMiddleware(ILogger<LoggingMiddleware> logger, bool enableDetailedLogging = false)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _enableDetailedLogging = enableDetailedLogging;
        }

        /// <summary>
        /// Gets middleware name for pipeline identification.
        /// </summary>
        public string GetName()
        {
            return "LoggingMiddleware";
        }

        /// <summary>
        /// Gets execution order priority (lower executes first).
        /// Logging should execute early to capture all operations.
        /// </summary>
        public int GetPriority()
        {
            return 100;
        }

        /// <summary>
        /// Executes middleware processing with comprehensive logging.
        /// Logs operation start, parameters, execution time, and results.
        /// </summary>
        public async Task<MiddlewareResult> ExecuteAsync(MiddlewareContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var stopwatch = Stopwatch.StartNew();
            var operationId = Guid.NewGuid().ToString("N");

            try
            {
                LogOperationStart(operationId, context);

                var result = await context.Next();

                stopwatch.Stop();
                LogOperationComplete(operationId, context, result, stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogOperationError(operationId, context, ex, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Logs operation initialization with parameter details.
        /// Captures input configuration and processing context.
        /// </summary>
        private void LogOperationStart(string operationId, MiddlewareContext context)
        {
            _logger.LogInformation(
                "Operation started - ID: {OperationId}, Type: {OperationType}, Timestamp: {Timestamp}",
                operationId,
                context.OperationType,
                DateTime.UtcNow);

            if (_enableDetailedLogging && context.Parameters != null)
            {
                foreach (var param in context.Parameters)
                {
                    _logger.LogDebug(
                        "Parameter - Name: {ParameterName}, Value: {ParameterValue}",
                        param.Key,
                        param.Value);
                }
            }
        }

        /// <summary>
        /// Logs successful operation completion with execution metrics.
        /// Records processing time and result statistics.
        /// </summary>
        private void LogOperationComplete(string operationId, MiddlewareContext context,
            MiddlewareResult result, long elapsedMs)
        {
            _logger.LogInformation(
                "Operation completed - ID: {OperationId}, Status: {Status}, Duration: {DurationMs}ms",
                operationId,
                result.IsSuccess ? "Success" : "Failed",
                elapsedMs);

            if (result.Metrics != null && _enableDetailedLogging)
            {
                foreach (var metric in result.Metrics)
                {
                    _logger.LogDebug(
                        "Metric - Name: {MetricName}, Value: {MetricValue}",
                        metric.Key,
                        metric.Value);
                }
            }
        }

        /// <summary>
        /// Logs operation errors with exception details.
        /// Captures stack trace and context information for debugging.
        /// </summary>
        private void LogOperationError(string operationId, MiddlewareContext context,
            Exception ex, long elapsedMs)
        {
            _logger.LogError(
                ex,
                "Operation failed - ID: {OperationId}, Type: {OperationType}, Duration: {DurationMs}ms, Error: {ErrorMessage}",
                operationId,
                context.OperationType,
                elapsedMs,
                ex.Message);
        }
    }
}
