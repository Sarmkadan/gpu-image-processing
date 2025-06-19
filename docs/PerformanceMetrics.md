# PerformanceMetrics

`PerformanceMetrics` captures runtime statistics for GPU‑accelerated image processing operations, including CPU and GPU utilization, memory consumption, execution timing, and operation success rates. It is intended to be instantiated per processing session or per component and updated as work progresses.

## API

### Id  
**Type:** `Guid`  
**Purpose:** Unique identifier for the metrics instance, typically set once at construction and never changed.  
**Throws:** None.

### RecordedAt  
**Type:** `DateTime`  
**Purpose:** Timestamp indicating when the metrics were last recorded or reset. Updated automatically by `Reset` and when significant counters change.  
**Throws:** None.

### CpuUsagePercent  
**Type:** `double`  
**Purpose:** Current CPU utilization as a percentage (0‑100).  
**Throws:** None.

### MemoryUsedBytes  
**Type:** `long`  
**Purpose:** Amount of process memory currently allocated, in bytes.  
**Throws:** None.

### GpuMemoryUsedBytes  
**Type:** `long`  
**Purpose:** Amount of GPU memory currently allocated, in bytes.  
**Throws:** None.

### GpuUtilizationPercent  
**Type:** `double`  
**Purpose:** Current GPU utilization as a percentage (0‑100).  
**Throws:** None.

### AverageExecutionTimeMs  
**Type:** `double`  
**Purpose:** Arithmetic mean of all recorded operation execution times, in milliseconds. Updated after each call to `RecordExecution`.  
**Throws:** None.

### MaxExecutionTimeMs  
**Type:** `double`  
**Purpose:** Maximum execution time observed among recorded operations, in milliseconds.  
**Throws:** None.

### MinExecutionTimeMs  
**Type:** `double`  
**Purpose:** Minimum execution time observed among recorded operations, in milliseconds.  
**Throws:** None.

### ImagePixelsProcessedPerSecond  
**Type:** `long`  
**Purpose:** Approximate number of image pixels processed per second, derived from total pixels and elapsed time.  
**Throws:** None.

### TotalOperationsCount  
**Type:** `int`  
**Purpose:** Cumulative count of all operations attempted since the instance was created or last reset.  
**Throws:** None.

### FailedOperationsCount  
**Type:** `int`  
**Purpose:** Number of operations that ended in failure (e.g., exceptions, timeouts).  
**Throws:** None.

### ThroughputMegabytesPerSecond  
**Type:** `double`  
**Purpose:** Data throughput measured in megabytes per second, based on processed image size and elapsed time.  
**Throws:** None.

### ExecutionTimes  
**Type:** `List<double>`  
**Purpose:** Collection of individual operation execution times (in milliseconds) recorded via `RecordExecution`. The list is not sorted and may grow without bound unless cleared by `Reset`.  
**Throws:** None.

### PerformanceMetrics  
**Purpose:** Parameterless constructor that creates a new, zero‑initialized metrics instance. Sets `Id` to a new `Guid`, `RecordedAt` to the current UTC time, and all numeric counters to zero.  
**Throws:** None.

