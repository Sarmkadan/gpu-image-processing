# PerformanceMonitoringService

`PerformanceMonitoringService` provides centralized tracking and reporting of GPU image processing pipeline performance. It records operation durations, maintains system-level metrics such as memory and GPU utilization, monitors throughput rates, and exposes both instantaneous snapshots and historical aggregates for diagnostics and tuning.

## API

### `public PerformanceMonitoringService()`
Initializes a new instance of the service with empty metric history and zeroed counters. No external dependencies are required at construction time.

### `public void RecordOperation(string operationName, TimeSpan duration)`
Records a single named operation with its elapsed duration. The operation name is case-sensitive and stored as given. If `operationName` is `null`, an `ArgumentNullException` is thrown. If `duration` is negative, an `ArgumentOutOfRangeException` is thrown.

### `public void UpdateSystemMetrics(float memoryUsageMB, float gpuUtilizationPercent)`
Overwrites the current system-level metrics snapshot. Values are expected to be non-negative; negative inputs are clamped to zero without throwing. This method does not append to history—only `SnapshotAndReset` or `GetCurrentMetrics` capture the values into the historical record.

### `public void UpdateThroughput(long itemsProcessed, TimeSpan elapsed)`
Updates the throughput calculation based on the number of items processed in the given time window. If `elapsed` is zero or negative, an `ArgumentOutOfRangeException` is thrown. Throughput is stored internally as items per second and reflected in subsequent metric snapshots.

### `public PerformanceMetrics GetCurrentMetrics()`
Returns a live `PerformanceMetrics` object representing the current accumulated state, including the latest system metrics, average operation durations, and current throughput. The returned object is a snapshot copy; mutations to it do not affect the service. This call does not reset any counters.

### `public PerformanceMetrics SnapshotAndReset()`
Atomically captures the current `PerformanceMetrics` and resets all accumulators (operation counts, total durations, throughput counters). System metrics are captured but not reset. The returned snapshot is appended to the internal history accessible via `GetMetricsHistory`. If called when no operations have been recorded, the returned metrics contain zeroed aggregates.

### `public IEnumerable<PerformanceMetrics> GetMetricsHistory()`
Returns an enumerable of all snapshots previously produced by `SnapshotAndReset`, in chronological order. The underlying collection is a defensive copy; modifications to the returned sequence do not alter the service’s history. If no snapshots have been taken, the enumerable is empty.

### `public Dictionary<string, double> GetAverageMetrics()`
Computes and returns a dictionary mapping each recorded operation name to its average duration in milliseconds across all recordings since the last reset. The dictionary is built from the current in-memory accumulators and does not include historical snapshots. If no operations have been recorded, the dictionary is empty.

### `public string GetPerformanceReport()`
Generates a formatted plain-text report containing current system metrics, per-operation averages, current throughput, and a summary of recent historical snapshots. The format is implementation-defined and intended for logging or diagnostic output. This method does not modify internal state.

## Usage

### Example 1: Basic pipeline instrumentation
```csharp
var monitor = new PerformanceMonitoringService();

// Simulate processing an image batch
var sw = Stopwatch.StartNew();
ProcessBatch(images);
sw.Stop();

monitor.RecordOperation("BatchProcess", sw.Elapsed);
monitor.UpdateThroughput(images.Count, sw.Elapsed);
monitor.UpdateSystemMetrics(GetMemoryMB(), GetGpuPercent());

PerformanceMetrics current = monitor.GetCurrentMetrics();
Console.WriteLine($"Current throughput: {current.ThroughputItemsPerSecond:F2} items/s");
```

### Example 2: Periodic snapshots with historical reporting
```csharp
var monitor = new PerformanceMonitoringService();
var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

while (await timer.WaitForNextTickAsync())
{
    monitor.UpdateSystemMetrics(GetMemoryMB(), GetGpuPercent());

    // Take an atomic snapshot and reset accumulators for the next window
    PerformanceMetrics windowMetrics = monitor.SnapshotAndReset();

    // Log the report every 5 windows
    if (monitor.GetMetricsHistory().Count() % 5 == 0)
    {
        string report = monitor.GetPerformanceReport();
        File.AppendAllText("perf.log", report + Environment.NewLine);
    }
}
```

## Notes

- **Thread safety**: All public methods that mutate state (`RecordOperation`, `UpdateSystemMetrics`, `UpdateThroughput`, `SnapshotAndReset`) are safe to call concurrently from multiple threads. `SnapshotAndReset` performs the capture and reset as an atomic unit, ensuring no partial updates are visible to other threads.
- **Empty state**: `GetCurrentMetrics`, `GetAverageMetrics`, and `GetPerformanceReport` all handle the case where no operations have been recorded by returning zeroed metrics, an empty dictionary, or a report indicating no data, respectively. No exceptions are thrown due to absence of data.
- **Negative input handling**: `UpdateSystemMetrics` clamps negative values to zero silently, while `RecordOperation` and `UpdateThroughput` throw on invalid arguments. Callers should validate sensor readings before passing them to `UpdateSystemMetrics` if negative values indicate a sensor fault.
- **History growth**: `GetMetricsHistory` accumulates a new entry on every `SnapshotAndReset` call. In long-running processes, callers should periodically trim or archive the history to avoid unbounded memory growth. The service itself does not enforce a retention limit.
- **Report format stability**: The output of `GetPerformanceReport` is intended for human consumption and may change across versions. Automated parsing of this string is discouraged; use `GetCurrentMetrics` or `GetAverageMetrics` for programmatic access.
