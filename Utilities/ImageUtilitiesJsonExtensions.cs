#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Utilities
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="ImageUtilities"/>.
    /// </summary>
    public static class ImageUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes ImageUtilities configuration to JSON string.
        /// </summary>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the ImageUtilities configuration.</returns>
        public static string ToJson(bool indented = false)
        {
            var config = new ImageUtilitiesConfiguration
            {
                SupportedExtensions = ImageUtilities.SupportedExtensions
            };

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(config, options);
        }

        /// <summary>
        /// Deserializes ImageUtilities configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized ImageUtilities configuration, or null if JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static ImageUtilitiesConfiguration? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ImageUtilitiesConfiguration>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize ImageUtilities configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized configuration if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out ImageUtilitiesConfiguration? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ImageUtilitiesConfiguration>(json, _jsonSerializerOptions);
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Represents a serializable ImageUtilities configuration.
        /// </summary>
        public sealed class ImageUtilitiesConfiguration
        {
            /// <summary>Gets or sets the supported image file extensions.</summary>
            public string[] SupportedExtensions { get; set; } = Array.Empty<string>();
        }
    }
}