using System;
using System.Text.Json;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for serializing and deserializing <see cref="RateLimitingMiddleware"/> instances to and from JSON.
    /// </summary>
    public static class RateLimitingMiddlewareJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Serializes the <see cref="RateLimitingMiddleware"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="RateLimitingMiddleware"/> instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>The JSON string representation of the <see cref="RateLimitingMiddleware"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this RateLimitingMiddleware value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="RateLimitingMiddleware"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The <see cref="RateLimitingMiddleware"/> instance deserialized from the JSON string, or <see langword="null"/> if the JSON string is invalid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        public static RateLimitingMiddleware? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                return JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="RateLimitingMiddleware"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the <see cref="RateLimitingMiddleware"/> instance deserialized from the JSON string,
        /// or <see langword="null"/> if the JSON string is missing or invalid.</param>
        /// <returns><see langword="true"/> if the JSON string was successfully deserialized to a <see cref="RateLimitingMiddleware"/> instance;
        /// otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson(string json, out RateLimitingMiddleware? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                value = JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
