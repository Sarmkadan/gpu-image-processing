#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="CliParser"/>.
    /// </summary>
    public static class CliParserJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes the <see cref="CliParser"/> instance to a JSON string using camelCase property naming.
        /// </summary>
        /// <param name="value">The CLI parser instance to serialize. Cannot be null.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the CLI parser.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this CliParser value, bool indented = false)
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
        /// Deserializes a JSON string to a <see cref="CliParser"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Can be null or empty.</param>
        /// <returns>The deserialized <see cref="CliParser"/> instance, or null if the JSON is null or empty.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
        public static CliParser? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<CliParser>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="CliParser"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize. Can be null or empty.</param>
        /// <param name="value">Receives the deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or whitespace.</exception>
        public static bool TryFromJson(string json, out CliParser? value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<CliParser>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}