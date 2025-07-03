# BatchProcessingService

The `BatchProcessingService` class provides a centralized mechanism for managing asynchronous image processing workloads as discrete batches. It supports creating new batches, tracking their progress, retrieving status and progress information, cancelling in-flight operations, and enumerating currently active batches. The service is designed for concurrent use and maintains internal state to coordinate multiple batch operations without external synchronization.

## API

### `public BatchProcessingService()`
Initializes a new instance of the `BatchProcessingService` class.  
No parameters.  
No return value.  
Does not throw.

### `public async Task<ImageBatch> ProcessBatchAsync(string batchId)`
Starts or resumes processing of an existing batch identified by `batchId`.  
**Parameters:**  
- `batchId` – A string that uniquely identifies the batch to process.  

**Returns:** A `Task<ImageBatch>` that resolves to the completed `ImageBatch` object once processing finishes.  

**Throws:**  
- `ArgumentNullException` if `batchId` is `null`.  
- `InvalidOperationException` if the batch does not exist or is already in a terminal state.  
- `OperationCanceledException` if processing is cancelled via `CancelBatch`.

### `public ImageBatch? GetBatchStatus(string batchId)`
Retrieves the current status of a batch without starting or modifying its processing.  
**Parameters:**  
- `batchId` – The identifier of the batch to query.  

**Returns:** An `ImageBatch?` object representing the batch’s state, or `null` if no batch with the given identifier exists.  

**Throws:**  
- `ArgumentNullException` if `batchId` is `null`.

### `public bool CancelBatch(string batchId)`
Requests cancellation of an active batch.  
**Parameters:**  
- `batchId` – The identifier of the batch to cancel.  

**Returns:** `true` if the batch was found and cancellation was requested; `false` if the batch does not exist or is already completed.  

**Throws:**  
- `ArgumentNullException` if `batchId` is `null`.

### `public async Task<ImageBatch> CreateBatchAsync(IEnumerable<string> imagePaths, ProcessingOptions options)`
Creates a new batch from the specified image files and processing options, then returns the newly created `ImageBatch` without starting processing.  
**Parameters:**  
- `imagePaths` – A collection of file paths to the images to include in the batch.  
- `options` – A `ProcessingOptions` object defining parameters such as output format, resize dimensions, or filter settings.  

**Returns:** A `Task<ImageBatch>` that resolves to the created `ImageBatch` instance.  

**Throws:**  
- `ArgumentNullException` if `imagePaths` or `options` is `null`.  
- `ArgumentException` if `imagePaths` is empty or contains invalid paths.  
- `InvalidOperationException` if the service cannot allocate resources for the new batch.

### `public Dictionary<string, object> GetBatchProgress(string batchId)`
Returns a dictionary of progress metrics for the specified batch.  
**Parameters:**  
- `batchId` – The identifier of the batch.  

**Returns:** A `Dictionary<string, object>` containing keys such as `"ProcessedCount"`, `"TotalCount"`, `"PercentComplete"`, and `"CurrentStage"`. Returns an empty dictionary if the batch does not exist.  

**Throws:**  
- `ArgumentNullException` if `batchId` is `null`.

### `public int GetActiveBatchCount()`
Returns the number of batches that are currently being processed (i.e., not yet completed or cancelled).  
**No parameters.**  
**Returns:** An `int` representing the count of active batches.

### `public IEnumerable<ImageBatch> GetActiveBatches()`
Enumerates all batches that are currently active.  
**No parameters.**  
**Returns:** An `IEnumerable<ImageBatch>` containing the active batch objects. The enumeration is a snapshot at the time of the call.

## Usage

### Example 1: Create, process, and monitor a batch

```csharp
var service = new BatchProcessingService();
var imagePaths = new[] { "photo1.jpg", "photo2.png", "photo3.bmp" };
var options = new ProcessingOptions { OutputFormat = "jpeg", MaxWidth = 1920 };

// Create the batch (processing not started)
ImageBatch batch = await service.CreateBatchAsync(imagePaths, options);

// Start processing
Task<ImageBatch> processingTask = service.ProcessBatchAsync(batch.Id);

// Poll progress while processing
while (!processingTask.IsCompleted)
{
    var progress = service.GetBatchProgress(batch.Id);
    Console.WriteLine($"Progress: {progress["PercentComplete"]}%");
    await Task.Delay(500);
}

ImageBatch completedBatch = await processingTask;
Console.WriteLine($"Batch {completedBatch.Id} finished with status {completedBatch.Status}");
```

### Example 2: Cancel a batch and check active batches

```csharp
var service = new BatchProcessingService();

// Create and start two batches
var batch1 = await service.CreateBatchAsync(new[] { "a.jpg" }, new ProcessingOptions());
var batch2 = await service.CreateBatchAsync(new[] { "b.jpg" }, new ProcessingOptions());

_ = service.ProcessBatchAsync(batch1.Id);
_ = service.ProcessBatchAsync(batch2.Id);

// Cancel the first batch
bool cancelled = service.CancelBatch(batch1.Id);
Console.WriteLine($"Batch {batch1.Id} cancelled: {cancelled}");

// Check active batches
int activeCount = service.GetActiveBatchCount();
Console.WriteLine($"Active batches: {activeCount}");

foreach (var active in service.GetActiveBatches())
{
    Console.WriteLine($"Still active: {active.Id}");
}
```

## Notes

- **Thread safety:** All public members are thread-safe. The service uses internal locking to protect its state, so multiple threads may call any method concurrently without additional synchronization.
- **Batch lifecycle:** A batch transitions through states: `Created` → `Processing` → `Completed` or `Cancelled`. `CreateBatchAsync` returns a batch in the `Created` state. `ProcessBatchAsync` moves it to `Processing`. `CancelBatch` requests cancellation; the batch may not stop immediately but will eventually transition to `Cancelled`.
- **Null and empty arguments:** Methods that accept a `batchId` or collection parameter throw `ArgumentNullException` for `null` inputs. `CreateBatchAsync` throws `ArgumentException` if `imagePaths` is empty.
- **Non-existent batches:** `GetBatchStatus` returns `null` for unknown batch IDs. `CancelBatch` returns `false` in that case. `GetBatchProgress` returns an empty dictionary. Other methods throw `InvalidOperationException` when attempting to process a non-existent batch.
- **Progress dictionary:** The keys and value types in the dictionary returned by `GetBatchProgress` are not guaranteed to be stable across versions. Consumers should handle missing keys gracefully.
- **Active batch enumeration:** `GetActiveBatches` returns a snapshot; the returned collection is not updated if batches complete or are cancelled after the call. Use `GetActiveBatchCount` for a lightweight count.
