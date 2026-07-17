#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Integration;

/// <summary>
/// Represents configuration settings for concurrency and configuration testing scenarios.
/// </summary>
/// <param name="MaxConcurrentThreads">Maximum number of concurrent threads to use during testing.</param>
/// <param name="TestDurationSeconds">Duration in seconds for each test run.</param>
/// <param name="EnableGpuAcceleration">Whether GPU acceleration should be enabled for tests.</param>
/// <param name="MemoryLimitMb">Maximum memory limit in megabytes for test operations.</param>
/// <param name="ConcurrencyStrategy">The concurrency strategy to apply during testing.</param>
public sealed record ConcurrencyAndConfigurationTestSettings(
    int MaxConcurrentThreads = 8,
    int TestDurationSeconds = 30,
    bool EnableGpuAcceleration = true,
    long MemoryLimitMb = 512,
    ConcurrencyStrategy ConcurrencyStrategy = ConcurrencyStrategy.Balanced)
{
    /// <summary>
    /// Gets a value indicating whether the settings are valid.
    /// </summary>
    public bool IsValid => MaxConcurrentThreads > 0 && TestDurationSeconds > 0 && MemoryLimitMb > 0;
}

/// <summary>
/// Defines the concurrency strategy to apply during testing.
/// </summary>
public enum ConcurrencyStrategy
{
    /// <summary>
    /// Balanced approach with moderate concurrency, suitable for most scenarios.
    /// </summary>
    Balanced,

    /// <summary>
    /// Maximize throughput with high concurrency, prioritizing processing speed over resource usage.
    /// </summary>
    ThroughputMaximized,

    /// <summary>
    /// Minimize latency with low concurrency, prioritizing response time over processing speed.
    /// </summary>
    LatencyMinimized,

    /// <summary>
    /// Optimize for memory efficiency, minimizing memory footprint at the cost of processing speed.
    /// </summary>
    MemoryOptimized
}

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ConcurrencyAndConfigurationTestSettings"/>.
/// </summary>
public static class ConcurrencyAndConfigurationTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="ConcurrencyAndConfigurationTestSettings"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The settings instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the settings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ConcurrencyAndConfigurationTestSettings value, bool indented = false)
        => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ConcurrencyAndConfigurationTestSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized settings instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ConcurrencyAndConfigurationTestSettings? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<ConcurrencyAndConfigurationTestSettings>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ConcurrencyAndConfigurationTestSettings"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized settings instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out ConcurrencyAndConfigurationTestSettings? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ConcurrencyAndConfigurationTestSettings>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}