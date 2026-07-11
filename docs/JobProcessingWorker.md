# JobProcessingWorker

A worker component responsible for managing the lifecycle of GPU-accelerated image processing jobs. It coordinates job submission, execution, and cleanup while providing control over the processing pipeline through start/stop semantics and resource management.

## API

### `public JobProcessingWorker`

Initializes a new instance of the `JobProcessingWorker` with default configuration. The worker is created in a stopped state and must be explicitly started via `StartAsync` before processing jobs.

### `public string GetName()`

Returns the human-readable name of the worker instance.

- **Returns**: `string` – A name identifying the worker, typically used for logging or diagnostics.
- **Throws**: No exceptions are thrown under normal operation.

### `public async Task StartAsync()`

Starts the worker's processing pipeline, enabling it to accept and execute image processing jobs. This method is idempotent; subsequent calls after the first successful invocation have no effect.

- **Returns**: `Task` – A task representing the asynchronous operation. The task completes when the worker is fully operational.
- **Throws**:
  - `InvalidOperationException` – If the worker is already running.
  - `ObjectDisposedException` – If the worker has been disposed.

### `public async Task StopAsync()`

Stops the worker's processing pipeline, halting acceptance of new jobs and allowing in-flight jobs to complete. This method is idempotent; subsequent calls after the first successful invocation have no effect.

- **Returns**: `Task` – A task representing the asynchronous operation. The task completes when the worker has stopped accepting new jobs and all ongoing jobs have finished.
- **Throws**:
  - `InvalidOperationException` – If the worker is not running.
  - `ObjectDisposedException` – If the worker has been disposed.

### `public void Dispose()`

Releases all resources used by the `JobProcessingWorker`, including stopping any active processing and cleaning up GPU resources. This method is idempotent; subsequent calls have no effect.

- **Throws**: No exceptions are thrown under normal operation.

## Usage

### Starting and stopping a worker

```csharp
using var worker = new JobProcessingWorker();
Console.WriteLine($"Worker name: {worker.GetName()}");

await worker.StartAsync();
Console.WriteLine("Worker started. Processing jobs...");

// Simulate job processing
await Task.Delay(1000);

await worker.StopAsync();
Console.WriteLine("Worker stopped.");
```

### Disposing a worker explicitly

```csharp
var worker = new JobProcessingWorker();
try
{
    await worker.StartAsync();
    // Process jobs...
}
finally
{
    worker.Dispose();
}
```

## Notes

- The worker is **not thread-safe** for concurrent calls to `StartAsync` or `StopAsync`; such operations should be serialized by the caller.
- Disposing the worker while `StartAsync` or `StopAsync` is in progress may lead to undefined behavior; ensure these tasks complete before disposal.
- The worker assumes exclusive access to GPU resources during operation; concurrent use of the same GPU by other components may cause contention or failure.
- If `StopAsync` is called while jobs are still processing, the worker waits for completion before returning, which may introduce latency in shutdown scenarios.
