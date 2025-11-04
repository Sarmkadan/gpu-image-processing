#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Middleware
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="CompressionMiddleware"/>.
    /// </summary>
    public static class CompressionMiddlewareJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a <see cref="CompressionMiddleware"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The middleware instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the middleware.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this CompressionMiddleware value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="CompressionMiddleware"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized middleware instance, or null if the JSON is null or empty.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static CompressionMiddleware? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<CompressionMiddleware>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="CompressionMiddleware"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized middleware instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        public static bool TryFromJson(string json, out CompressionMiddleware? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<CompressionMiddleware>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}