#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using GpuImageProcessing.Core.Constants;

namespace GpuImageProcessing.Core.Models
{
    /// <summary>
    /// Represents a batch processing job that applies multiple filters and transforms to images
    /// </summary>
    public class ProcessingJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
        public List<Guid> ImageIds { get; set; } = new();
        public List<Guid> FilterIds { get; set; } = new();
        public List<Guid> TransformIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public float ProgressPercentage { get; set; }
        public int TotalImages { get; set; }
        public int ProcessedImages { get; set; }
        public int FailedImages { get; set; }
        public string OutputDirectory { get; set; } = string.Empty;
        public Dictionary<string, string> JobMetadata { get; set; } = new();
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the ProcessingJob class
        /// </summary>
        public ProcessingJob()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Starts the processing job by updating timestamps and status
        /// </summary>
        public void Start()
        {
            if (Status != ProcessingStatus.Pending)
                throw new InvalidOperationException("Can only start jobs in Pending status");

            Status = ProcessingStatus.Running;
            StartedAt = DateTime.UtcNow;
            ProgressPercentage = 0f;
        }

        /// <summary>
        /// Completes the processing job successfully
        /// </summary>
        public void Complete()
        {
            if (Status != ProcessingStatus.Running)
                throw new InvalidOperationException("Can only complete jobs in Running status");

            Status = ProcessingStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            ProgressPercentage = 100f;
        }

        /// <summary>
        /// Marks the job as failed with an error message
        /// </summary>
        public void MarkFailed(string errorMessage)
        {
            Status = ProcessingStatus.Failed;
            ErrorMessage = errorMessage;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the processing progress
        /// </summary>
        public void UpdateProgress(int imagesProcessed, int imagesFailed)
        {
            ProcessedImages = imagesProcessed;
            FailedImages = imagesFailed;

            if (TotalImages > 0)
            {
                ProgressPercentage = ((float)(imagesProcessed + imagesFailed) / TotalImages) * 100f;
            }
        }

        /// <summary>
        /// Cancels the processing job
        /// </summary>
        public void Cancel()
        {
            if (Status == ProcessingStatus.Completed || Status == ProcessingStatus.Failed)
                throw new InvalidOperationException("Cannot cancel completed or failed jobs");

            Status = ProcessingStatus.Cancelled;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the elapsed time since job creation or start
        /// </summary>
        public TimeSpan GetElapsedTime()
        {
            var endTime = CompletedAt ?? DateTime.UtcNow;
            var startTime = StartedAt ?? CreatedAt;
            return endTime - startTime;
        }

        /// <summary>
        /// Calculates the processing rate (images per second)
        /// </summary>
        public double GetProcessingRate()
        {
            var elapsed = GetElapsedTime().TotalSeconds;
            if (elapsed <= 0)
                return 0;

            return ProcessedImages / elapsed;
        }

        /// <summary>
        /// Estimates the remaining time based on current processing rate
        /// </summary>
        public TimeSpan EstimateRemainingTime()
        {
            var rate = GetProcessingRate();
            if (rate <= 0)
                return TimeSpan.Zero;

            var remaining = TotalImages - ProcessedImages;
            var secondsRemaining = remaining / rate;
            return TimeSpan.FromSeconds(secondsRemaining);
        }

        /// <summary>
        /// Adds metadata to the job
        /// </summary>
        public void AddMetadata(string key, string value)
        {
            JobMetadata[key] = value;
        }

        /// <summary>
        /// Gets whether the job is still running
        /// </summary>
        public bool IsRunning => Status == ProcessingStatus.Running;

        /// <summary>
        /// Gets the total number of operations (filters + transforms)
        /// </summary>
        public int GetTotalOperations()
        {
            return FilterIds.Count + TransformIds.Count;
        }
    }
}
