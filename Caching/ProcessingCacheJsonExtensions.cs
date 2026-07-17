#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Caching
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="ProcessingCache"/>.
    /// </summary>
    public static class ProcessingCacheJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Serializes the <see cref="ProcessingCache"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The processing cache to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the processing cache.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ProcessingCache value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ProcessingCache"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized processing cache, or null if the JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static ProcessingCache? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<ProcessingCache>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="ProcessingCache"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized processing cache if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out ProcessingCache? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ProcessingCache>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
