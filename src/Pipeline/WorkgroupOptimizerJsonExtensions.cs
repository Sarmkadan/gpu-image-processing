#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="WorkgroupOptimizer"/>.
/// </summary>
/// <remarks>
/// This static class offers extension methods for serializing <see cref="WorkgroupOptimizer"/> instances
/// to JSON strings and deserializing them back, using camelCase property naming and
/// <see cref="JsonSerializerDefaults.Web"/> defaults.
/// </remarks>
public static class WorkgroupOptimizerJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes the <see cref="WorkgroupOptimizer"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The optimizer instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the optimizer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this WorkgroupOptimizer value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="WorkgroupOptimizer"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must be valid JSON representing a <see cref="WorkgroupOptimizer"/>.</param>
    /// <returns>
    /// The deserialized <see cref="WorkgroupOptimizer"/> instance if the JSON is non-empty and valid;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into a <see cref="WorkgroupOptimizer"/> instance.</exception>
    public static WorkgroupOptimizer? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<WorkgroupOptimizer>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="WorkgroupOptimizer"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// Receives the deserialized instance if deserialization succeeds; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out WorkgroupOptimizer? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<WorkgroupOptimizer>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}