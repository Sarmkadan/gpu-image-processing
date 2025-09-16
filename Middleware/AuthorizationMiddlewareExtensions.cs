using System;
using System.Collections.Generic;
using System.Linq;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for the <see cref="AuthorizationMiddleware"/> class.
    /// </summary>
    public static class AuthorizationMiddlewareExtensions
    {
        /// <summary>
        /// Retrieves all active API keys.
        /// </summary>
        /// <param name="middleware">The <see cref="AuthorizationMiddleware"/> instance.</param>
        /// <returns>A read-only list of all currently active <see cref="ApiKeyInfo"/> entries.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static IReadOnlyList<ApiKeyInfo> GetActiveKeys(this AuthorizationMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.ListActiveKeys();
        }

        /// <summary>
        /// Retrieves all active API keys for a specified user.
        /// </summary>
        /// <param name="middleware">The <see cref="AuthorizationMiddleware"/> instance.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A read-only list of active <see cref="ApiKeyInfo"/> entries for the specified user.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is null or empty.</exception>
        public static IReadOnlyList<ApiKeyInfo> GetUserKeys(this AuthorizationMiddleware middleware, string userId)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            ArgumentException.ThrowIfNullOrEmpty(userId);
            return middleware.ListActiveKeys(userId);
        }

        /// <summary>
        /// Gets the total count of all active API keys.
        /// </summary>
        /// <param name="middleware">The <see cref="AuthorizationMiddleware"/> instance.</param>
        /// <returns>The total number of active API keys.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        public static int CountActiveKeys(this AuthorizationMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            return middleware.ListActiveKeys().Count;
        }

        /// <summary>
        /// Checks if there are any active API keys for the specified user.
        /// </summary>
        /// <param name="middleware">The <see cref="AuthorizationMiddleware"/> instance.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>True if the user has active API keys; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="middleware"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is null or empty.</exception>
        public static bool HasActiveKeysForUser(this AuthorizationMiddleware middleware, string userId)
        {
            ArgumentNullException.ThrowIfNull(middleware);
            ArgumentException.ThrowIfNullOrEmpty(userId);
            return middleware.ListActiveKeys(userId).Any();
        }
    }
}
