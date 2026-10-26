# DomainEvents

`Events/DomainEvents.cs` defines the strongly-typed domain events raised across the image processing lifecycle: image registration, filter and transform application, batch job progress, processing errors, performance metrics, GPU device status, cache hits and memory warnings. Each event derives from the abstract `DomainEvent` base class (defined in `Events/EventAggregator.cs`) and is intended to be published through an `EventAggregator`, so producers and consumers stay decoupled.

The file contains only data-carrying types: there are no constructors beyond the implicit parameterless ones, no methods other than the `ToString` override on `ImageRegisteredEvent`, and no validation logic.

## API

### Inherited from `DomainEvent`

Every event below inherits these members. The base constructor assigns `Id` and `OccurredAt` automatically.

| Member | Type | Purpose |
|--------|------|---------|
| `Id` | `Guid` | Unique identifier of the event instance, generated with `Guid.NewGuid()` on construction. |
| `OccurredAt` | `DateTime` | Creation time in UTC, set to `DateTime.UtcNow` on construction. |
| `Source` | `string` | Optional name of the component that raised the event. Not set by the constructor. |

### `ImageRegisteredEvent`
Raised when an image has been registered.

| Property | Type | Purpose |
|----------|------|---------|
| `ImageId` | `Guid` | Identifier of the registered image. |
| `ImagePath` | `string` | Path of the image source. |
| `Width` | `int` | Image width in pixels. |
| `Height` | `int` | Image height in pixels. |
| `Description` | `string` | Optional human-readable description. |

Overrides `ToString()` to produce a diagnostic string listing all five properties.

### `FilterAppliedEvent`
Raised after a filter is applied to an image.

| Property | Type | Purpose |
|----------|------|---------|
| `ImageId` | `Guid` | Identifier of the processed image. |
| `FilterId` | `Guid` | Identifier of the filter. |
| `FilterName` | `string` | Name of the filter. |
| `DurationMilliseconds` | `double` | Time taken by the operation. |
| `Success` | `bool` | Whether the filter completed without error. |
| `ErrorMessage` | `string` | Error details when `Success` is `false`. |

### `TransformAppliedEvent`
Raised after a transform is applied to an image.

| Property | Type | Purpose |
|----------|------|---------|
| `ImageId` | `Guid` | Identifier of the processed image. |
| `TransformId` | `Guid` | Identifier of the transform. |
| `TransformName` | `string` | Name of the transform. |
| `Parameters` | `Dictionary<string, object>` | Parameters the transform was applied with. |
| `DurationMilliseconds` | `double` | Time taken by the operation. |
| `Success` | `bool` | Whether the transform completed without error. |

### `BatchJobCreatedEvent`
Raised when a batch job is created.

| Property | Type | Purpose |
|----------|------|---------|
| `JobId` | `Guid` | Identifier of the job. |
| `JobName` | `string` | Name of the job. |
| `TotalImages` | `int` | Number of images in the job. |
| `ImageIds` | `List<Guid>` | Identifiers of the images in the job. |
| `FilterIds` | `List<Guid>` | Identifiers of the filters to apply. |

### `BatchJobStartedEvent`
Raised when a batch job begins processing.

| Property | Type | Purpose |
|----------|------|---------|
| `JobId` | `Guid` | Identifier of the job. |
| `StartTime` | `DateTime` | When processing started. |
| `TotalImages` | `int` | Number of images to process. |

### `BatchJobProgressEvent`
Raised periodically while a batch job runs.

| Property | Type | Purpose |
|----------|------|---------|
| `JobId` | `Guid` | Identifier of the job. |
| `ProcessedCount` | `int` | Images processed so far. |
| `TotalCount` | `int` | Total images in the job. |
| `PercentComplete` | `double` | Completion percentage. |
| `ElapsedTime` | `TimeSpan` | Time elapsed since the job started. |
| `EstimatedTimeRemaining` | `TimeSpan?` | Estimated remaining time; `null` when it cannot be estimated. |

### `BatchJobCompletedEvent`
Raised when a batch job finishes.

| Property | Type | Purpose |
|----------|------|---------|
| `JobId` | `Guid` | Identifier of the job. |
| `SuccessCount` | `int` | Images processed successfully. |
| `FailureCount` | `int` | Images that failed. |
| `TotalDuration` | `TimeSpan` | Total job duration. |
| `SuccessRate` | `double` | Fraction of successful images. |

### `BatchJobFailedEvent`
Raised when a batch job fails.

| Property | Type | Purpose |
|----------|------|---------|
| `JobId` | `Guid` | Identifier of the job. |
| `ErrorMessage` | `string` | Description of the failure. |
| `ProcessedCount` | `int` | Images processed before the failure. |
| `TotalCount` | `int` | Total images in the job. |

