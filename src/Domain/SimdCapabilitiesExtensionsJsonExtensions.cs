#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides System.Text.Json serialization helpers for SimdCapabilitiesExtensions.
/// </summary>
public static class SimdCapabilitiesExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Represents the configuration and state of SimdCapabilitiesExtensions extension methods.
    /// </summary>
    public sealed class SimdCapabilitiesExtensions
    {
        /// <summary>
        /// Gets or sets whether vector width support checking is enabled.
        /// </summary>
        public bool IsVectorWidthSupportEnabled { get; init; } = true;

        /// <summary>
        /// Gets or sets whether optimal SIMD level detection is enabled.
        /// </summary>
        public bool IsOptimalSimdLevelEnabled { get; init; } = true;

        /// <summary>
        /// Gets or sets whether SIMD availability checking is enabled.
        /// </summary>
        public bool IsSimdAvailabilityEnabled { get; init; } = true;

        /// <summary>
        /// Gets or sets whether friendly string formatting is enabled.
        /// </summary>
        public bool IsFriendlyStringEnabled { get; init; } = true;
    }

    /// <summary>
    /// Serializes a SimdCapabilitiesExtensions configuration to a JSON string.
    /// </summary>
    /// <param name="value">The configuration to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this SimdCapabilitiesExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a SimdCapabilitiesExtensions instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static SimdCapabilitiesExtensions? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SimdCapabilitiesExtensions>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a SimdCapabilitiesExtensions instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out SimdCapabilitiesExtensions? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<SimdCapabilitiesExtensions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
