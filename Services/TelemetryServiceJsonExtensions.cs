#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="TelemetryService"/> and related types.
    /// </summary>
    public static class TelemetryServiceJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Converts a <see cref="TelemetryService"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="TelemetryService"/> instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the <see cref="TelemetryService"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this TelemetryService value, bool indented = false)
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
        /// Deserializes a JSON string into a <see cref="TelemetryService"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="TelemetryService"/> instance, or <see langword="null"/> if deserialization fails.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
        public static TelemetryService? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<TelemetryService>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="TelemetryService"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized <see cref="TelemetryService"/> instance, or <see langword="null"/> on failure.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
        public static bool TryFromJson(string json, out TelemetryService? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<TelemetryService>(json, _jsonSerializerOptions);
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