#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace GpuImageProcessing.Cli
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="ProcessImageCommand"/>.
    /// </summary>
    public static class ProcessImageCommandJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        /// <summary>
        /// Serializes the <see cref="ProcessImageCommand"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The process image command to serialize.</param>
        /// <param name="indented">Whether to indent the JSON for readability.</param>
        /// <returns>The JSON string representation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ProcessImageCommand value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ProcessImageCommand"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized process image command, or <see langword="null"/> if the JSON is empty.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
        public static ProcessImageCommand? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ProcessImageCommand>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="ProcessImageCommand"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized process image command if successful.</param>
        /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson(string json, out ProcessImageCommand? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            value = default;

            try
            {
                value = JsonSerializer.Deserialize<ProcessImageCommand>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}