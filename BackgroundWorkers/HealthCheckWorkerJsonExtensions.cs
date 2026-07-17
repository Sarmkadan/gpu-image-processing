#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.BackgroundWorkers
{
	/// <summary>
	/// Provides JSON (de)serialization helpers for <see cref="HealthCheckWorker"/>.
	/// </summary>
	public static class HealthCheckWorkerJsonExtensions
	{
		private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		/// <summary>
		/// Serializes the <see cref="HealthCheckWorker"/> instance to a JSON string.
		/// </summary>
		/// <param name="value">The worker instance to serialize.</param>
		/// <param name="indented">If <see langword="true"/>, the output JSON will be indented.</param>
		/// <returns>A JSON representation of <paramref name="value"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
		public static string ToJson(this HealthCheckWorker value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);
			var opts = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
			return JsonSerializer.Serialize(value, opts);
		}

		/// <summary>
		/// Deserializes a JSON string into a <see cref="HealthCheckWorker"/> instance.
		/// </summary>
		/// <param name="json">The JSON representation of a <see cref="HealthCheckWorker"/>.</param>
		/// <returns>The deserialized <see cref="HealthCheckWorker"/>, or <see langword="null"/> if the JSON does not contain a valid object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of whitespace.</exception>
		/// <exception cref="JsonException">The JSON is malformed or cannot be deserialized.</exception>
		public static HealthCheckWorker? FromJson(string json)
		{
			ArgumentNullException.ThrowIfNull(json);
			ArgumentException.ThrowIfNullOrWhiteSpace(json);
			return JsonSerializer.Deserialize<HealthCheckWorker>(json, _options);
		}

		/// <summary>
		/// Attempts to deserialize a JSON string into a <see cref="HealthCheckWorker"/> instance.
		/// </summary>
		/// <param name="json">The JSON representation of a <see cref="HealthCheckWorker"/>.</param>
		/// <param name="value">When this method returns, contains the deserialized instance if the operation succeeded; otherwise, <see langword="null"/>.</param>
		/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of whitespace.</exception>
		public static bool TryFromJson(string json, out HealthCheckWorker? value)
		{
			ArgumentNullException.ThrowIfNull(json);
			ArgumentException.ThrowIfNullOrWhiteSpace(json);
			try
			{
				value = JsonSerializer.Deserialize<HealthCheckWorker>(json, _options);
				return value is not null;
			}
			catch (JsonException)
			{
				value = null;
				return false;
			}
		}
	}
}
