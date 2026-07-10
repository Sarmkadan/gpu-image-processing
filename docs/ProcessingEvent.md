# ProcessingEvent
The `ProcessingEvent` type represents an event that occurs during the processing of images in the gpu-image-processing project. It provides information about the event, such as its type, timestamp, and associated metadata, as well as details about the processing job, including the number of images processed, failed, and the duration of the processing.

## API
* `EventId`: A unique identifier for the event.
* `EventType`: The type of event that occurred (abstract, must be implemented by derived classes).
* `Timestamp`: The date and time when the event occurred.
* `OperationId`: The identifier of the operation that triggered the event.
* `Metadata`: A dictionary of additional metadata associated with the event.
* `JobId`: The identifier of the job that the event belongs to.
* `JobName`: The name of the job that the event belongs to.
* `TotalImages`: The total number of images being processed in the job.
* `FilterIds`: A list of identifiers of filters applied during the processing.
* `TransformIds`: A list of identifiers of transformations applied during the processing.
* `ProcessedImages`: The number of images that have been successfully processed.
* `FailedImages`: The number of images that failed processing.
* `DurationMs`: The duration of the processing in milliseconds.
* `Success`: A boolean indicating whether the processing was successful.
* `ErrorMessage`: An error message if the processing failed.
* `ErrorCode`: An error code if the processing failed.
* `AttemptNumber`: The number of attempts made to process the images.
* `Retryable`: A boolean indicating whether the processing can be retried.

## Usage
```csharp
// Example 1: Creating a new ProcessingEvent
var @event = new MyProcessingEvent {
    EventId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    OperationId = "MyOperation",
    Metadata = new Dictionary<string, object> { { "MyKey", "MyValue" } },
    JobId = "MyJob",
    JobName = "My Job",
    TotalImages = 10,
    FilterIds = new List<string> { "Filter1", "Filter2" },
    TransformIds = new List<string> { "Transform1" },
    ProcessedImages = 5,
    FailedImages = 2,
    DurationMs = 1000,
    Success = true
};

// Example 2: Handling a ProcessingEvent
void HandleEvent(ProcessingEvent @event) {
    if (@event.Success) {
        Console.WriteLine($"Processing successful: { @event.ProcessedImages } images processed in { @event.DurationMs }ms");
    } else {
        Console.WriteLine($"Processing failed: { @event.ErrorMessage } (Error code: { @event.ErrorCode })");
    }
}
```

## Notes
The `ProcessingEvent` type is designed to be inherited from, with the `EventType` property being abstract. This allows for different types of events to be created, each with their own specific properties and behavior. When working with `ProcessingEvent` instances, it is essential to consider thread-safety, as the events may be triggered from multiple threads. Additionally, the `Retryable` property should be carefully evaluated, as it may impact the overall processing workflow. The `Metadata` dictionary can be used to store additional information about the event, but its contents should be carefully managed to avoid performance issues.
