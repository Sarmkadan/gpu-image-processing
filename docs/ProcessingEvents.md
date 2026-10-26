# ProcessingEvents

`Events/ProcessingEvents.cs` (namespace `GpuImageProcessing.Events`) defines the event types that the application shell publishes while jobs, images, devices, caches and batches are running. `ProcessingEvent` is the abstract base class; each concrete event exposes a stable `EventType` string (for example `job.completed`) that consumers can use for routing, logging or telemetry. Events are plain, mutable data objects: they carry no behavior beyond the constructor that stamps the metadata.

## API

### `public abstract class ProcessingEvent`
Base class for all processing events. The protected constructor sets `EventId` to a new GUID (`"N"` format), `Timestamp` to `DateTime.UtcNow`, and `Metadata` to an empty dictionary.

- `public string EventId { get; set; }`: Unique event identifier.
- `public abstract string EventType { get; }`: Event type name, implemented by each derived class.
- `public DateTime Timestamp { get; set; }`: UTC time the event was created.
- `public string OperationId { get; set; }`: Identifier of the operation the event belongs to, for tracking.
- `public Dictionary<string, object> Metadata { get; set; }`: Additional free-form metadata.

### Job events

#### `public class JobStartedEvent : ProcessingEvent`
`EventType`: `job.started`. Raised when an image processing job starts.
- `string JobId`, `string JobName`
- `int TotalImages`
- `List<string> FilterIds`, `List<string> TransformIds`

#### `public class JobCompletedEvent : ProcessingEvent`
`EventType`: `job.completed`. Raised when a job completes.
- `string JobId`
- `int ProcessedImages`, `int FailedImages`
- `long DurationMs`
- `bool Success`

#### `public class JobFailedEvent : ProcessingEvent`
`EventType`: `job.failed`. Raised when a job fails.
- `string JobId`
- `string ErrorMessage`, `string ErrorCode`
- `int AttemptNumber`
- `bool Retryable`

### Image events

#### `public class ImageProcessingStartedEvent : ProcessingEvent`
`EventType`: `image.processing.started`. Raised when processing of a single image starts.
- `string ImageId`, `string ImagePath`, `string FilterName`
- `int ImageWidth`, `int ImageHeight`

#### `public class ImageProcessingCompletedEvent : ProcessingEvent`
`EventType`: `image.processing.completed`. Raised when processing of a single image completes.
- `string ImageId`, `string OutputPath`
- `long ProcessedSize`, `long DurationMs`
- `Dictionary<string, object> ProcessingMetrics`

#### `public class ImageProcessingFailedEvent : ProcessingEvent`
`EventType`: `image.processing.failed`. Raised when processing of a single image fails.
- `string ImageId`
- `string ErrorMessage`, `string ErrorCode`
- `long AttemptDurationMs`

### Device, performance and resource events

#### `public class DeviceStatusChangedEvent : ProcessingEvent`
`EventType`: `device.status.changed`. Raised when a device becomes available or unavailable.
- `int DeviceId`, `string DeviceName`
- `bool IsAvailable`
- `string PreviousStatus`, `string CurrentStatus`

#### `public class PerformanceDegradedEvent : ProcessingEvent`
`EventType`: `performance.degraded`. Raised when system performance degrades.
- `string Reason`
- `double CurrentThroughput`, `double ExpectedThroughput`
- `Dictionary<string, object> Metrics`

#### `public class ResourceLimitReachedEvent : ProcessingEvent`
`EventType`: `resource.limit.reached`. Raised when a resource limit is reached.
- `string ResourceType`
- `long CurrentUsage`, `long MaxLimit`
- `double UtilizationPercent`

#### `public class CacheOperationEvent : ProcessingEvent`
`EventType`: `cache.operation`. Raised on a cache operation.
- `string Operation`: One of `"hit"`, `"miss"`, `"evict"` or `"clear"` (by convention; the type does not enforce the values).
- `string CacheKey`, `long CacheSize`
- `double HitRate`

### Batch and health events

#### `public class BatchProgressEvent : ProcessingEvent`
`EventType`: `batch.progress`. Raised for batch progress updates.
- `string BatchId`
- `int ProcessedCount`, `int TotalCount`
- `double ProgressPercent`
- `long EstimatedRemainingMs`

#### `public class HealthCheckEvent : ProcessingEvent`
`EventType`: `health.check`. Raised when a system health check runs.
- `bool IsHealthy`, `string HealthStatus`
- `Dictionary<string, bool> ComponentHealth`
- `List<string> Issues`

## Usage

```csharp
using GpuImageProcessing.Events;

// Publish-side: build a completed-job event
var completed = new JobCompletedEvent
{
    OperationId = "batch-2026-10-26",
    JobId = "job-42",
    ProcessedImages = 98,
    FailedImages = 2,
    DurationMs = 15230,
    Success = true
};

// Consumer-side: branch on the stable EventType string or on the concrete type
void HandleEvent(ProcessingEvent @event)
{
    switch (@event)
    {
        case JobCompletedEvent done:
            Console.WriteLine($"Job {done.JobId}: {done.ProcessedImages} ok, {done.FailedImages} failed in {done.DurationMs} ms");
            break;
        case JobFailedEvent failed when failed.Retryable:
            Console.WriteLine($"Job {failed.JobId} failed ({failed.ErrorCode}), attempt {failed.AttemptNumber}; will retry");
            break;
        default:
            Console.WriteLine($"Unhandled event {@event.EventType} ({@event.EventId})");
            break;
    }
}

HandleEvent(completed);
```

## Notes

- `EventId`, `Timestamp` and `Metadata` are initialized by the base constructor; set `OperationId` explicitly to correlate events that belong to the same operation.
- `EventType` values are strings intended for routing and logs. Prefer matching on the concrete type in code, since the string is not compile-checked.
- Properties are settable, so an event can be mutated after it is published. Treat published events as immutable by convention.
- Event objects may be created and published from multiple threads; the types themselves do not synchronize access.
- Each derived type declares its own `EventType` as a constant expression, so it is the same for every instance of that type.
