#nullable enable
using System;
using System.Text.Json;

namespace GpuImageProcessing.Core.Constants
{
    /// <summary>
    /// JSON (de)serialization helpers for <see cref="ImageFormat"/>.
    /// </summary>
    public static class ImageFormatHelperJsonExtensions
    {
        private static readonly JsonSerializerOptions _baseOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serialises the <see cref="ImageFormat"/> value to a JSON string.
        /// </summary>
        /// <param name="format">The image format to serialise.</param>
        /// <param name="indented">
        /// If <c>true</c>, the output JSON will be formatted with indentation;
        /// otherwise it will be compact.
        /// </param>
        /// <returns>A JSON representation of <paramref name="format"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="format"/> is not a defined enum value.</exception>
        public static string ToJson(this ImageFormat format, bool indented = false)
        {
            // Guard: ensure the enum value is defined.
            if (!Enum.IsDefined(typeof(ImageFormat), format))
            {
                throw new ArgumentException($"Undefined {nameof(ImageFormat)} value: {format}.", nameof(format));
            }

            // Create a copy of the base options to avoid modifying shared state
            var options = new JsonSerializerOptions(_baseOptions)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(format, options);
        }

        /// <summary>
        /// Deserialises a JSON string to an <see cref="ImageFormat"/> value.
        /// </summary>
        /// <param name="json">The JSON string representing an <see cref="ImageFormat"/>.</param>
        /// <returns>The deserialized <see cref="ImageFormat"/> value, or <c>null</c> if the JSON is <c>null</c> or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        /// <exception cref="JsonException">Thrown when the JSON cannot be deserialized to <see cref="ImageFormat"/>.</exception>
        public static ImageFormat? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ImageFormat>(json, _baseOptions);
        }

        /// <summary>
        /// Attempts to deserialise a JSON string to an <see cref="ImageFormat"/> value.
        /// </summary>
        /// <param name="json">The JSON string representing an <see cref="ImageFormat"/>.</param>
        /// <param name="value">
        /// When this method returns, contains the deserialized <see cref="ImageFormat"/> value
        /// if the operation succeeded; otherwise <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        public static bool TryFromJson(string json, out ImageFormat? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = FromJson(json);
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