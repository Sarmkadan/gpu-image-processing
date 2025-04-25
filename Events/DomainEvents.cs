// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Domain events for image processing lifecycle events.
    /// Published throughout the application for decoupled event handling.
    /// </summary>

    public class ImageRegisteredEvent : ProcessingEvent
    {
        public Guid ImageId { get; set; }
        public string ImagePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Description { get; set; }
    }

    public class FilterAppliedEvent : ProcessingEvent
    {
        public Guid ImageId { get; set; }
        public Guid FilterId { get; set; }
        public string FilterName { get; set; }
        public double DurationMilliseconds { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class TransformAppliedEvent : ProcessingEvent
    {
        public Guid ImageId { get; set; }
        public Guid TransformId { get; set; }
        public string TransformName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public double DurationMilliseconds { get; set; }
        public bool Success { get; set; }
    }

    public class BatchJobCreatedEvent : ProcessingEvent
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public int TotalImages { get; set; }
        public List<Guid> ImageIds { get; set; }
        public List<Guid> FilterIds { get; set; }
    }

    public class BatchJobStartedEvent : ProcessingEvent
    {
        public Guid JobId { get; set; }
        public DateTime StartTime { get; set; }
        public int TotalImages { get; set; }
    }

    public class BatchJobProgressEvent : ProcessingEvent
    {
        public Guid JobId { get; set; }
        public int ProcessedCount { get; set; }
        public int TotalCount { get; set; }
        public double PercentComplete { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }

    public class BatchJobCompletedEvent : ProcessingEvent
    {
        public Guid JobId { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double SuccessRate { get; set; }
    }

    public class BatchJobFailedEvent : ProcessingEvent
    {
        public Guid JobId { get; set; }
        public string ErrorMessage { get; set; }
        public int ProcessedCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class ProcessingErrorEvent : ProcessingEvent
    {
        public Guid ImageId { get; set; }
        public string OperationName { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public ErrorSeverity Severity { get; set; }
    }

    public class PerformanceMetricsEvent : ProcessingEvent
    {
        public Guid ResourceId { get; set; }
        public string ResourceType { get; set; }
        public float CpuUsagePercent { get; set; }
        public float GpuUsagePercent { get; set; }
        public float MemoryUsagePercent { get; set; }
        public double ThroughputItemsPerSecond { get; set; }
        public double AverageLatencyMilliseconds { get; set; }
    }

    public class DeviceStatusChangedEvent : ProcessingEvent
    {
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; }
        public DeviceStatus OldStatus { get; set; }
        public DeviceStatus NewStatus { get; set; }
        public string Reason { get; set; }
    }

    public class CacheHitEvent : ProcessingEvent
    {
        public Guid ResourceId { get; set; }
        public string CacheKey { get; set; }
        public long CacheSizeBytes { get; set; }
        public TimeSpan TimeSavedMilliseconds { get; set; }
    }

    public class ResourceMemoryWarningEvent : ProcessingEvent
    {
        public Guid ResourceId { get; set; }
        public string ResourceType { get; set; }
        public long TotalMemoryBytes { get; set; }
        public long UsedMemoryBytes { get; set; }
        public float UsagePercent { get; set; }
    }

    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public enum DeviceStatus
    {
        Available,
        Busy,
        Idle,
        Offline,
        Maintenance
    }
}
