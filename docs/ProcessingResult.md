# ProcessingResult

A record-like class that encapsulates the outcome of an image processing operation, including timing metrics, applied filters, performance data, and error information when applicable. It serves as the primary payload for tracking and reporting the status of GPU-accelerated image processing tasks.

## API

### Properties

- **`public Guid Id`**
  A unique identifier for the processing result. Generated when the `ProcessingResult` instance is created and immutable thereafter.

- **`public Guid ImageId`**
  The identifier of the image that was processed. Set during construction and immutable thereafter.

- **`public string OutputPath`**
  The filesystem path where the processed image was saved. `null` if processing failed or the output was not persisted.

- **`public ProcessingStatus Status`**
  Indicates the current state of processing. Can be `Pending`, `Completed`, or `Failed`. Defaults to `Pending` on creation.

- **`public DateTime StartedAt`**
  Timestamp when processing began. Set automatically upon construction and immutable thereafter.

- **`public DateTime CompletedAt`**
  Timestamp when processing finished (successfully or otherwise). Set by calling `Complete()` or `Fail()`. Throws `InvalidOperationException` if accessed before completion.

- **`public long ProcessingTimeMilliseconds`**
  Duration of the processing operation in milliseconds. Calculated as `CompletedAt - StartedAt`. Throws `InvalidOperationException` if accessed before completion.

- **`public string? ErrorMessage`**
  Human-readable error message populated when `Fail()` is called. `null` if processing succeeded.

- **`public int ErrorCode`**
  Numeric error code associated with the failure. Defaults to `0` (no error) and is set when `Fail()` is invoked.

- **`public List<FilterApplied> FiltersApplied`**
  Collection of filters applied during processing, in execution order. Populated via `AddFilterApplied()`. Immutable after construction except for additions.

- **`public PerformanceMetrics Metrics`**
  Aggregated performance data for the processing operation, including GPU utilization, memory usage, and kernel execution times. Defaults to an empty instance on creation.

- **`public Dictionary<string, object> ResultMetadata`**
  Arbitrary key-value metadata generated during processing. Can be populated by external systems or filters.

- **`public bool IsSuccessful`**
  Indicates whether processing completed without errors. `true` if `Complete()` was called; otherwise `false`.

### Constructors

- **`public ProcessingResult(Guid imageId)`**
  Initializes a new instance with the specified `imageId`. Sets `Id` to a new `Guid`, `StartedAt` to `DateTime.UtcNow`, `Status` to `Pending`, and initializes empty collections and default values.

### Methods

- **`public void Complete()`**
  Marks the processing operation as completed successfully. Sets `Status` to `Completed`, `CompletedAt` to `DateTime.UtcNow`, and `IsSuccessful` to `true`. Throws `InvalidOperationException` if already completed.

- **`public void Fail(string errorMessage, int errorCode = 0)`**
  Marks the processing operation as failed. Sets `Status` to `Failed`, `CompletedAt` to `DateTime.UtcNow`, `ErrorMessage` to `errorMessage`, `ErrorCode` to `errorCode`, and `IsSuccessful` to `false`. Throws `InvalidOperationException` if already completed.

- **`public void AddFilterApplied(FilterApplied filter)`**
  Adds a filter execution record to `FiltersApplied`. Throws `ArgumentNullException` if `filter` is `null`.

- **`public double GetTotalFilterExecutionTime()`**
  Returns the sum of all filter execution times in milliseconds. Returns `0` if no filters were applied. Throws `InvalidOperationException` if called before completion.

- **`public string FilterName`**
  *(Note: This appears to be incorrectly listed as a member; it is likely a property of `FilterApplied`.)*

- **`public FilterType FilterType`**
  *(Note: This appears to be incorrectly listed as a member; it is likely a property of `FilterApplied`.)*

## Usage
