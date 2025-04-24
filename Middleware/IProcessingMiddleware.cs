// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Interface for processing middleware components in the image processing pipeline.
    /// Supports cross-cutting concerns like logging, error handling, and performance monitoring.
    /// </summary>
    public interface IProcessingMiddleware
    {
        /// <summary>
        /// Gets the unique name identifier for this middleware component.
        /// </summary>
        string GetName();

        /// <summary>
        /// Gets execution priority (lower number executes first).
        /// Allows control over middleware execution order.
        /// </summary>
        int GetPriority();

        /// <summary>
        /// Executes middleware logic asynchronously with pipeline continuation.
        /// Must call context.Next() to pass control to next middleware.
        /// </summary>
        Task<MiddlewareResult> ExecuteAsync(MiddlewareContext context);
    }

    /// <summary>
    /// Context passed through middleware pipeline containing operation information.
    /// Provides access to parameters, state, and pipeline continuation.
    /// </summary>
    public class MiddlewareContext
    {
        /// <summary>
        /// Gets the type of operation being processed.
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// Gets operation parameters as key-value pairs.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets custom state storage for inter-middleware communication.
        /// </summary>
        public Dictionary<string, object> State { get; set; }

        /// <summary>
        /// Gets the unique operation identifier for tracking.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets timestamp when operation was initiated.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the delegate for next middleware/handler in pipeline.
        /// </summary>
        public Func<Task<MiddlewareResult>> Next { get; set; }

        public MiddlewareContext()
        {
            Parameters = new Dictionary<string, object>();
            State = new Dictionary<string, object>();
            OperationId = Guid.NewGuid().ToString("N");
            StartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets a parameter value in the context.
        /// </summary>
        public void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }

        /// <summary>
        /// Gets a parameter value with optional default.
        /// </summary>
        public T GetParameter<T>(string key, T defaultValue = default)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                return value is T typed ? typed : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets a state value for inter-middleware communication.
        /// </summary>
        public void SetState(string key, object value)
        {
            State[key] = value;
        }

        /// <summary>
        /// Gets a state value with optional default.
        /// </summary>
        public T GetState<T>(string key, T defaultValue = default)
        {
            if (State.TryGetValue(key, out var value))
            {
                return value is T typed ? typed : defaultValue;
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Result returned from middleware pipeline execution.
    /// Contains success status, data, and performance metrics.
    /// </summary>
    public class MiddlewareResult
    {
        /// <summary>
        /// Gets whether operation executed successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets the operation result data.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets error message if operation failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets performance metrics as key-value pairs.
        /// </summary>
        public Dictionary<string, object> Metrics { get; set; }

        /// <summary>
        /// Gets execution duration in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        public MiddlewareResult()
        {
            IsSuccess = true;
            Metrics = new Dictionary<string, object>();
            DurationMs = 0;
        }

        /// <summary>
        /// Creates a successful result with data.
        /// </summary>
        public static MiddlewareResult Success(object data = null)
        {
            return new MiddlewareResult
            {
                IsSuccess = true,
                Data = data
            };
        }

        /// <summary>
        /// Creates a failed result with error message.
        /// </summary>
        public static MiddlewareResult Failure(string errorMessage)
        {
            return new MiddlewareResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Adds a metric to the result.
        /// </summary>
        public void AddMetric(string name, object value)
        {
            Metrics[name] = value;
        }
    }
}
