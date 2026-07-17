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
/// Provides System.Text.Json serialization extensions for benchmark configuration and results.
/// </summary>
public static class EnumerableExtensionsBenchmarksExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Benchmark configuration for enumerable operations.
    /// </summary>
    private sealed class BenchmarkConfig
    {
        public int BatchSize { get; set; } = 32;
        public int ShuffleIterations { get; set; } = 100;
        public bool EnableValidation { get; set; } = true;
    }

    /// <summary>
    /// Serializes a benchmark configuration object to JSON.
    /// </summary>
    /// <param name="value">The configuration object to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format with indentation.</param>
    /// <returns>JSON string representation of the configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes JSON to a benchmark configuration.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Must not be null or whitespace.</param>
    /// <returns>Deserialized configuration instance or null if JSON is empty.</returns>
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

        return JsonSerializer.Deserialize<BenchmarkConfig>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize JSON to a benchmark configuration.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Must not be null.</param>
    /// <param name="value">Receives deserialized configuration if successful.</param>
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
            return value != null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}