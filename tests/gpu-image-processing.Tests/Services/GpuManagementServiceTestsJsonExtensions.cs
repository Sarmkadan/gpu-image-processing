using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Tests.Services;

/// <summary>
/// Provides JSON serialization extensions for <see cref="GpuManagementServiceTests"/>.
/// </summary>
public static class GpuManagementServiceTestsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes the <see cref="GpuManagementServiceTests"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The value to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the value.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this GpuManagementServiceTests value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(value, CreateOptions(indented));
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="GpuManagementServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized instance, or <see langword="null"/> if the JSON represents a null value.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static GpuManagementServiceTests? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return JsonSerializer.Deserialize<GpuManagementServiceTests>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="GpuManagementServiceTests"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized instance, or <see langword="null"/> if the JSON represents a null value.</param>
	/// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out GpuManagementServiceTests? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		try
		{
			value = JsonSerializer.Deserialize<GpuManagementServiceTests>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}

	/// <summary>
	/// Creates a <see cref="JsonSerializerOptions"/> instance with the specified indentation setting.
	/// </summary>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A configured <see cref="JsonSerializerOptions"/> instance.</returns>
	private static JsonSerializerOptions CreateOptions(bool indented)
		=> new(_jsonOptions)
		{
			WriteIndented = indented
		};
}
