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
    /// Provides System.Text.Json serialization extensions for <see cref="CommandHandler"/>.
    /// </summary>
    public static class CommandHandlerJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the <see cref="CommandHandler"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The command handler to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the command handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this CommandHandler value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="CommandHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized command handler instance, or null if the JSON is invalid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static CommandHandler? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<CommandHandler>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="CommandHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized command handler if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out CommandHandler? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<CommandHandler>(json, _jsonOptions);
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