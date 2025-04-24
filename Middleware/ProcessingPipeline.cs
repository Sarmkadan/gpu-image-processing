// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Pipeline orchestrator that chains middleware components for image processing.
    /// Manages middleware registration, ordering, and execution flow.
    /// </summary>
    public class ProcessingPipeline
    {
        private readonly ILogger<ProcessingPipeline> _logger;
        private readonly List<IProcessingMiddleware> _middlewares;
        private Func<Task<MiddlewareResult>> _finalHandler;

        public ProcessingPipeline(ILogger<ProcessingPipeline> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _middlewares = new List<IProcessingMiddleware>();
        }

        /// <summary>
        /// Registers a middleware component into the pipeline.
        /// Middlewares are executed in priority order (lower priority executes first).
        /// </summary>
        public void RegisterMiddleware(IProcessingMiddleware middleware)
        {
            if (middleware == null)
                throw new ArgumentNullException(nameof(middleware));

            _middlewares.Add(middleware);
            _middlewares.Sort((a, b) => a.GetPriority().CompareTo(b.GetPriority()));

            _logger.LogInformation(
                "Middleware registered - Name: {MiddlewareName}, Priority: {Priority}",
                middleware.GetName(),
                middleware.GetPriority());
        }

        /// <summary>
        /// Sets the final handler executed after all middleware.
        /// This is the actual operation handler.
        /// </summary>
        public void SetFinalHandler(Func<Task<MiddlewareResult>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _finalHandler = handler;
        }

        /// <summary>
        /// Executes the processing pipeline with given context.
        /// Routes through all registered middleware in priority order.
        /// </summary>
        public async Task<MiddlewareResult> ExecuteAsync(MiddlewareContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (_finalHandler == null)
                throw new InvalidOperationException("Final handler not configured");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogDebug(
                    "Pipeline execution started - OperationId: {OperationId}, Type: {OperationType}",
                    context.OperationId,
                    context.OperationType);

                // Build pipeline chain
                Func<Task<MiddlewareResult>> chain = BuildPipeline();
                context.Next = chain;

                // Execute pipeline
                var result = await chain();

                stopwatch.Stop();
                result.DurationMs = stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "Pipeline execution completed - OperationId: {OperationId}, Success: {IsSuccess}, Duration: {DurationMs}ms",
                    context.OperationId,
                    result.IsSuccess,
                    result.DurationMs);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "Pipeline execution failed - OperationId: {OperationId}, Error: {ErrorMessage}",
                    context.OperationId,
                    ex.Message);

                throw;
            }
        }

        /// <summary>
        /// Builds the middleware chain by composing each middleware with the next.
        /// Returns a function representing the complete pipeline.
        /// </summary>
        private Func<Task<MiddlewareResult>> BuildPipeline()
        {
            // Start with final handler
            Func<Task<MiddlewareResult>> chain = _finalHandler;

            // Wrap with each middleware in reverse order (so first middleware executes first)
            for (int i = _middlewares.Count - 1; i >= 0; i--)
            {
                var middleware = _middlewares[i];
                var nextInChain = chain;

                chain = async () => await middleware.ExecuteAsync(CreateContextWithNext(nextInChain));
            }

            return chain;
        }

        /// <summary>
        /// Creates a middleware context with the next handler in the pipeline.
        /// </summary>
        private MiddlewareContext CreateContextWithNext(Func<Task<MiddlewareResult>> nextHandler)
        {
            var context = new MiddlewareContext();
            context.Next = nextHandler;
            return context;
        }

        /// <summary>
        /// Gets the list of registered middleware in execution order.
        /// </summary>
        public IReadOnlyList<string> GetMiddlewareOrder()
        {
            return _middlewares.Select(m => m.GetName()).ToList().AsReadOnly();
        }

        /// <summary>
        /// Removes a middleware by name.
        /// </summary>
        public bool RemoveMiddleware(string middlewareName)
        {
            var middleware = _middlewares.FirstOrDefault(m => m.GetName() == middlewareName);
            if (middleware != null)
            {
                _middlewares.Remove(middleware);
                _logger.LogInformation("Middleware removed - Name: {MiddlewareName}", middlewareName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clears all registered middleware from the pipeline.
        /// </summary>
        public void ClearMiddleware()
        {
            _middlewares.Clear();
            _logger.LogInformation("All middleware cleared from pipeline");
        }
    }
}
