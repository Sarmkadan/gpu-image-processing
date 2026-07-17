#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Domain;

/// <summary>
/// Provides JSON serialization helpers for <see cref="ComputeShaderPass"/> and related types.
/// </summary>
public static class ComputeShaderPassJsonExtensions
{
	/// <summary>
	/// Configures JSON serialization options with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		Converters = { new JsonStringEnumConverter() }
	};

	/// <summary>
	/// Serializes a <see cref="ComputeShaderPass"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="ComputeShaderPass"/> to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the object.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this ComputeShaderPass value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		JsonOptions.WriteIndented = indented;
		return JsonSerializer.Serialize(value, JsonOptions);
	}

	/// <summary>
	/// Deserializes a <see cref="ComputeShaderPass"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="ComputeShaderPass"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
	/// <exception cref="JsonException">Thrown when JSON parsing fails.</exception>
	public static ComputeShaderPass FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		return JsonSerializer.Deserialize<ComputeShaderPass>(json, JsonOptions)
			?? throw new JsonException("Failed to deserialize ComputeShaderPass");
	}

	/// <summary>
	/// Tries to deserialize a <see cref="ComputeShaderPass"/> instance from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized <see cref="ComputeShaderPass"/> if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
	public static bool TryFromJson(string json, out ComputeShaderPass? value)
	{
		ArgumentNullException.ThrowIfNull(json);
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<ComputeShaderPass>(json, JsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}