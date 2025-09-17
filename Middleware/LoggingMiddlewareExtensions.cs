using System;
using System.Threading.Tasks;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for the <see cref="LoggingMiddleware"/> class.
    /// </summary>
    public static class LoggingMiddlewareExtensions
    {
        /// <summary>
        /// Retrieves the metadata of the middleware including its name and priority.
        /// </summary>
        /// <param name="middleware">The middleware instance.</param>
        /// <returns>A tuple containing the middleware name and its priority.</returns>
        /// <exception cref="ArgumentNullException">Thrown when middleware is null.</exception>
        public static (string Name, int Priority) GetMiddlewareMetadata(this LoggingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return (middleware.GetName(), middleware.GetPriority());
        }

        /// <summary>
        /// Determines if the middleware has a higher priority than the specified value.
        /// Note: Lower numerical priority value indicates higher priority.
        /// </summary>
        /// <param name="middleware">The middleware instance.</param>
        /// <param name="otherPriority">The priority to compare against.</param>
        /// <returns>True if the middleware has higher priority, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when middleware is null.</exception>
        public static bool IsHigherPriorityThan(this LoggingMiddleware middleware, int otherPriority)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.GetPriority() < otherPriority;
        }

        /// <summary>
        /// Executes the middleware and returns whether the operation was successful.
        /// </summary>
        /// <param name="middleware">The middleware instance.</param>
        /// <param name="context">The middleware execution context.</param>
        /// <returns>A task that represents the asynchronous operation, returning true if the operation succeeded, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when middleware or context is null.</exception>
        public static async Task<bool> ExecuteAndCheckSuccessAsync(this LoggingMiddleware middleware, MiddlewareContext context)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            ArgumentNullException.ThrowIfNull(context);
            var result = await middleware.ExecuteAsync(context);
            return result.IsSuccess;
        }
    }
}
