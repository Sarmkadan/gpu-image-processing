// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Represents the result of a single image processing operation
    /// </summary>
    public class ProcessingResult
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid ImageId { get; set; }
        public string InputFilePath { get; set; } = string.Empty;
        public string OutputFilePath { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
        public float ProcessingTimeMs { get; set; }
        public long OutputFileSizeBytes { get; set; }
        public Dictionary<string, object> ProcessingMetrics { get; set; } = new();
        public List<string> AppliedFilters { get; set; } = new();
        public List<string> AppliedTransforms { get; set; } = new();
        public string? WarningMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the ProcessingResult class
        /// </summary>
        public ProcessingResult()
        {
            Id = Guid.NewGuid();
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a successful processing result
        /// </summary>
        public static ProcessingResult CreateSuccess(Guid jobId, Guid imageId, string inputPath, string outputPath)
        {
            return new ProcessingResult
            {
                JobId = jobId,
                ImageId = imageId,
                InputFilePath = inputPath,
                OutputFilePath = outputPath,
                IsSuccessful = true,
                ProcessedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failed processing result with error message
        /// </summary>
        public static ProcessingResult CreateFailure(Guid jobId, Guid imageId, string inputPath, string errorMessage)
        {
            return new ProcessingResult
            {
                JobId = jobId,
                ImageId = imageId,
                InputFilePath = inputPath,
                IsSuccessful = false,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Adds a processing metric to track performance
        /// </summary>
        public void AddMetric(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Metric key cannot be empty", nameof(key));

            ProcessingMetrics[key] = value;
        }

        /// <summary>
        /// Gets a metric value with default fallback
        /// </summary>
        public object? GetMetric(string key, object? defaultValue = null)
        {
            return ProcessingMetrics.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Records a filter that was applied
        /// </summary>
        public void RecordAppliedFilter(string filterName)
        {
            if (!AppliedFilters.Contains(filterName))
                AppliedFilters.Add(filterName);
        }

        /// <summary>
        /// Records a transform that was applied
        /// </summary>
        public void RecordAppliedTransform(string transformName)
        {
            if (!AppliedTransforms.Contains(transformName))
                AppliedTransforms.Add(transformName);
        }

        /// <summary>
        /// Gets a summary of the processing result
        /// </summary>
        public string GetSummary()
        {
            var status = IsSuccessful ? "✓ SUCCESS" : "✗ FAILED";
            var time = $"{ProcessingTimeMs}ms";
            var size = $"{OutputFileSizeBytes} bytes";
            var filters = $"Filters: {AppliedFilters.Count}";
            var transforms = $"Transforms: {AppliedTransforms.Count}";

            return $"{status} | {time} | {size} | {filters} | {transforms}";
        }

        /// <summary>
        /// Gets the ratio of output to input file size
        /// </summary>
        public float GetCompressionRatio(long inputSizeBytes)
        {
            if (inputSizeBytes <= 0)
                return 0f;

            return (float)OutputFileSizeBytes / inputSizeBytes;
        }

        /// <summary>
        /// Adds a warning message without marking the result as failed
        /// </summary>
        public void AddWarning(string warning)
        {
            WarningMessage = string.IsNullOrEmpty(WarningMessage)
                ? warning
                : $"{WarningMessage}\n{warning}";
        }
    }
}
