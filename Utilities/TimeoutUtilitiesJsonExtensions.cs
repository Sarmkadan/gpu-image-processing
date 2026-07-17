#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for timeout-related operations.
    /// </summary>
    public static class TimeoutUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes timeout configuration to JSON string.
        /// </summary>
        /// <param name="timeout">The timeout value to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the timeout configuration.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="timeout"/> is negative.</exception>
        public static string ToJson(TimeSpan timeout, bool indented = false)
        {
            if (timeout < TimeSpan.Zero)
                throw new ArgumentException("Timeout cannot be negative", nameof(timeout));

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            var timeoutConfig = new TimeoutConfiguration
            {
                Milliseconds = timeout.TotalMilliseconds,
                Formatted = TimeoutUtilities.FormatTimeout(timeout)
            };

            return JsonSerializer.Serialize(timeoutConfig, options);
        }

        /// <summary>
        /// Deserializes a timeout configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized timeout configuration, or null if JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static TimeoutConfiguration? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonSerializer.Deserialize<TimeoutConfiguration>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a timeout configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized timeout configuration if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out TimeoutConfiguration? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                value = JsonSerializer.Deserialize<TimeoutConfiguration>(json, _jsonSerializerOptions);
                return value != null && value.Milliseconds >= 0;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Represents a serializable timeout configuration.
        /// </summary>
        public sealed class TimeoutConfiguration
        {
            /// <summary>Gets or sets the timeout duration in milliseconds.</summary>
            public double Milliseconds { get; set; }

            /// <summary>Gets or sets the human-readable formatted timeout string.</summary>
            public string? Formatted { get; set; }

            /// <summary>
            /// Converts the timeout configuration to a TimeSpan.
            /// </summary>
            /// <returns>The TimeSpan representation.</returns>
            /// <exception cref="InvalidOperationException">Thrown when <see cref="Milliseconds"/> is negative.</exception>
            public TimeSpan ToTimeSpan()
            {
                if (Milliseconds < 0)
                    throw new InvalidOperationException("Milliseconds cannot be negative");

                return TimeSpan.FromMilliseconds(Milliseconds);
            }
        }
    }
}