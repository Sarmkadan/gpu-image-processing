using System;
using System.Text.Json;

#nullable enable

namespace GpuImageProcessing.Tests.Core.Services
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="CoreImageProcessingServiceTests"/>.
    /// </summary>
    public static class CoreImageProcessingServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        /// <summary>
        /// Serializes the <see cref="CoreImageProcessingServiceTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON representation of the value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this CoreImageProcessingServiceTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="CoreImageProcessingServiceTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized instance, or <see langword="null"/> if the JSON represents a null value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
        public static CoreImageProcessingServiceTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<CoreImageProcessingServiceTests>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="CoreImageProcessingServiceTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        public static bool TryFromJson(string json, out CoreImageProcessingServiceTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<CoreImageProcessingServiceTests>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = default;
                return false;
            }
        }
    }
}