### `ProcessingErrorEvent`
Raised when an operation on an image fails.

| Property | Type | Purpose |
|----------|------|---------|
| `ImageId` | `Guid` | Identifier of the affected image. |
| `OperationName` | `string` | Name of the operation that failed. |
| `ErrorMessage` | `string` | Error description. |
| `StackTrace` | `string` | Stack trace of the error. |
| `Severity` | `ErrorSeverity` | Severity of the error. |

### `PerformanceMetricsEvent`
Carries a snapshot of resource utilization and throughput.

| Property | Type | Purpose |
|----------|------|---------|
| `ResourceId` | `Guid` | Identifier of the measured resource. |
| `ResourceType` | `string` | Kind of resource, such as a device or pipeline. |
| `CpuUsagePercent` | `float` | CPU utilization percentage. |
| `GpuUsagePercent` | `float` | GPU utilization percentage. |
| `MemoryUsagePercent` | `float` | Memory utilization percentage. |
| `ThroughputItemsPerSecond` | `double` | Items processed per second. |
| `AverageLatencyMilliseconds` | `double` | Average latency per item, in milliseconds. |

### `GpuDeviceStatusChangedEvent`
Raised when a GPU device changes status.

| Property | Type | Purpose |
|----------|------|---------|
| `DeviceId` | `Guid` | Identifier of the device. |
| `DeviceName` | `string` | Name of the device. |
| `OldStatus` | `DeviceStatus` | Previous status. |
| `NewStatus` | `DeviceStatus` | Current status. |
| `Reason` | `string` | Explanation for the change. |

### `CacheHitEvent`
Raised when a processing result is served from cache.

| Property | Type | Purpose |
|----------|------|---------|
| `ResourceId` | `Guid` | Identifier of the cached resource. |
| `CacheKey` | `string` | Key that was looked up. |
| `CacheSizeBytes` | `long` | Size of the cached entry in bytes. |
| `TimeSavedMilliseconds` | `TimeSpan` | Time saved by avoiding recomputation. |

Despite its name, `TimeSavedMilliseconds` is typed as `TimeSpan`, not as a number of milliseconds.

### `ResourceMemoryWarningEvent`
Raised when a resource's memory usage is high.

| Property | Type | Purpose |
|----------|------|---------|
| `ResourceId` | `Guid` | Identifier of the resource. |
| `ResourceType` | `string` | Kind of resource. |
| `TotalMemoryBytes` | `long` | Total memory available, in bytes. |
| `UsedMemoryBytes` | `long` | Memory in use, in bytes. |
| `UsagePercent` | `float` | Memory utilization percentage. |

### `ErrorSeverity` (enum)
Severity levels for `ProcessingErrorEvent.Severity`: `Info`, `Warning`, `Error`, `Critical`.

### `DeviceStatus` (enum)
Device states used by `GpuDeviceStatusChangedEvent`: `Available`, `Busy`, `Idle`, `Offline`, `Maintenance`.

## Usage

```csharp
// Example 1: Publishing a filter result through the event aggregator
var aggregator = new EventAggregator();

aggregator.Subscribe<FilterAppliedEvent>(domainEvent =>
{
    var e = (FilterAppliedEvent)domainEvent;
    if (!e.Success)
        Console.Error.WriteLine($"Filter {e.FilterName} failed on {e.ImageId}: {e.ErrorMessage}");
});

aggregator.Publish(new FilterAppliedEvent
{
    ImageId = imageId,
    FilterId = filterId,
    FilterName = "GaussianBlur",
    DurationMilliseconds = 12.5,
    Success = true,
    Source = "FilterService"
});
```

```csharp
// Example 2: Reporting batch progress from a job
var progress = new BatchJobProgressEvent
{
    JobId = jobId,
    ProcessedCount = 40,
    TotalCount = 200,
    PercentComplete = 20.0,
    ElapsedTime = TimeSpan.FromSeconds(30),
    EstimatedTimeRemaining = TimeSpan.FromSeconds(120)
};

await aggregator.PublishAsync(progress);
```

## Notes

- Every event has a unique `Id` and a UTC `OccurredAt` timestamp set at construction, so events can be correlated and ordered without extra bookkeeping.
- The types have public get/set properties and no validation. Producers are responsible for consistent values, such as setting `ErrorMessage` only when `Success` is `false`.
- `EstimatedTimeRemaining` is nullable. Consumers must check for `null` before using it.
- `ProcessingErrorEvent.StackTrace` may contain sensitive internal details. Avoid exposing it to end users.
