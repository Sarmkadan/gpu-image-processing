#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for ResultRepository
    /// </summary>
    public static class ResultRepositoryJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Converts a ResultRepository to its JSON representation
        /// </summary>
        /// <param name="value">The repository to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>JSON string representation of the repository</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this ResultRepository value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions);
        }

        /// <summary>
        /// Creates a ResultRepository from its JSON representation
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>Deserialized repository instance, or null if JSON is null or whitespace</returns>
        /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid</exception>
        public static ResultRepository? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ResultRepository>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to create a ResultRepository from its JSON representation
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="value">Output parameter for the deserialized repository</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
        public static bool TryFromJson(string json, out ResultRepository? value)
        {
            value = null;

            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ResultRepository>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}