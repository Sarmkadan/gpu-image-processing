#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Repository
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for GenericRepository
    /// </summary>
    public static class GenericRepositoryJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Converts a GenericRepository to its JSON representation
        /// </summary>
        /// <typeparam name="T">The entity type stored in the repository</typeparam>
        /// <param name="value">The repository to serialize</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>JSON string representation of the repository</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
        public static string ToJson<T>(this GenericRepository<T> value, bool indented = false) where T : class
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonOptions)
                {
                    WriteIndented = true
                }
                : _jsonOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Creates a GenericRepository from its JSON representation
        /// </summary>
        /// <typeparam name="T">The entity type stored in the repository</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>Deserialized repository instance, or null if JSON is null or empty</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid</exception>
        public static GenericRepository<T>? FromJson<T>(string? json) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<GenericRepository<T>>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to create a GenericRepository from its JSON representation
        /// </summary>
        /// <typeparam name="T">The entity type stored in the repository</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <param name="value">Output parameter for the deserialized repository</param>
        /// <returns>True if deserialization succeeded; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
        public static bool TryFromJson<T>(string? json, out GenericRepository<T>? value) where T : class
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<GenericRepository<T>>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}