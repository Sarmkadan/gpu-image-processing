// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Base class for all processing events in the system.
    /// Provides common event metadata and tracking.
    /// </summary>
    public abstract class ProcessingEvent
    {
        /// <summary>
        /// Gets unique event identifier.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets the event type name.
        /// </summary>
        public abstract string EventType { get; }

        /// <summary>
        /// Gets timestamp when event was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets associated operation ID for tracking.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets additional event metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        protected ProcessingEvent()
        {
            EventId = Guid.NewGuid().ToString("N");
            Timestamp = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Event fired when image processing job starts.
    /// </summary>
    public class JobStartedEvent : ProcessingEvent
    {
        public override string EventType => "job.started";

        public string JobId { get; set; }
        public string JobName { get; set; }
        public int TotalImages { get; set; }
        public List<string> FilterIds { get; set; }
        public List<string> TransformIds { get; set; }
    }

    /// <summary>
    /// Event fired when image processing job completes.
    /// </summary>
    public class JobCompletedEvent : ProcessingEvent
    {
        public override string EventType => "job.completed";

        public string JobId { get; set; }
        public int ProcessedImages { get; set; }
        public int FailedImages { get; set; }
        public long DurationMs { get; set; }
        public bool Success { get; set; }
    }

    /// <summary>
    /// Event fired when image processing fails.
    /// </summary>
    public class JobFailedEvent : ProcessingEvent
    {
        public override string EventType => "job.failed";

        public string JobId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public int AttemptNumber { get; set; }
        public bool Retryable { get; set; }
    }

    /// <summary>
    /// Event fired when individual image processing starts.
    /// </summary>
    public class ImageProcessingStartedEvent : ProcessingEvent
    {
        public override string EventType => "image.processing.started";

        public string ImageId { get; set; }
        public string ImagePath { get; set; }
        public string FilterName { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }

    /// <summary>
    /// Event fired when individual image processing completes.
    /// </summary>
    public class ImageProcessingCompletedEvent : ProcessingEvent
    {
        public override string EventType => "image.processing.completed";

        public string ImageId { get; set; }
        public string OutputPath { get; set; }
        public long ProcessedSize { get; set; }
        public long DurationMs { get; set; }
        public Dictionary<string, object> ProcessingMetrics { get; set; }
    }

    /// <summary>
    /// Event fired when image processing encounters an error.
    /// </summary>
    public class ImageProcessingFailedEvent : ProcessingEvent
    {
        public override string EventType => "image.processing.failed";

        public string ImageId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public long AttemptDurationMs { get; set; }
    }

    /// <summary>
    /// Event fired when device becomes available or unavailable.
    /// </summary>
    public class DeviceStatusChangedEvent : ProcessingEvent
    {
        public override string EventType => "device.status.changed";

        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public bool IsAvailable { get; set; }
        public string PreviousStatus { get; set; }
        public string CurrentStatus { get; set; }
    }

    /// <summary>
    /// Event fired when system performance degrades.
    /// </summary>
    public class PerformanceDegradedEvent : ProcessingEvent
    {
        public override string EventType => "performance.degraded";

        public string Reason { get; set; }
        public double CurrentThroughput { get; set; }
        public double ExpectedThroughput { get; set; }
        public Dictionary<string, object> Metrics { get; set; }
    }

    /// <summary>
    /// Event fired when resource limit is reached.
    /// </summary>
    public class ResourceLimitReachedEvent : ProcessingEvent
    {
        public override string EventType => "resource.limit.reached";

        public string ResourceType { get; set; }
        public long CurrentUsage { get; set; }
        public long MaxLimit { get; set; }
        public double UtilizationPercent { get; set; }
    }

    /// <summary>
    /// Event fired when cache operation occurs.
    /// </summary>
    public class CacheOperationEvent : ProcessingEvent
    {
        public override string EventType => "cache.operation";

        public string Operation { get; set; } // "hit", "miss", "evict", "clear"
        public string CacheKey { get; set; }
        public long CacheSize { get; set; }
        public double HitRate { get; set; }
    }

    /// <summary>
    /// Event fired for batch operation progress updates.
    /// </summary>
    public class BatchProgressEvent : ProcessingEvent
    {
        public override string EventType => "batch.progress";

        public string BatchId { get; set; }
        public int ProcessedCount { get; set; }
        public int TotalCount { get; set; }
        public double ProgressPercent { get; set; }
        public long EstimatedRemainingMs { get; set; }
    }

    /// <summary>
    /// Event fired when system health check occurs.
    /// </summary>
    public class HealthCheckEvent : ProcessingEvent
    {
        public override string EventType => "health.check";

        public bool IsHealthy { get; set; }
        public string HealthStatus { get; set; }
        public Dictionary<string, bool> ComponentHealth { get; set; }
        public List<string> Issues { get; set; }
    }
}
