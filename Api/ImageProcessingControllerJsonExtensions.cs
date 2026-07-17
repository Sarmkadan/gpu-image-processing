#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Api
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="ImageProcessingController"/>.
    /// </summary>
    public static class ImageProcessingControllerJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Serializes the <see cref="ImageProcessingController"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The image processing controller to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the image processing controller.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ImageProcessingController value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to an <see cref="ImageProcessingController"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized image processing controller, or null if the JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static ImageProcessingController? FromJson(string json)
        {
    ArgumentNullException.ThrowIfNull(json);

    return string.IsNullOrEmpty(json)
        ? null
        : JsonSerializer.Deserialize<ImageProcessingController>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an <see cref="ImageProcessingController"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized image processing controller if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>

/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
public static bool TryFromJson(string json, out ImageProcessingController? value)
{
    ArgumentNullException.ThrowIfNull(json);
    value = null;

    return !string.IsNullOrEmpty(json) && (value = JsonSerializer.Deserialize<ImageProcessingController>(json, _jsonSerializerOptions)) is not null;
}
    }
}