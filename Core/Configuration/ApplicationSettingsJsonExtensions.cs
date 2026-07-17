#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Configuration
{
    /// <summary>
    /// Provides JSON serialization and deserialization helpers for <see cref="ApplicationSettings"/> instances.
    /// </summary>
    /// <remarks>
    /// This class uses System.Text.Json with camelCase property naming policy and ignores null values during serialization.
    /// All serialization operations are culture-invariant to ensure consistent behavior across different systems.
    /// </remarks>
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
        /// Serializes an <see cref="ApplicationSettings"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="ApplicationSettings"/> instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability. Defaults to <see langword="true"/> for better debugging.</param>
        /// <returns>A JSON string representation of the <see cref="ApplicationSettings"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ApplicationSettings value, bool indented = true)
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
        /// Deserializes a JSON string to an <see cref="ApplicationSettings"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="ApplicationSettings"/> instance, or <see langword="null"/> if JSON is invalid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized into an <see cref="ApplicationSettings"/> instance.</exception>
        public static ApplicationSettings? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ApplicationSettings>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an <see cref="ApplicationSettings"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized <see cref="ApplicationSettings"/> instance if successful.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
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
