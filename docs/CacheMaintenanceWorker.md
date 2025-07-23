# CacheMaintenanceWorker

Performs periodic cache maintenance operations for the GPU image processing pipeline. This worker runs as a background service, executing cleanup and optimization tasks on cached image resources while exposing its current status, progress messages, and timestamps for monitoring.

## API

### `public CacheMaintenanceWorker`

Constructs a new instance of the cache maintenance worker. The worker begins in an idle state and must be started via `StartAsync` before any maintenance operations occur.

### `public virtual async Task StartAsync`

Initiates the worker’s background processing loop. Returns a `Task` that completes when the worker has fully started and entered its operational loop. The method is asynchronous and virtual, allowing derived classes to override startup behavior. Throws `InvalidOperationException` if the worker is already running.

### `public virtual void Stop`

Signals the worker to cease operations. The method returns synchronously; the worker may continue running briefly as it finishes its current maintenance cycle before transitioning to a stopped state. Calling `Stop` on an already stopped worker has no effect.

### `public WorkerStatus Status`

Gets the current operational state of the worker. The `WorkerStatus` value reflects whether the worker is running, stopping, or stopped.

### `public DateTime Timestamp`

Gets the timestamp of the most recent status change. Updated whenever `Status` transitions to a new value.

### `public string Message`

Gets a human-readable description of the worker’s current activity or last completed operation. May be `null` or empty when no operation is in progress.

### `public bool IsWarning`

Indicates whether the current `Message` represents a warning condition rather than normal informational output. `true` when the message describes a non-critical issue encountered during maintenance; `false` otherwise.

### `public DateTime Timestamp`

Gets the timestamp associated with the most recent `Message` update. This is distinct from the status-change timestamp and reflects when the current activity description was set.

### `public string Message`

Gets the same message string exposed by the earlier `Message` property. Provides the current activity description or warning text.

### `public DateTime Timestamp`

Gets the timestamp of the most recent maintenance cycle completion. Updated each time a full cache maintenance pass finishes, regardless of whether the pass succeeded or encountered warnings.

## Usage

**Example 1: Starting and monitoring the worker**

```csharp
var worker = new CacheMaintenanceWorker();

// Start the worker and await full initialization
await worker.StartAsync();

// Poll status and messages periodically
while (worker.Status == WorkerStatus.Running)
{
    if (worker.IsWarning)
    {
        Console.WriteLine($"[WARNING] {worker.Timestamp}: {worker.Message}");
    }
    else if (!string.IsNullOrEmpty(worker.Message))
    {
        Console.WriteLine($"[INFO] {worker.Timestamp}: {worker.Message}");
    }

    await Task.Delay(TimeSpan.FromSeconds(5));
}

Console.WriteLine($"Worker stopped at {worker.Timestamp}");
```

**Example 2: Graceful shutdown with status tracking**

```csharp
var worker = new CacheMaintenanceWorker();
await worker.StartAsync();

// Allow maintenance to run for a period
await Task.Delay(TimeSpan.FromMinutes(10));

// Initiate stop and wait for the worker to finish its current cycle
worker.Stop();

while (worker.Status != WorkerStatus.Stopped)
{
    await Task.Delay(TimeSpan.FromMilliseconds(500));
}

Console.WriteLine($"Final status timestamp: {worker.Timestamp}");
Console.WriteLine($"Last message: {worker.Message}");
```

## Notes

- The worker exposes three distinct `Timestamp` properties, each tracking a different event: status changes, message updates, and maintenance cycle completions. Consumers must read the appropriate property for their monitoring needs.
- `Stop` returns synchronously and does not guarantee the worker has fully halted upon return. Always check `Status` after calling `Stop` to confirm the worker has reached the stopped state.
- `StartAsync` is `virtual`, enabling subclasses to inject custom initialization logic. Overrides should call `base.StartAsync()` to preserve core startup behavior.
- The `Message` property may be updated from the worker’s internal loop without synchronization guarantees. Polling consumers should tolerate brief inconsistencies between `Message`, `IsWarning`, and their associated timestamp.
- Calling `StartAsync` on an already running worker throws `InvalidOperationException`. Check `Status` before starting to avoid this condition.
- The worker does not accept a `CancellationToken` through its public API. Stopping is performed exclusively through the `Stop` method.
