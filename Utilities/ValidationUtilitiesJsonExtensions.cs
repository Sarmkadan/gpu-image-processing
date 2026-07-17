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
    /// Provides System.Text.Json serialization extensions for <see cref="ValidationResult"/> type.
    /// </summary>
    public static class ValidationUtilitiesJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a <see cref="ValidationResult"/> instance to a JSON string.
        /// </summary>
        /// <param name="result">The validation result to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the validation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <see langword="null"/>.</exception>
        public static string ToJson(this ValidationResult result, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(result);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(result, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ValidationResult"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized <see cref="ValidationResult"/> instance if successful; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
        public static ValidationResult? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                return JsonSerializer.Deserialize<ValidationResult>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="ValidationResult"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace.</exception>
        public static bool TryFromJson(string json, out ValidationResult? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);

            try
            {
                value = JsonSerializer.Deserialize<ValidationResult>(json, _jsonSerializerOptions);
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
