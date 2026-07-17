#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="DeviceCommand"/>.
    /// </summary>
    public static class DeviceCommandJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the <see cref="DeviceCommand"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The device command to serialize. Must not be null.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the device command.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public static string ToJson(this DeviceCommand value, bool indented = false)
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
        /// Deserializes a JSON string to a <see cref="DeviceCommand"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
        /// <returns>
        /// The deserialized device command if the JSON is valid and deserialization succeeds;
        /// otherwise, null.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
        /// <remarks>
        /// This method catches <see cref="JsonException"/> internally and returns null
        /// rather than propagating the exception.
        /// </remarks>
        public static DeviceCommand? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<DeviceCommand>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="DeviceCommand"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Must not be null or empty.</param>
        /// <param name="value">
        /// Receives the deserialized device command if deserialization succeeds;
        /// otherwise, receives null.
        /// </param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
        /// <remarks>
        /// This method provides a non-exception-throwing alternative to <see cref="FromJson"/>.
        /// </remarks>
        public static bool TryFromJson(string json, out DeviceCommand? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<DeviceCommand>(json, _jsonSerializerOptions);
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
