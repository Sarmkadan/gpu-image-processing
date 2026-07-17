#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Pipeline;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="BatchProcessingPipeline"/>
/// and related types using invariant culture for consistent JSON formatting.
/// </summary>
public static class BatchProcessingPipelineJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string.
    /// </summary>
    /// <param name="value">The pipeline instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the pipeline.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="value"/> is null.</exception>
    public static string ToJson(this BatchProcessingPipeline value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="BatchProcessingPipeline"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="BatchProcessingPipeline"/> instance, or null if JSON is invalid.</returns>
    /// <exception cref="ArgumentException">When <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">When the JSON is malformed and cannot be deserialized.</exception>
    public static BatchProcessingPipeline? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<BatchProcessingPipeline>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BatchProcessingPipeline"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">When <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out BatchProcessingPipeline? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<BatchProcessingPipeline>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}