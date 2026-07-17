using System;
using System.Globalization;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for <see cref="RateLimitingMiddleware"/>.
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        /// <summary>
        /// Gets the current utilization percentage of the rate limiter.
        /// </summary>
        /// <param name="middleware">The <see cref="RateLimitingMiddleware"/> instance.</param>
        /// <returns>The utilization percentage.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static double GetUtilization(this RateLimitingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.GetStatus().UtilizationPercent;
        }

        /// <summary>
        /// Gets the number of currently active operations.
        /// </summary>
        /// <param name="middleware">The <see cref="RateLimitingMiddleware"/> instance.</param>
        /// <returns>The number of active operations.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static int GetActiveOperationCount(this RateLimitingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.GetStatus().ActiveOperations;
        }

        /// <summary>
        /// Gets the current rate limit status including available tokens and capacity.
        /// </summary>
        /// <param name="middleware">The <see cref="RateLimitingMiddleware"/> instance.</param>
        /// <returns>A <see cref="RateLimitStatus"/> object containing current rate limiting metrics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static RateLimitStatus GetStatus(this RateLimitingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.GetStatus();
        }

        /// <summary>
        /// Gets a detailed status report of the rate limiter.
        /// </summary>
        /// <param name="middleware">The <see cref="RateLimitingMiddleware"/> instance.</param>
        /// <returns>A formatted string representation of the current status.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static string GetStatusReport(this RateLimitingMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);

            var status = middleware.GetStatus();
            return string.Format(
                CultureInfo.InvariantCulture,
                "Active Operations: {0}/{1}, Tokens: {2}/{3}, Utilization: {4:P2}",
                status.ActiveOperations,
                status.MaxConcurrentOperations,
                status.AvailableTokens,
                status.MaxTokens,
                status.UtilizationPercent / 100.0);
        }
    }
}
