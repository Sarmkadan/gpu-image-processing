#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Integration
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="MetricsPublisher"/>.
    /// </summary>
    public static class MetricsPublisherJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes the <see cref="MetricsPublisher"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The metrics publisher instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the metrics publisher.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this MetricsPublisher value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var state = new MetricsPublisherState
            {
                BufferSize = value.BufferSize,
                EndpointCount = value.EndpointCount
            };

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(state, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="MetricsPublisher"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized <see cref="MetricsPublisher"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static MetricsPublisher? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            var state = JsonSerializer.Deserialize<MetricsPublisherState>(json, _jsonOptions);

            if (state == null)
            {
                throw new JsonException("Failed to deserialize MetricsPublisher state from JSON.");
            }

            return new MetricsPublisher(state.BufferSize);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="MetricsPublisher"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="MetricsPublisher"/> instance, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out MetricsPublisher? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = FromJson(json);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private sealed class MetricsPublisherState
        {
            public int BufferSize { get; set; }

            public int EndpointCount { get; set; }
        }
    }
}