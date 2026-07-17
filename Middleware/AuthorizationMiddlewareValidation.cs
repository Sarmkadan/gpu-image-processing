using System;
using System.Collections.Generic;
using System.Linq;

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

            var errors = new List<string>();

            // Validate API keys
            var activeKeys = value.ListActiveKeys();
            if (activeKeys.Count == 0)
            {
                errors.Add("No active API keys registered");
            }
            else
            {
                foreach (var key in activeKeys)
                {
                    if (string.IsNullOrWhiteSpace(key.UserId))
                    {
                        errors.Add("API key has null or empty UserId");
                    }

                    if (key.Scopes == null || key.Scopes.Count == 0)
                    {
                        errors.Add($"API key for user '{key.UserId}' has no scopes defined");
                    }
                }
            }

            // Validate user roles
            var allUserIds = activeKeys.Select(k => k.UserId).Distinct().ToList();
            foreach (var userId in allUserIds)
            {
                if (value.GetUserRole(userId) == null)
                {
                    errors.Add($"User '{userId}' has no role assigned");
                }
            }

            return errors.AsReadOnly();
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
                    $"AuthorizationMiddleware is invalid. Validation errors: {string.Join("; ", errors)}");
            }
        }

        /// <summary>
        /// Gets the role assigned to a user.
        /// </summary>
        /// <param name="middleware">The middleware instance.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's role, or null if not assigned.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="middleware"/> or <paramref name="userId"/> is null.</exception>
        private static UserRole? GetUserRole(this AuthorizationMiddleware middleware, string userId)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            ArgumentNullException.ThrowIfNull(userId);

            // Access the private _userRoles field via reflection
            var field = typeof(AuthorizationMiddleware).GetField("_userRoles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var userRoles = field.GetValue(middleware) as Dictionary<string, UserRole>;
                if (userRoles != null && userRoles.TryGetValue(userId, out var role))
                {
                    return role;
                }
            }

            return null;
        }
    }
}