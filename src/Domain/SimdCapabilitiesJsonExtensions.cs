using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace GpuImageProcessing.Domain;

public static class SimdCapabilitiesJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="SimdCapabilities"/> value to a JSON string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SimdCapabilities value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = new JsonSerializerOptions(_options) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SimdCapabilities"/> value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
    public static SimdCapabilities FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<SimdCapabilities>(json, _options)!;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SimdCapabilities"/> value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized value if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out SimdCapabilities? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<SimdCapabilities>(json, _options);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}