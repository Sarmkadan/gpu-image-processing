using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuImageProcessing.Services;

/// <summary>
/// Provides JSON serialization extensions for <see cref="GpuManagementService"/>.
/// </summary>
public static class GpuManagementServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the <see cref="GpuManagementService"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The service instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this GpuManagementService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? GetIndentedOptions() : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="GpuManagementService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized service instance, or <see langword="null"/> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static GpuManagementService? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<GpuManagementService?>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="GpuManagementService"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized service instance, or <see langword="null"/> if the JSON represents a null value.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out GpuManagementService? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<GpuManagementService?>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }

    private static JsonSerializerOptions GetIndentedOptions() => new(_jsonOptions) { WriteIndented = true };

}