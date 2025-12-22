using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides validation helpers for <see cref="RateLimitingMiddleware"/> instances.
    /// </summary>
    public static class RateLimitingMiddlewareValidation
    {
        /// <summary>
        /// Validates the specified <see cref="RateLimitingMiddleware"/> instance.
        /// </summary>
        /// <param name="value">The middleware instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this RateLimitingMiddleware value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="RateLimitingMiddleware"/> instance is valid.
        /// </summary>
        /// <param name="value">The middleware instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this RateLimitingMiddleware value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="RateLimitingMiddleware"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The middleware instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValid(this RateLimitingMiddleware value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"RateLimitingMiddleware is invalid. Validation errors: {string.Join(" ", problems)}");
            }
        }
    }
}