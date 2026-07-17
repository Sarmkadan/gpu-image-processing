using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Services
{
    /// <summary>
    /// Provides JSON serialization and deserialization extension methods for <see cref="FilterServiceTests"/>.
    /// </summary>
    public static class FilterServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a <see cref="FilterServiceTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">Whether to indent the JSON for readability.</param>
        /// <returns>A JSON string representation of the instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this FilterServiceTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? _jsonOptionsIndented : _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="FilterServiceTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
        /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
        public static FilterServiceTests? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<FilterServiceTests>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="FilterServiceTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out FilterServiceTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<FilterServiceTests>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}