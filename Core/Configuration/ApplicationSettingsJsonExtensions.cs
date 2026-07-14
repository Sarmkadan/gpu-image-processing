#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Provides JSON serialization and deserialization helpers for ApplicationSettings
    /// </summary>
    public static class ApplicationSettingsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Serializes ApplicationSettings to a JSON string
        /// </summary>
        /// <param name="value">The ApplicationSettings instance to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability</param>
        /// <returns>A JSON string representation of the ApplicationSettings</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this ApplicationSettings value, bool indented = false)
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
        /// Deserializes a JSON string to an ApplicationSettings instance
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized ApplicationSettings instance, or null if JSON is invalid</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static ApplicationSettings? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ApplicationSettings>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an ApplicationSettings instance
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized ApplicationSettings instance if successful</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static bool TryFromJson(string json, out ApplicationSettings? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ApplicationSettings>(json, _jsonSerializerOptions);
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