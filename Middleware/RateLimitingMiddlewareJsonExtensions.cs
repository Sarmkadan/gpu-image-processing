using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides extension methods for serializing and deserializing RateLimitingMiddleware instances to and from JSON.
    /// </summary>
    public static class RateLimitingMiddlewareJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Serializes the RateLimitingMiddleware instance to a JSON string.
        /// </summary>
        /// <param name="value">The RateLimitingMiddleware instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>The JSON string representation of the RateLimitingMiddleware instance.</returns>
        public static string ToJson(this RateLimitingMiddleware value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a RateLimitingMiddleware instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The RateLimitingMiddleware instance deserialized from the JSON string, or null if the JSON string is invalid.</returns>
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
        /// Attempts to deserialize a JSON string to a RateLimitingMiddleware instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The RateLimitingMiddleware instance deserialized from the JSON string, or null if the JSON string is invalid.</param>
        /// <returns>True if the JSON string was successfully deserialized to a RateLimitingMiddleware instance, false otherwise.</returns>
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
