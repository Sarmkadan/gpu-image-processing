#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for ProcessingJob
    /// </summary>
    public static class ProcessingJobJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        /// <summary>
        /// Converts a ProcessingJob instance to its JSON representation
        /// </summary>
        /// <param name="value">The ProcessingJob instance to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability</param>
        /// <returns>A JSON string representation of the ProcessingJob</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this ProcessingJob value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
        }

        /// <summary>
        /// Parses a JSON string and creates a ProcessingJob instance
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>A ProcessingJob instance, or null if the JSON is invalid</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static ProcessingJob? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ProcessingJob>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to parse a JSON string and create a ProcessingJob instance
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Receives the deserialized ProcessingJob if successful</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentException">Thrown when json is null or empty</exception>
        public static bool TryFromJson(string json, out ProcessingJob? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ProcessingJob>(json, _jsonOptions);
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