### RecordExecution  
**Signature:** `void RecordExecution()`  
**Purpose:** Records a single operation execution. Internally increments `TotalOperationsCount`, updates `AverageExecutionTimeMs‑based statistics (`MinExecutionTimeMs`, `MaxExecutionTimeMs`, `AverageExecutionTimeMs`), appends the measured execution time to `ExecutionTimes`, and updates derived rates such as `ImagePixelsProcessedPerSecond` and `ThroughputMegabytesPerSecond`. The actual execution time value is assumed to be supplied by the caller via an external mechanism (e.g., a surrounding timer).  
**Throws:** None.

### GetSuccessRate  
**Signature:** `double GetSuccessRate()`  
**Purpose:** Returns the percentage of operations that succeeded, calculated as `((TotalOperationsCount - FailedOperationsCount) / TotalOperationsCount) * 100`. If `TotalOperationsCount` is zero, returns `0.0`.  
**Returns:** A value between `0.0` and `100.0`.  
**Throws:** None.

### IsMemoryWarningRequired  
**Signature:** `bool IsMemoryWarningRequired()`  
**Purpose:** Determines whether the current memory usage warrants a warning. Returns `true` when either `MemoryUsedBytes` or `GpuMemoryUsedBytes` exceeds a predefined threshold (implementation‑specific); otherwise returns `false`.  
**Returns:** `true` if a memory warning condition is met, `false` otherwise.  
**Throws:** None.

### GetMemoryUsagePercent  
**Signature:** `double GetMemoryUsagePercent()`  
**Purpose:** Returns the process memory usage as a percentage of the total available memory (physical + virtual). The calculation uses `MemoryUsedBytes` divided by the system‑reported total memory, multiplied by 100.  
**Returns:** A value between `0.0` and `100.0`.  
**Throws:** None.

### Reset  
**Signature:** `void Reset()`  
**Purpose:** Resets all metrics to their initial state: clears `ExecutionTimes`, sets all counters to zero, generates a new `Guid` for `Id`, and updates `RecordedAt` to the current UTC time.  
**Throws:** None.

## Usage

```csharp
using System;
using System.Collections.Generic;
using GpuImageProcessing; // namespace containing PerformanceMetrics

public class ImageProcessor
{
    private readonly PerformanceMetrics _metrics = new PerformanceMetrics();

    public void ProcessFrame(byte[] imageData)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Simulate GPU work …
            // …
        }
        catch (Exception)
        {
            // Count failure
            _metrics.FailedOperationsCount++;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            // Assume execution time is captured elsewhere and supplied here:
            _metrics.RecordExecution(); // updates timing stats
            // Update memory/Gpu counters from external monitoring APIs
            _metrics.MemoryUsedBytes = GetCurrentProcessMemory();
            _metrics.GpuMemoryUsedBytes = GetCurrentGpuMemory();
        }
    }

    public void Report()
    {
        Console.WriteLine($"Success rate: {_metrics.GetSuccessRate():F2}%");
        Console.WriteLine($"Avg exec time: {_metrics.AverageExecutionTimeMs:F2} ms");
        Console.WriteLine($"Memory warning: {_metrics.IsMemoryWarningRequired()}");
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using GpuImageProcessing;

public class BenchmarkRunner
{
    public async Task RunAsync()
    {
        var metrics = new PerformanceMetrics();

        for (int i = 0; i < 1000; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await ProcessImageAsync(); // placeholder for actual work
            sw.Stop();

            // Record the elapsed time manually before calling RecordExecution
            // (the method itself does not accept a parameter, so the elapsed
            //  time must be stored elsewhere; here we assume a static holder)
            LastExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            metrics.RecordExecution();

            // Periodically check memory usage
            if (i % 100 == 0 && metrics.IsMemoryWarningRequired())
            {
                Console.WriteLine("High memory usage detected.");
            }
        }

        Console.WriteLine($"Throughput: {metrics.ThroughputMegabytesPerSecond:F2} MB/s");
        metrics.Reset(); // prepare for next benchmark run
    }
}
```

## Notes

- The `ExecutionTimes` list accumulates every recorded execution time; in long‑running scenarios it can consume significant memory. Call `Reset` periodically or replace the list with a bounded collection if unbounded growth is undesirable.  
- All numeric properties are updated non‑atomically; the type does **not** provide internal synchronization. If `PerformanceMetrics` is accessed from multiple threads concurrently, external locking (e.g., `lock` or `ConcurrentBag` for the list) is required to avoid race conditions.  
- `GetSuccessRate` returns `0.0` when `TotalOperationsCount` is zero to avoid division‑by‑zero exceptions.  
- `IsMemoryWarningRequired` and `GetMemoryUsagePercent` rely on external memory‑querying mechanisms; their accuracy depends on the fidelity of those sources.  
- The `Id` field is regenerated on each call to `Reset`, ensuring a fresh identifier for each measurement session.  
- No members throw exceptions under normal operation; invalid state (e.g., negative counters) would indicate misuse of the class rather than a runtime error thrown by the type itself.
