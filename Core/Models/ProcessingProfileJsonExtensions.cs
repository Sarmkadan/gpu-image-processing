#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Core.Models
{
	/// <summary>
	/// Provides JSON serialization and deserialization extensions for <see cref="ProcessingProfile"/> instances.
	/// </summary>
	public static class ProcessingProfileJsonExtensions
	{
		private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		};

		/// <summary>
		/// Converts a <see cref="ProcessingProfile"/> instance to its JSON representation.
		/// </summary>
		/// <param name="value">The <see cref="ProcessingProfile"/> instance to serialize.</param>
		/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
		/// <returns>A JSON string representation of the <see cref="ProcessingProfile"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		public static string ToJson(this ProcessingProfile value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
		}

		/// <summary>
		/// Parses a JSON string and creates a <see cref="ProcessingProfile"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <returns>The deserialized <see cref="ProcessingProfile"/> instance if successful; otherwise, <see langword="null"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
		/// <exception cref="JsonException">Thrown when the JSON is invalid and cannot be deserialized.</exception>
		public static ProcessingProfile? FromJson(string json)
		{
			ArgumentException.ThrowIfNullOrEmpty(json);

			return JsonSerializer.Deserialize<ProcessingProfile>(json, _jsonOptions);
		}

		/// <summary>
		/// Attempts to parse a JSON string and create a <see cref="ProcessingProfile"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize.</param>
		/// <param name="value">Receives the deserialized <see cref="ProcessingProfile"/> if successful; otherwise, <see langword="null"/>.</param>
		/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
		public static bool TryFromJson(string json, out ProcessingProfile? value)
		{
			ArgumentException.ThrowIfNullOrEmpty(json);

			try
			{
				value = JsonSerializer.Deserialize<ProcessingProfile>(json, _jsonOptions);
				return true;
			}
			catch (JsonException)
			{
				value = null;
				return false;
			}
		}
	}
}