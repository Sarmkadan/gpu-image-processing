#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Constants
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="ProcessingStatusHelper"/>
    /// </summary>
    public static class ProcessingStatusHelperJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Converts a <see cref="ProcessingStatus"/> value to its JSON representation
        /// </summary>
        /// <param name="value">The processing status value to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability</param>
        /// <returns>A JSON string representing the processing status</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static string ToJson(this ProcessingStatus value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Parses a JSON string to a <see cref="ProcessingStatus"/> value
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized <see cref="ProcessingStatus"/> value</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
        public static ProcessingStatus FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ProcessingStatus>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to parse a JSON string to a <see cref="ProcessingStatus"/> value
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized value if successful</param>
        /// <returns>True if parsing succeeded; otherwise, false</returns>
        public static bool TryFromJson(string json, out ProcessingStatus? value)
        {
            value = default;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ProcessingStatus>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
