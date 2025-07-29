# TelemetryService
TelemetryService collects, aggregates, and provides access to telemetry data such as events, timing intervals, counters, and gauges for the GPU image processing pipeline. It enables monitoring of operation performance and system health while allowing consumers to retrieve recorded data for reporting or debugging.

## API
### TelemetryService()
Initializes a new instance of the TelemetryService. The service starts with empty telemetry stores and is ready to accept recordings immediately.

### void RecordEvent(TelemetryEvent @event)
Records a single telemetry event.  
- **Parameters**  
  - `@event`: The event to record; must not be null.  
- **Return value**  
  - None.  
- **Exceptions**  
  - `ArgumentNullException` if `@event` is null.  
  - `ObjectDisposedException` if the service has been disposed.

### TimingToken StartTiming()
Begins a timing interval that can be used to measure the duration of an operation.  
- **Parameters**  
  - None.  
- **Return value**  
  - A `TimingToken` representing the started timer; calling `Dispose` on the token stops the timer and records the elapsed time.  
- **Exceptions**  
  - `ObjectDisposedException` if the service has been disposed.

### void IncrementCounter(string name, long increment = 1)
Increments a named counter by the specified amount.  
- **Parameters**  
  - `name`: Identifier of the counter; must not be null or whitespace.  
  - `increment`: The value to add; defaults to 1.  
- **Return value**  
  - None.  
- **Exceptions**  
  - `ArgumentException` if `name` is null, empty, or consists only of whitespace.  
  - `ObjectDisposedException` if the service has been disposed.

### void SetGauge(string name, double value)
Sets a named gauge to the supplied value, overwriting any previous value.  
- **Parameters**  
  - `name`: Identifier of the gauge; must not be null or whitespace.  
  - `value`: The gauge value to store.  
- **Return value**  
  - None.  
- **Exceptions**  
  - `ArgumentException` if `name` is null, empty, or consists only of whitespace.  
  - `ObjectDisposedException` if the service has been disposed.

### OperationStats GetOperationStats(string operationName)
Retrieves aggregated statistics for a named operation.  
- **Parameters**  
  - `operationName`: The operation whose stats are requested; must not be null or whitespace.  
- **Return value**  
  - An `OperationStats` instance containing count, success/failure counts, timing aggregates, and the last recorded timestamp for the operation. Returns a default‑valued struct if no data exists for the operation.  
- **Exceptions**  
  - `ArgumentException` if `operationName` is null, empty, or consists only of whitespace.  
  - `ObjectDisposedException` if the service has been disposed.

### List<TelemetryEvent> GetEvents()
Returns a snapshot of all recorded telemetry events.  
- **Parameters**  
  - None.  
- **Return value**  
  - A new `List<TelemetryEvent>` containing copies of the events in the order they were recorded. Returns an empty list if no events have been recorded.  
- **Exceptions**  
  - `ObjectDisposedException` if the service has been disposed.

### SystemTelemetryStats GetSystemStats()
Retrieves overall telemetry statistics for the service.  
- **Parameters**  
  - None.  
- **Return value**  
  - A `SystemTelemetryStats` instance summarizing total events, counters, gauges, and timing data across all operations.  
- **Exceptions**  
  - `ObjectDisposedException` if the service has been disposed.

### void Clear()
Removes all recorded telemetry data, resetting counters, gauges, events, and timing aggregates to their initial state.  
- **Parameters**  
  - None.  
- **Return value**  
  - None.  
- **Exceptions**  
  - `ObjectDisposedException` if the service has been disposed.

### string Name
Gets the identifier assigned to this telemetry service instance.  
- **Return value**  
  - A string representing the service name; never null.  
- **Exceptions**  
  - None.

### long Count
Gets the total number of telemetry events recorded since the service was created or last cleared.  
- **Return value**  
  - A non‑negative integer.  
- **Exceptions**  
  - None.

### long SuccessCount
Gets the number of recorded events that indicated success.  
- **Return value**  
  - A non‑negative integer.  
- **Exceptions**  
  - None.

### long FailureCount
Gets the number of recorded events that indicated failure.  
- **Return value**  
  - A non‑negative integer.  
- **Exceptions**  
  - None.

### double TotalMilliseconds
Gets the cumulative elapsed time (in milliseconds) of all timed operations recorded.  
- **Return value**  
  - A non‑negative double representing milliseconds.  
- **Exceptions**  
  - None.

### double MinMilliseconds
Gets the smallest elapsed time (in milliseconds) among all timed operations recorded.  
- **Return value**  
  - A non‑negative double; returns 0 if no timings have been recorded.  
- **Exceptions**  
  - None.

