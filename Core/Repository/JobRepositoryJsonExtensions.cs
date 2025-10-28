#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for JobRepository
    /// </summary>
    public static class JobRepositoryJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Converts a JobRepository to its JSON representation
        /// </summary>
        /// <param name="value">The repository to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>JSON string representation of the repository</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this JobRepository value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                { WriteIndented = true }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Creates a JobRepository from its JSON representation
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>Deserialized repository instance, or null if JSON is null or empty</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid</exception>
        public static JobRepository? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<JobRepository>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to create a JobRepository from its JSON representation
        /// </summary>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="value">Output parameter for the deserialized repository</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        public static bool TryFromJson(string json, out JobRepository? value)
        {
            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<JobRepository>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}