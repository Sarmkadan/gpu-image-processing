# MetricsAggregationWorker

The `MetricsAggregationWorker` type collects and aggregates runtime metrics for the GPU image‑processing pipeline. It maintains counters for operations, memory usage, latency, and success rates over a configurable time window and can produce a summarized snapshot via `GetMetricsSummary`.

## API

### MetricsAggregationWorker()
Creates a new instance of `MetricsAggregationWorker`.  
- **Parameters:** none  
- **Return value:** a new worker ready to receive metric updates.  
- **Exceptions:** none under normal conditions; if internal allocation fails, an `OutOfMemoryException` may propagate.

### MetricsSummary GetMetricsSummary()
Returns a snapshot of the aggregated metrics collected since the worker’s start or the last reset.  
- **Parameters:** none  
- **Return value:** a `MetricsSummary` object containing the current aggregate values.  
- **Exceptions:**  
  - `InvalidOperationException` – called before any metric data has been recorded (i.e., `SnapshotCount` is zero).  

### DateTime Timestamp
Gets the UTC time at which the most recent metric sample was added.  
- **Type:** `DateTime` (read‑only)  
- **Exceptions:** none.

### int ProcessId
Gets the identifier of the operating‑system process that the worker is monitoring.  
- **Type:** `int` (read‑only)  
- **Exceptions:** none.

### double MemoryUsageMb
Gets the latest measured memory consumption of the GPU image‑processing component, in megabytes.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### int ThreadCount
Gets the current number of worker threads used by the pipeline.  
- **Type:** `int` (read‑only)  
- **Exceptions:** none.

### long TotalOperations
Gets the cumulative count of image‑processing operations performed since the worker started.  
- **Type:** `long` (read‑only)  
- **Exceptions:** none.

### double SuccessRate
Gets the ratio of successful operations to total operations, expressed as a value between 0 and 1.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### double AverageLatencyMs
Gets the average latency of operations, in milliseconds, over the aggregation window.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### int PeriodMinutes
Gets the length of the aggregation window, in minutes, that defines how far back metrics are considered for summaries.  
- **Type:** `int` (read‑only)  
- **Exceptions:** none.

### int SnapshotCount
Gets the number of metric samples that have been recorded within the current aggregation window.  
- **Type:** `int` (read‑only)  
- **Exceptions:** none.

### double AvgMemoryMb
Gets the average memory usage, in megabytes, across all snapshots in the window.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### double MaxMemoryMb
Gets the peak memory usage observed in the window, in megabytes.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### double AvgLatencyMs
Gets the average latency, in milliseconds, across all snapshots in the window.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### double MaxLatencyMs
Gets the maximum latency observed in the window, in milliseconds.  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### double AvgSuccessRate
Gets the average success rate across all snapshots in the window (value between 0 and 1).  
- **Type:** `double` (read‑only)  
- **Exceptions:** none.

### DateTime StartTime
Gets the UTC time when the worker began collecting metrics.  
- **Type:** `DateTime` (read‑only)  
- **Exceptions:** none.

### DateTime EndTime
Gets the UTC time when the worker stopped collecting metrics; if the worker is still active, this equals `DateTime.MaxValue`.  
- **Type:** `DateTime` (read‑only)  
- **Exceptions:** none.

## Usage

### Example 1: Basic aggregation and summary retrieval
```csharp
using System;
using GpuImageProcessing.Metrics;

var worker = new MetricsAggregationWorker();

// Simulate receiving metric updates (in a real scenario these would come from the pipeline)
worker.Timestamp = DateTime.UtcNow;
worker.ProcessId = Environment.ProcessId;
worker.MemoryUsageMb = 256.5;
worker.ThreadCount = 8;
worker.TotalOperations += 1_000;
worker.SuccessRate = 0.98;
worker.AverageLatencyMs = 12.3;

// After a period, obtain the aggregated summary
MetricsSummary summary = worker.GetMetricsSummary();
Console.WriteLine($"Operations: {summary.TotalOperations}");
Console.WriteLine($"Average latency (ms): {summary.AverageLatencyMs}");
```

### Example 2: Configuring the aggregation window and checking for data availability
```csharp
using System;
using GpuImageProcessing.Metrics;

var worker = new MetricsAggregationWorker();
// Assume the worker internally uses PeriodMinutes to limit the window.
// Set a custom window if the type exposes a setter; otherwise rely on default.
// worker.PeriodMinutes = 5; // hypothetical

// Wait until at least one snapshot is available
while (worker.SnapshotCount == 0)
{
    // In practice, metric updates would be pushed from the pipeline.
    // Here we simulate a delay.
    System.Threading.Thread.Sleep(1000);
}

// Safely retrieve the summary now that data exists
MetricsSummary summary = worker.GetMetricsSummary();
Console.WriteLine($"Memory usage (Mb) – avg: {worker.AvgMemoryMb}, max: {worker.MaxMemoryMb}");
```

## Notes
- The type does **not** provide any public methods for injecting metric data; updates are expected to be made directly to the public properties or through internal mechanisms not shown in the public surface.  
- All properties are get‑only and reflect the current state; they are not atomic. Concurrent updates from multiple threads without external synchronization can lead to inconsistent reads. Consumers should synchronize access (e.g., using a `lock`) if the worker is shared across threads.  
- `GetMetricsSummary` throws `InvalidOperationException` when called before any snapshot has been recorded (`SnapshotCount == 0`). Checking this property beforehand avoids the exception.  
- The aggregation window (`PeriodMinutes`) determines how long samples are retained; older samples are discarded automatically by the internal logic for discarding is internal and not exposed.  
- If the worker is stopped (e.g., by setting `EndTime` to a real value), further calls to `GetMetricsSummary` will return a summary based on the data collected up to that point; no new snapshots will be added.  
- Memory and latency values are expressed in megabytes and milliseconds respectively; callers should apply appropriate scaling if different units are required.  
- The type does not implement `IDisposable`; no explicit resource cleanup is required.
