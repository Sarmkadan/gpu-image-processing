#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Exceptions
{
    /// <summary>
    /// Provides System.Text.Json serialization and deserialization helpers for <see cref="ImageProcessingException"/>
    /// </summary>
    public static class ImageProcessingExceptionJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes an <see cref="ImageProcessingException"/> to a JSON string
        /// </summary>
        /// <param name="value">The exception to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>A JSON string representation of the exception</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static string ToJson(this ImageProcessingException value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes an <see cref="ImageProcessingException"/> from a JSON string
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized exception, or null if the JSON is invalid</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace</exception>
        public static ImageProcessingException? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json.Trim());

            try
            {
                return JsonSerializer.Deserialize<ImageProcessingException>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize an <see cref="ImageProcessingException"/> from a JSON string
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized exception if successful</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
        public static bool TryFromJson(string json, out ImageProcessingException? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<ImageProcessingException>(json, _jsonOptions);
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