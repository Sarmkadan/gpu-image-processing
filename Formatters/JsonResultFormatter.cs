#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GpuImageProcessing.Core.Models;

namespace GpuImageProcessing.Formatters
{
    /// <summary>
    /// Formatter for converting processing results to JSON output format.
    /// Supports pretty-printing and configurable serialization options.
    /// </summary>
    public class JsonResultFormatter : IResultFormatter
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public JsonResultFormatter(bool prettyPrint = true)
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = prettyPrint,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
            _serializerOptions.Converters.Add(new DateTimeConverter());
        }

        /// <summary>
        /// Gets the file extension for this format.
        /// </summary>
        public string GetFileExtension()
        {
            return ".json";
        }

        /// <summary>
        /// Gets the MIME type for this format.
        /// </summary>
        public string GetMimeType()
        {
            return "application/json";
        }

        /// <summary>
        /// Formats a processing result to JSON string.
        /// </summary>
        public string FormatResult(ProcessingResult result)
        {
            if (result == null)
                return SerializeObject(new { error = "No result provided" });

            var jsonResult = new
            {
                result.Id,
                result.JobId,
                result.ImageId,
                result.Status,
                result.StartTime,
                result.CompletionTime,
                DurationMs = (result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0,
                result.OutputImagePath,
                result.ProcessedSize,
                result.Metadata
            };

            return SerializeObject(jsonResult);
        }

        /// <summary>
        /// Formats batch processing results to JSON array.
        /// </summary>
        public string FormatResults(List<ProcessingResult> results)
        {
            if (results == null || results.Count == 0)
                return SerializeObject(new { results = new object[0] });

            var jsonResults = new List<object>();

            foreach (var result in results)
            {
                jsonResults.Add(new
                {
                    result.Id,
                    result.JobId,
                    result.ImageId,
                    result.Status,
                    result.StartTime,
                    result.CompletionTime,
                    DurationMs = (result.CompletionTime - result.StartTime)?.TotalMilliseconds ?? 0,
                    result.OutputImagePath,
                    result.ProcessedSize,
                });
            }

            return SerializeObject(new { results = jsonResults, count = jsonResults.Count });
        }

        /// <summary>
        /// Formats processing job information to JSON.
        /// </summary>
        public string FormatJob(ProcessingJob job)
        {
            if (job == null)
                return SerializeObject(new { error = "No job provided" });

            var jsonJob = new
            {
                job.Id,
                job.Name,
                job.Status,
                job.TotalImages,
                job.ProcessedImages,
                job.FailedImages,
                CompletionPercent = (job.ProcessedImages / (double)job.TotalImages) * 100,
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                Filters = job.Filters?.Count ?? 0,
                Transforms = job.Transforms?.Count ?? 0
            };

            return SerializeObject(jsonJob);
        }

        /// <summary>
        /// Formats device information to JSON.
        /// </summary>
        public string FormatDevice(DeviceInfo device)
        {
            if (device == null)
                return SerializeObject(new { error = "No device provided" });

            var jsonDevice = new
            {
                device.Id,
                device.Name,
                device.Type,
                device.Vendor,
                device.MemoryBytes,
                MemoryGb = device.MemoryBytes / (1024.0 * 1024.0 * 1024.0),
                device.ComputeUnits,
                device.IsAvailable,
                device.DriverVersion,
                device.Extensions
            };

            return SerializeObject(jsonDevice);
        }

        /// <summary>
        /// Formats error information to JSON.
        /// </summary>
        public string FormatError(string errorMessage, string errorCode = null, Exception exception = null)
        {
            var jsonError = new
            {
                error = true,
                message = errorMessage,
                code = errorCode ?? "UNKNOWN_ERROR",
                timestamp = DateTime.UtcNow,
                details = exception?.Message,
                stackTrace = exception?.StackTrace
            };

            return SerializeObject(jsonError);
        }

        /// <summary>
        /// Serializes an object to JSON string.
        /// </summary>
        private string SerializeObject(object obj)
        {
            try
            {
                return JsonSerializer.Serialize(obj, _serializerOptions);
            }
            catch (JsonException ex)
            {
                return JsonSerializer.Serialize(new { error = "Serialization failed", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Custom JSON converter for DateTime to ISO 8601 format.
    /// </summary>
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()).ToUniversalTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
        }
    }
}
