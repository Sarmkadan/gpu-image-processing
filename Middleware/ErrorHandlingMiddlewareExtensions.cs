using System;
using GpuImageProcessing.Middleware;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for the <see cref="ErrorHandlingMiddleware"/> class.
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Determines if the middleware is considered a high-priority middleware based on its execution priority.
        /// </summary>
        /// <param name="middleware">The <see cref="ErrorHandlingMiddleware"/> instance.</param>
        /// <returns>True if the middleware priority is less than or equal to 50; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static bool IsHighPriority(this ErrorHandlingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.GetPriority() <= 50;
        }

        /// <summary>
        /// Gets the display name of the middleware, ensuring consistent formatting.
        /// </summary>
        /// <param name="middleware">The <see cref="ErrorHandlingMiddleware"/> instance.</param>
        /// <returns>A formatted string representing the middleware name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="ErrorHandlingMiddleware.GetName"/> returns null.</exception>
        public static string GetDisplayName(this ErrorHandlingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.GetName()?.ToUpperInvariant() ?? throw new InvalidOperationException("Middleware name cannot be null");
        }
    }
}