### double MaxMilliseconds
Gets the largest elapsed time (in milliseconds) among all timed operations recorded.  
- **Return value**  
  - A non‑negative double; returns 0 if no timings have been recorded.  
- **Exceptions**  
  - None.

### double Value
Gets the most recent gauge value set via `SetGauge` for the default gauge (if the service tracks a single gauge); otherwise returns 0.  
- **Return value**  
  - A double representing the latest gauge value.  
- **Exceptions**  
  - None.

### DateTime LastRecordedAt
Gets the timestamp of the most recently recorded telemetry item (event, timing, counter increment, or gauge set).  
- **Return value**  
  - A `DateTime` in UTC; equals `DateTime.MinValue` if no data has been recorded.  
- **Exceptions**  
  - None.

### void Dispose()
Releases any resources held by the telemetry service. After disposal, further calls to members will throw `ObjectDisposedException`.  
- **Parameters**  
  - None.  
- **Return value**  
  - None.  
- **Exceptions**  
  - None.

### Guid Id
Gets a unique identifier for this telemetry service instance.  
- **Return value**  
  - A `Guid` that differs for each instance.  
- **Exceptions**  
  - None.

## Usage
```csharp
using GpuImageProcessing.Telemetry;

// Create a service named for the current processing pipeline
var telemetry = new TelemetryService { Name = "PipelineA" };

// Record a custom event
telemetry.RecordEvent(new TelemetryEvent
{
    Operation = "LoadImage",
    Success = true,
    TimestampUtc = DateTime.UtcNow,
    Properties = { ["FileSize"] = 1024000 }
});

// Measure the duration of a processing step
using (telemetry.StartTiming())
{
    // Simulated work
    Thread.Sleep(15);
}

// Increment a counter for each processed frame
telemetry.IncrementCounter("FramesProcessed");

// Update a gauge reflecting current memory usage
telemetry.SetGauge("MemoryMb", Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0));

// Retrieve statistics for the LoadImage operation
var stats = telemetry.GetOperationStats("LoadImage");
Console.WriteLine($"LoadImage called {stats.Count} times, avg {stats.AverageMilliseconds:F1}ms");

// Later, when the service is no longer needed
telemetry.Dispose();
```
```csharp
using GpuImageProcessing.Telemetry;

// Example showing batch processing and final reporting
var telemetry = new TelemetryService();

foreach (var image in imageBatch)
{
    using (telemetry.StartTiming())
    {
        // Process image...
        var result = ProcessImage(image);
        telemetry.RecordEvent(new TelemetryEvent
        {
            Operation = "ProcessImage",
            Success = result.IsSuccess,
            TimestampUtc = DateTime.UtcNow
        });
    }

    // Track a counter of failures
    if (!result.IsSuccess)
        telemetry.IncrementCounter("ProcessFailures");
}

// Obtain overall system telemetry
var system = telemetry.GetSystemStats();
Console.WriteLine($"Total events: {system.TotalEvents}");
Console.WriteLine($"Total processing time: {system.TotalMilliseconds:F1}ms");
Console.WriteLine($"Failure rate: {(double)system.FailureCount / system.TotalEvents:P2}");

// Reset for the next batch
telemetry.Clear();
```

## Notes
- All instance members are safe to call concurrently from multiple threads; internal state is protected with appropriate locking mechanisms.  
- Properties such as `Count`, `SuccessCount`, `FailureCount`, `TotalMilliseconds`, `MinMilliseconds`, `MaxMilliseconds`, `Value`, and `LastRecordedAt` return a snapshot of the state at the moment of access; rapid updates by other threads may cause the values to change immediately after they are read.  
- After `Dispose` is invoked, any subsequent call to a member (including property getters) will throw `ObjectDisposedException`. It is recommended to treat the service as unusable after disposal.  
- `Clear` does not reset the `Id` or `Name` properties; they retain the values assigned at construction or set explicitly.  
- If a timing token returned by `StartTiming` is disposed without having been started (e.g., via a misuse of the API), the resulting elapsed time will be zero; the service does not validate the token’s origin beyond null checks.  
- The `Value` property reflects the most recent value set by `SetGauge` for the default gauge; if the service is configured to track multiple gauges, this property may return 0 and callers should rely on `GetSystemStats` for gauge‑specific data.  
- All string‑based arguments (`name` in `IncrementCounter` and `SetGauge`, `operationName` in `GetOperationStats`) are validated for null, empty, or whitespace content; invalid input results in an `ArgumentException`.  
- The service does not automatically flush telemetry to external stores; consumers must persist data retrieved via `GetEvents`, `GetOperationStats`, or `GetSystemStats` as needed.
