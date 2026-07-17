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
    /// Provides System.Text.Json serialization extensions for PathUtilities configuration.
    /// </summary>
    public static class PathUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes PathUtilities configuration to a JSON string.
        /// </summary>
        /// <param name="configuration">The PathUtilities configuration to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the PathUtilities configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public static string ToJson(PathUtilitiesConfiguration configuration, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(configuration, options);
        }

        /// <summary>
        /// Deserializes PathUtilities configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized PathUtilities configuration, or null if JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static PathUtilitiesConfiguration? FromJson(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PathUtilitiesConfiguration>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize PathUtilities configuration from JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized configuration if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string? json, out PathUtilitiesConfiguration? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<PathUtilitiesConfiguration>(json, _jsonSerializerOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Represents a serializable PathUtilities configuration.
        /// </summary>
        public sealed class PathUtilitiesConfiguration
        {
            private string[] _supportedExtensions = Array.Empty<string>();

            /// <summary>
            /// Gets or sets the supported file extensions.
            /// </summary>
            /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
            public string[] SupportedExtensions
            {
                get => _supportedExtensions;
                set => _supportedExtensions = value ?? throw new ArgumentNullException(nameof(value));
            }

            /// <summary>
            /// Gets or sets whether path normalization is enabled.
            /// </summary>
            public bool DefaultPathNormalization { get; set; }

            /// <summary>
            /// Gets or sets whether cross-platform support is enabled.
            /// </summary>
            public bool CrossPlatformSupport { get; set; }
        }
    }
}