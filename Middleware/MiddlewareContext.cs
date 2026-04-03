#nullable enable
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
    /// Context object passed through middleware pipeline containing request/response data.
    /// Provides access to request metadata, user information, and operational context.
    /// </summary>
    public class MiddlewareContext
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string ApiKey { get; set; }
        public string UserId { get; set; }
        public UserRole? UserRole { get; set; }
        public List<string> Scopes { get; set; } = new();
        public string Operation { get; set; }
        public Dictionary<string, object> RequestData { get; set; } = new();
        public object ResponseData { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public int? ResponseStatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccessful { get; set; } = true;

        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;

        public void SetSuccess(object data, int statusCode = 200)
        {
            IsSuccessful = true;
            ResponseData = data;
            ResponseStatusCode = statusCode;
            EndTime = DateTime.UtcNow;
        }

        public void SetError(string message, int statusCode = 500)
        {
            IsSuccessful = false;
            ErrorMessage = message;
            ResponseStatusCode = statusCode;
            EndTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Result of middleware processing
    /// </summary>
    public class MiddlewareResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static MiddlewareResult Success(object data = null, string message = "OK") =>
            new() { IsSuccessful = true, Data = data, Message = message };

        public static MiddlewareResult Failure(string message, object data = null) =>
            new() { IsSuccessful = false, Message = message, Data = data };
    }

    /// <summary>
    /// Pipeline for executing middleware in sequence
    /// </summary>
    public class MiddlewarePipeline
    {
        private readonly List<IProcessingMiddleware> _middleware;

        public MiddlewarePipeline()
        {
            _middleware = new List<IProcessingMiddleware>();
        }

        /// <summary>
        /// Adds middleware to the pipeline
        /// </summary>
        public void Use(IProcessingMiddleware middleware)
        {
            _middleware.Add(middleware);
            _middleware.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        /// <summary>
        /// Executes the pipeline for a context
        /// </summary>
        public async Task<MiddlewareResult> ExecuteAsync(MiddlewareContext context)
        {
            foreach (var middleware in _middleware)
            {
                var result = await middleware.ProcessAsync(context);

                if (!result.IsSuccessful)
                {
                    context.SetError(result.Message);
                    return result;
                }
            }

            context.SetSuccess(context.ResponseData);
            return MiddlewareResult.Success();
        }
    }

    /// <summary>
    /// Interface for middleware components
    /// </summary>
    public interface IProcessingMiddleware
    {
        /// <summary>
        /// Processes the context. Return Success or Failure result.
        /// </summary>
        Task<MiddlewareResult> ProcessAsync(MiddlewareContext context);

        /// <summary>
        /// Execution order in pipeline (lower numbers execute first)
        /// </summary>
        int Order { get; }
    }

    /// <summary>
    /// Noop middleware for testing
    /// </summary>
    public class NoOpMiddleware : IProcessingMiddleware
    {
        public int Order => 0;

        public async Task<MiddlewareResult> ProcessAsync(MiddlewareContext context)
        {
            return await Task.FromResult(MiddlewareResult.Success());
        }
    }

    /// <summary>
    /// Validation middleware base class
    /// </summary>
    public abstract class ValidationMiddleware : IProcessingMiddleware
    {
        public virtual int Order => 20;

        public async Task<MiddlewareResult> ProcessAsync(MiddlewareContext context)
        {
            var validationResult = await ValidateAsync(context);

            if (!validationResult.IsValid)
                return MiddlewareResult.Failure(validationResult.Message);

            return MiddlewareResult.Success();
        }

        protected abstract Task<ValidationResult> ValidateAsync(MiddlewareContext context);

        protected class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }
    }

    /// <summary>
    /// Request logging middleware
    /// </summary>
    public class RequestLoggingMiddleware : IProcessingMiddleware
    {
        public int Order => 5;

        public async Task<MiddlewareResult> ProcessAsync(MiddlewareContext context)
        {
            Console.WriteLine($"[{context.StartTime:O}] {context.Operation} (RequestId: {context.RequestId})");

            if (!string.IsNullOrEmpty(context.UserId))
                Console.WriteLine($"  User: {context.UserId} (Role: {context.UserRole})");

            return await Task.FromResult(MiddlewareResult.Success());
        }
    }
}
