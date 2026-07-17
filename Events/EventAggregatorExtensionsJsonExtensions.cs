#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="EventAggregator"/> instances.
    /// </summary>
    public static class EventAggregatorExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Serializes the EventAggregator instance to a JSON string.
        /// </summary>
        /// <param name="value">The event aggregator instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>JSON representation of the event aggregator instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this EventAggregator value, bool indented = false)
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
        /// Deserializes a JSON string to an EventAggregator instance.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <returns>Deserialized event aggregator instance, or <see langword="null"/> if JSON is <see langword="null"/> or empty.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static EventAggregator? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<EventAggregator>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an EventAggregator instance.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <param name="value">Output parameter containing the deserialized instance if successful.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        public static bool TryFromJson(string json, out EventAggregator? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<EventAggregator>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}