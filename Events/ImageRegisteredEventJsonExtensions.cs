#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Provides JSON serialization helpers for <see cref="ImageRegisteredEvent"/>.
    /// </summary>
    public static class ImageRegisteredEventJsonExtensions
    {
        /// <summary>
        /// JSON serializer options with camelCase naming policy.
        /// </summary>
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes an <see cref="ImageRegisteredEvent"/> to a JSON string.
        /// </summary>
        /// <param name="value">The event to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>JSON string representation of the event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ImageRegisteredEvent value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            JsonSerializerOptions options = new(JsonSerializerOptions)
            {
                WriteIndented = indented
            };
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an <see cref="ImageRegisteredEvent"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
        public static ImageRegisteredEvent? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return JsonSerializer.Deserialize<ImageRegisteredEvent>(json, JsonSerializerOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string to an <see cref="ImageRegisteredEvent"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized event if successful; otherwise null.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJson(string json, out ImageRegisteredEvent? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                value = JsonSerializer.Deserialize<ImageRegisteredEvent>(json, JsonSerializerOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
