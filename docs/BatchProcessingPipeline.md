# BatchProcessingPipeline

The `BatchProcessingPipeline` class provides a configurable mechanism for processing a batch of images with support for concurrent execution, automatic retries on failure, and exponential backoff. It is designed to orchestrate the processing of multiple images, collect per-image outcomes, and produce a consolidated result containing aggregate statistics and detailed outcome records.

## API

### BatchProcessingPipeline

- **`public BatchProcessingPipeline()`**  
  Initializes a new instance of the pipeline with default values for concurrency, retry count, and retry delay.

- **`public int MaxConcurrency { get; set; }`**  
  Gets or sets the maximum number of images processed concurrently. Higher values increase parallelism but may consume more system resources.

- **`public int MaxRetries { get; set; }`**  
  Gets or sets the maximum number of retry attempts for a single image when processing fails. A value of zero means no retries.

- **`public int RetryBaseDelayMs { get; set; }`**  
  Gets or sets the base delay in milliseconds used for exponential backoff between retries. The actual delay is calculated as `RetryBaseDelayMs * (2 ^ attempt)`.

- **`public async Task<BatchPipelineResult> RunAsync()`**  
  Executes the pipeline asynchronously. Processes all images in the batch, applying the configured concurrency and retry settings.  
  **Returns:** A `BatchPipelineResult` containing aggregate statistics and per-image outcomes.  
  **Throws:** `InvalidOperationException` if the pipeline is already running. `ObjectDisposedException` if the pipeline has been disposed.

### BatchPipelineResult

The result returned by `RunAsync`. Contains overall batch statistics and a list of individual image outcomes.

- **`public Guid BatchId`** – Unique identifier for the batch execution.
- **`public string BatchName`** – Human-readable name assigned to the batch.
- **`public int TotalImages`** – Total number of images submitted for processing.
- **`public int SucceededCount`** – Number of images that completed successfully.
- **`public int FailedCount`** – Number of images that failed after all retries.
- **`public TimeSpan TotalDuration`** – Wall-clock time taken to process the entire batch.
- **`public double AverageProcessingMs`** – Average processing time per image in milliseconds (excluding retry delays).
- **`public IReadOnlyList<ImagePipelineOutcome> Outcomes`** – Collection of per-image outcome records.
- **`public DateTime CompletedAt`** – Timestamp when the batch finished processing.
- **`public double ProgressPercent`** – Overall progress as a percentage (0.0 to 100.0) at the time the result was produced.
- **`public ImagePipelineOutcome Outcome`** – A summary outcome representing the overall batch result (e.g., success or failure).

### ImagePipelineOutcome

- **`public sealed record ImagePipelineOutcome`**  
  A sealed record type that encapsulates the result of processing a single image. The record’s properties are not listed here but are accessible through the `Outcomes` collection of `BatchPipelineResult`.

## Usage

### Example 1: Basic pipeline with default settings

```csharp
using GpuImageProcessing;

var pipeline = new BatchProcessingPipeline();
// Assume images are loaded into a list or provided via some source
// pipeline.RunAsync() processes the batch internally

BatchPipelineResult result = await pipeline.RunAsync();

Console.WriteLine($"Batch {result.BatchName} completed in {result.TotalDuration.TotalSeconds:F2}s");
Console.WriteLine($"Succeeded: {result.SucceededCount}, Failed: {result.FailedCount}");
```

### Example 2: Custom concurrency and retry configuration

```csharp
using GpuImageProcessing;

var pipeline = new BatchProcessingPipeline
{
    MaxConcurrency = 4,
    MaxRetries = 3,
    RetryBaseDelayMs = 200
};

BatchPipelineResult result = await pipeline.RunAsync();

foreach (var outcome in result.Outcomes)
{
    // Process each image outcome (properties depend on the record definition)
    // e.g., outcome.ImageId, outcome.Status, outcome.DurationMs
}

Console.WriteLine($"Average processing time: {result.AverageProcessingMs:F1} ms");
```

## Notes

- **Thread safety:** The `BatchProcessingPipeline` instance is not thread-safe for concurrent modifications to its properties while `RunAsync` is executing. Do not change `MaxConcurrency`, `MaxRetries`, or `RetryBaseDelayMs` after calling `RunAsync` until the task completes.
- **Reentrancy:** Calling `RunAsync` while a previous invocation is still in progress will throw an `InvalidOperationException`. Ensure only one execution is active at a time.
- **Empty batch:** If no images are provided, `RunAsync` returns a `BatchPipelineResult` with `TotalImages = 0`, `SucceededCount = 0`, `FailedCount = 0`, and an empty `Outcomes` list. `TotalDuration` will be near zero.
- **Retry behavior:** Retries use exponential backoff based on `RetryBaseDelayMs`. If all retries are exhausted, the image is marked as failed and the error is recorded in the corresponding `ImagePipelineOutcome`.
- **Disposal:** The pipeline implements `IDisposable` (not shown in the public API list). After disposal, calling `RunAsync` throws `ObjectDisposedException`.
