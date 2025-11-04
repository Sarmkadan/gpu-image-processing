using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GpuImageProcessing.Middleware
{
    public static class RateLimitingMiddlewareJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string ToJson(this RateLimitingMiddleware value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static RateLimitingMiddleware? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                return JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out RateLimitingMiddleware? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                value = JsonSerializer.Deserialize<RateLimitingMiddleware>(json, _jsonSerializerOptions);
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
