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
    public class RequestMiddlewareContext
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
    public class RequestMiddlewareResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static RequestMiddlewareResult Success(object data = null, string message = "OK") =>
            new() { IsSuccessful = true, Data = data, Message = message };

        public static RequestMiddlewareResult Failure(string message, object data = null) =>
            new() { IsSuccessful = false, Message = message, Data = data };
    }

    /// <summary>
    /// Pipeline for executing middleware in sequence
    /// </summary>
    public class MiddlewarePipeline
    {
        private readonly List<IRequestMiddleware> _middleware;

        public MiddlewarePipeline()
        {
            _middleware = new List<IRequestMiddleware>();
        }

        /// <summary>
        /// Adds middleware to the pipeline
        /// </summary>
        public void Use(IRequestMiddleware middleware)
        {
            _middleware.Add(middleware);
            _middleware.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        /// <summary>
        /// Executes the pipeline for a context
        /// </summary>
        public async Task<RequestMiddlewareResult> ExecuteAsync(RequestMiddlewareContext context)
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
            return RequestMiddlewareResult.Success();
        }
    }

    /// <summary>
    /// Interface for middleware components
    /// </summary>
    public interface IRequestMiddleware
    {
        /// <summary>
        /// Processes the context. Return Success or Failure result.
        /// </summary>
        Task<RequestMiddlewareResult> ProcessAsync(RequestMiddlewareContext context);

        /// <summary>
        /// Execution order in pipeline (lower numbers execute first)
        /// </summary>
        int Order { get; }
    }

    /// <summary>
    /// Noop middleware for testing
    /// </summary>
    public class NoOpMiddleware : IRequestMiddleware
    {
        public int Order => 0;

        public async Task<RequestMiddlewareResult> ProcessAsync(RequestMiddlewareContext context)
        {
            return await Task.FromResult(RequestMiddlewareResult.Success());
        }
    }

    /// <summary>
    /// Validation middleware base class
    /// </summary>
    public abstract class ValidationMiddleware : IRequestMiddleware
    {
        public virtual int Order => 20;

        public async Task<RequestMiddlewareResult> ProcessAsync(RequestMiddlewareContext context)
        {
            var validationResult = await ValidateAsync(context);

            if (!validationResult.IsValid)
                return RequestMiddlewareResult.Failure(validationResult.Message);

            return RequestMiddlewareResult.Success();
        }

        protected abstract Task<ValidationResult> ValidateAsync(RequestMiddlewareContext context);

        protected class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; }
        }
    }

    /// <summary>
    /// Request logging middleware
    /// </summary>
    public class RequestLoggingMiddleware : IRequestMiddleware
    {
        public int Order => 5;

        public async Task<RequestMiddlewareResult> ProcessAsync(RequestMiddlewareContext context)
        {
            Console.WriteLine($"[{context.StartTime:O}] {context.Operation} (RequestId: {context.RequestId})");

            if (!string.IsNullOrEmpty(context.UserId))
                Console.WriteLine($"  User: {context.UserId} (Role: {context.UserRole})");

            return await Task.FromResult(RequestMiddlewareResult.Success());
        }
    }
}
