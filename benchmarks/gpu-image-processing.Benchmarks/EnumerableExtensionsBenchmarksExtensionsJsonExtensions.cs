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
/// Provides System.Text.Json serialization extensions for EnumerableExtensionsBenchmarksExtensions
/// benchmark configuration and results.
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
    /// Serializes an EnumerableExtensionsBenchmarksExtensions instance to JSON.
    /// </summary>
    /// <param name="value">The instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format with indentation.</param>
    /// <returns>JSON string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var config = new BenchmarkConfig
        {
            BatchSize = 32,
            ShuffleIterations = 100,
            EnableValidation = true
        };

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Deserializes JSON to an EnumerableExtensionsBenchmarksExtensions instance.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized instance or null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
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
    /// Attempts to deserialize JSON to an EnumerableExtensionsBenchmarksExtensions instance.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Receives deserialized instance if successful.</param>
    /// <returns>True if successful; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null.</exception>
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
