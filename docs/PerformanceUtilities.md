# PerformanceUtilities

Utility class that provides static helpers for measuring performance, memory usage, CPU load, and statistical metrics, along with a nested `PerformanceTimer` type for high‑resolution timing of code blocks.

## API

### Static members

| Member | Purpose | Parameters | Return value | Exceptions |
|--------|---------|------------|--------------|------------|
| `StartTimer` | Begins a new timing interval and returns a `PerformanceTimer` instance that is already running. | None | `PerformanceTimer` – a timer that has started measuring elapsed time. | May throw `InvalidOperationException` if the underlying high‑resolution counter is not available. |
| `GetCurrentMemoryUsage` | Retrieves the current managed memory usage of the process. | None | `long` – number of bytes currently allocated. | May throw `NotSupportedException` on platforms where memory information cannot be obtained. |
| `GetPeakMemoryUsage` | Retrieves the peak managed memory usage observed since the process started. | None | `long` – number of bytes at the peak. | May throw `NotSupportedException` on unsupported platforms. |
| `GetAvailableMemory` | Retrieves the amount of memory currently available for allocation. | None | `long` – number of bytes available. | May throw `NotSupportedException` if the query fails. |
| `CalculateThroughput` | Computes throughput as operations per second given a count and elapsed time. | `long operationCount`, `double elapsedSeconds` | `double` – operations per second. | Throws `ArgumentOutOfRangeException` if `elapsedSeconds` is zero or negative. |
| `GetLatencyPercentile` | Returns the latency value at the specified percentile from a sample set. | `double[] latencies`, `double percentile` (0‑100) | `double` – latency at the given percentile. | Throws `ArgumentNullException` if `latencies` is null; throws `ArgumentOutOfRangeException` if `percentile` is outside `[0,100]` or if `latencies` is empty. |
| `GetAverageLatency` | Calculates the arithmetic mean of a latency sample set. | `double[] latencies` | `double` – average latency. | Throws `ArgumentNullException` if `latencies` is null; throws `InvalidOperationException` if the array is empty. |
| `GetMedianLatency` | Computes the median latency of a sample set. | `double[] latencies` | `double` – median latency. | Throws `ArgumentNullException` if `latencies` is null; throws `InvalidOperationException` if the array is empty. |
| `ForceGarbageCollection` | Requests a full garbage collection and waits for its completion. | None | `void` | None. |
| `FormatDuration` | Formats a time span into a human‑readable string (e.g., "12.3 ms"). | `double totalSeconds` | `string` – formatted duration. | Throws `ArgumentOutOfRangeException` if `totalSeconds` is negative. |
| `FormatMemory` | Formats a byte count into a suitable unit (B, KB, MB, GB, TB). | `long bytes` | `string` – formatted memory size. | Throws `ArgumentOutOfRangeException` if `bytes` is negative. |
| `GetCpuUsage` | Returns an estimate of the current CPU usage as a percentage. | None | `float` – CPU usage from 0 to 100. | May throw `NotSupportedException` if CPU counters are unavailable. |
| `CalculateStandardDeviation` | Computes the standard deviation of a set of values. | `double[] values` | `double` – standard deviation. | Throws `ArgumentNullException` if `values` is null; throws `InvalidOperationException` if the array contains fewer than two elements. |

### Nested type: `PerformanceTimer`

A lightweight, high‑resolution stopwatch used to measure elapsed time for a scoped operation.

| Member | Purpose | Parameters | Return value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Stop` | Stops the timer and returns the elapsed time as a formatted string. | None | `string` – formatted duration (see `FormatDuration`). | Throws `InvalidOperationException` if the timer has already been stopped. |
| `Dispose` | Stops the timer (if still running) and releases any resources. Implements the dispose pattern. | None | `void` | None. |
| `ToString` | Returns a string representation of the elapsed time; equivalent to calling `Stop` if the timer is still running, otherwise returns the last measured duration. | None | `string` – formatted duration. | None. |

## Usage

### Measuring a code block with `PerformanceTimer`

```csharp
using GpuImageProcessing.Utilities;

// Start timing
using var timer = PerformanceUtilities.StartTimer;

// Simulate work
ProcessImage(frame);

// timer.Dispose() is called automatically via using block,
// which stops the timer and releases resources.
string elapsed = timer.ToString(); // e.g., "4.27 ms"
Console.WriteLine($"Processing took {elapsed}");
```

### Retrieving system metrics and calculating throughput

```csharp
using GpuImageProcessing.Utilities;

// Get current memory usage
long currentMem = PerformanceUtilities.GetCurrentMemoryUsage;
Console.WriteLine($"Current memory: {PerformanceUtilities.FormatMemory(currentMem)}");

// Estimate CPU load
float cpu = PerformanceUtilities.GetCpuUsage();
Console.WriteLine($"CPU usage: {cpu:0.0}%");

// Calculate throughput after processing a batch
long framesProcessed = 120;
double elapsedSec = 0.35; // obtained from a timer
double fps = PerformanceUtilities.CalculateThroughput(framesProcessed, elapsedSec);
Console.WriteLine($"Throughput: {fps:F2} frames/sec");
```

## Notes

- All static members of `PerformanceUtilities` are thread‑safe and can be called concurrently from multiple threads without external synchronization.
- Instances of `PerformanceTimer` are **not** thread‑safe; a single instance should be accessed by only one thread at a time.
- The `StartTimer` property returns a timer that has already begun measuring; calling `Stop` or `Dispose` more than once on the same instance will throw an `InvalidOperationException`.
- Memory‑related methods (`GetCurrentMemoryUsage`, `GetPeakMemoryUsage`, `GetAvailableMemory`) rely on platform‑specific performance counters; on unsupported operating systems they throw `NotSupportedException`.
- `ForceGarbageCollection` triggers a blocking collection; use it sparingly in production code as it may degrade performance.
- The latency statistic methods (`GetLatencyPercentile`, `GetAverageLatency`, `GetMedianLatency`) expect a non‑null array containing at least one element (except for standard deviation, which requires at least two). Empty or null inputs result in exceptions as documented.
- Formatting methods (`FormatDuration`, `FormatMemory`) produce culture‑invariant output suitable for logs or UI display. They throw if supplied with negative values.
