#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Benchmarks;

/// <summary>
/// Provides System.Text.Json serialization extensions for FilterChainBenchmarksExtensions
/// benchmark configuration and results.
/// </summary>
public static class FilterChainBenchmarksExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Configuration for FilterChainBenchmarksExtensions operations.
    /// </summary>
    private sealed class BenchmarkConfig
    {
        public int FilterCount { get; set; } = 10;
        public bool EnableValidation { get; set; } = true;
        public bool EnableCloning { get; set; } = true;
        public int MaxParallelSteps { get; set; } = 4;
    }

    /// <summary>
    /// Serializes a FilterChainBenchmarksExtensions configuration to a JSON string.
    /// </summary>
    /// <param name="value">The configuration to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format with indentation.</param>
    /// <returns>JSON string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a FilterChainBenchmarksExtensions configuration.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized configuration or null if JSON is empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    public static object? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BenchmarkConfig>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a FilterChainBenchmarksExtensions configuration.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Receives deserialized instance if successful.</param>
    /// <returns>True if successful; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<BenchmarkConfig>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
