using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for <see cref="RequestMiddlewareContext"/>.
    /// </summary>
    public static class RequestMiddlewareContextExtensions
    {
        /// <summary>
        /// Checks if the request is authenticated by verifying if the UserId is present.
        /// </summary>
        /// <param name="context">The <see cref="RequestMiddlewareContext"/> instance.</param>
        /// <returns>True if the user is authenticated, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public static bool IsAuthenticated(this RequestMiddlewareContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return !string.IsNullOrEmpty(context.UserId);
        }

        /// <summary>
        /// Checks if the request has the specified scope.
        /// </summary>
        /// <param name="context">The <see cref="RequestMiddlewareContext"/> instance.</param>
        /// <param name="scope">The scope to check.</param>
        /// <returns>True if the scope is present, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="scope"/> is null or empty.</exception>
        public static bool HasScope(this RequestMiddlewareContext context, string scope)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrEmpty(scope);

            return context.Scopes?.Contains(scope) ?? false;
        }

        /// <summary>
        /// Adds or updates metadata in the request context.
        /// </summary>
        /// <param name="context">The <see cref="RequestMiddlewareContext"/> instance.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/>, <paramref name="key"/>, or <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
        public static void AddMetadata(this RequestMiddlewareContext context, string key, object value)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentNullException.ThrowIfNull(value);

            context.Metadata ??= new Dictionary<string, object>();
            context.Metadata[key] = value;
        }

        /// <summary>
        /// Retrieves all metadata keys from the request context.
        /// </summary>
        /// <param name="context">The <see cref="RequestMiddlewareContext"/> instance.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of metadata keys.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public static IEnumerable<string> GetMetadataKeys(this RequestMiddlewareContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Metadata?.Keys ?? Enumerable.Empty<string>();
        }
    }
}