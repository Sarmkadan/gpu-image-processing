using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides validation helpers for <see cref="AuthorizationMiddleware"/> instances.
    /// </summary>
    public static class AuthorizationMiddlewareValidation
    {
        /// <summary>
        /// Validates an <see cref="AuthorizationMiddleware"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The middleware instance to validate.</param>
        /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AuthorizationMiddleware value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether an <see cref="AuthorizationMiddleware"/> instance is valid.
        /// </summary>
        /// <param name="value">The middleware instance to check.</param>
        /// <returns>True if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this AuthorizationMiddleware value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that an <see cref="AuthorizationMiddleware"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
        /// </summary>
        /// <param name="value">The middleware instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this AuthorizationMiddleware value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"AuthorizationMiddleware is invalid. Validation errors: {string.Join(" ", errors)}");
            }
        }
    }
}