#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="ProcessingEvent"/> types.
    /// </summary>
    public static class ProcessingEventJsonExtensions
    {
        /// <summary>
        /// JSON serializer options with camelCase naming policy.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Converts a <see cref="ProcessingEvent"/> to its JSON representation.
        /// </summary>
        /// <param name="value">The processing event to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>The JSON string representation of the processing event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ProcessingEvent value, bool indented = false)
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
        /// Parses a JSON string into a <see cref="ProcessingEvent"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized processing event, or null if parsing fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static ProcessingEvent? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<ProcessingEvent>(json, _jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to deserialize ProcessingEvent from JSON", ex);
            }
        }

        /// <summary>
        /// Attempts to parse a JSON string into a <see cref="ProcessingEvent"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized processing event if successful; otherwise, null.</param>
        /// <returns>True if parsing succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        public static bool TryFromJson(string json, out ProcessingEvent? value)
        {
            value = null;

            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ProcessingEvent>(json, _jsonSerializerOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}