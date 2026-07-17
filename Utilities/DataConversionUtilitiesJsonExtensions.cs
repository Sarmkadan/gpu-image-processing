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
    /// Provides System.Text.Json serialization extensions for DataConversionUtilities.
    /// </summary>
    public static class DataConversionUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes the DataConversionUtilities configuration to a JSON string.
        /// </summary>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the DataConversionUtilities configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indented"/> parameter is invalid.</exception>
        public static string ToJson(bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(indented);

            var config = new DataConversionUtilitiesConfiguration
            {
                SupportedExtensions = DataConversionUtilities.SupportedExtensions
            };

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(config, options);
        }

        /// <summary>
        /// Deserializes DataConversionUtilities configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized DataConversionUtilities configuration, or null if JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static DataConversionUtilitiesConfiguration? FromJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<DataConversionUtilitiesConfiguration>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize DataConversionUtilities configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized configuration if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string? json, out DataConversionUtilitiesConfiguration? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<DataConversionUtilitiesConfiguration>(json, _jsonSerializerOptions);
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Represents a serializable DataConversionUtilities configuration.
        /// </summary>
        public sealed class DataConversionUtilitiesConfiguration
        {
            /// <summary>
            /// Gets or sets the supported data conversion file extensions.
            /// </summary>
            public string[] SupportedExtensions { get; set; } = Array.Empty<string>();
        }
    }
}