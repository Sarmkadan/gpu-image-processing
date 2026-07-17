using System.Globalization;
using System.Text.Json;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Provides JSON serialization extensions for <see cref="PerformanceMonitoringServiceTests"/> to support test data persistence and exchange.
/// Uses invariant culture for consistent serialization behavior across different environments.
/// </summary>
public static class PerformanceMonitoringServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the <see cref="PerformanceMonitoringServiceTests"/> instance to a JSON string using invariant culture for consistent behavior.
    /// </summary>
    /// <param name="value">The test instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the test instance with camelCase property names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PerformanceMonitoringServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PerformanceMonitoringServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null.</param>
    /// <returns>The deserialized test instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PerformanceMonitoringServiceTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<PerformanceMonitoringServiceTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PerformanceMonitoringServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be null.</param>
    /// <param name="value">Receives the deserialized test instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out PerformanceMonitoringServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<PerformanceMonitoringServiceTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}