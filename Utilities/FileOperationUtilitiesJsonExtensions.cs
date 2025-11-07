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
    /// Provides System.Text.Json serialization extensions for <see cref="FileOperationUtilities"/>
    /// </summary>
    public static class FileOperationUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a <see cref="FileOperationUtilities"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>A JSON string representation of the instance</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static string ToJson(this FileOperationUtilities value, bool indented = false)
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
        /// Deserializes a JSON string to a <see cref="FileOperationUtilities"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>A deserialized instance, or null if the JSON is empty or whitespace</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
        public static FileOperationUtilities? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<FileOperationUtilities>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="FileOperationUtilities"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized instance if successful</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        public static bool TryFromJson(string json, out FileOperationUtilities? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<FileOperationUtilities>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}