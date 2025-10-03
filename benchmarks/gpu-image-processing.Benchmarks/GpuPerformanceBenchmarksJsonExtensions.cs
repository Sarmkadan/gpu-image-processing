#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// JSON serialization helpers for GPU performance benchmarks
// =============================================================================

using System;
using System.Text.Json;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Provides JSON serialization helpers for <see cref="GpuPerformanceBenchmarks"/>.
/// </summary>
public static class GpuPerformanceBenchmarksJsonExtensions
{
    /// <summary>
    /// JSON serializer options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="GpuPerformanceBenchmarks"/> to a JSON string.
    /// </summary>
    /// <param name="value">The benchmarks to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the benchmarks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this GpuPerformanceBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        JsonSerializerOptions options = new(JsonSerializerOptions)
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="GpuPerformanceBenchmarks"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized benchmarks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
    public static GpuPerformanceBenchmarks? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<GpuPerformanceBenchmarks>(json, JsonSerializerOptions);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="GpuPerformanceBenchmarks"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized benchmarks if successful; otherwise null.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out GpuPerformanceBenchmarks? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<GpuPerformanceBenchmarks>(json, JsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}