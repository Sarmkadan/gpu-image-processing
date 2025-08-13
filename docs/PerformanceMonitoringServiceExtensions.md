# PerformanceMonitoringServiceExtensions

Provides extension methods for performance monitoring services, enabling recording of operations, retrieval of metrics with trend analysis, time-range queries, and performance alert generation. This type also exposes current and previous performance snapshots along with computed change percentages across CPU, GPU, memory, throughput, and execution time dimensions.

## API

### Static Methods

#### `RecordOperations`

```csharp
public static void RecordOperations(this IPerformanceMonitoringService service, IEnumerable<PerformanceOperation> operations)
```

Records a batch of performance operations into the monitoring service for later analysis.

- **Parameters:**
  - `service` — The `IPerformanceMonitoringService` instance being extended.
  - `operations` — A collection of `PerformanceOperation` objects to record.
- **Returns:** Nothing.
- **Throws:** `ArgumentNullException` when `service` or `operations` is `null`.

#### `GetMetricsWithTrends`

```csharp
public static PerformanceMetricsWithTrends GetMetricsWithTrends(this IPerformanceMonitoringService service)
```

Retrieves the current performance metrics bundled with computed trend data, including change percentages relative to the previous snapshot.

- **Parameters:**
  - `service` — The `IPerformanceMonitoringService` instance being extended.
- **Returns:** A `PerformanceMetricsWithTrends` object containing current metrics and trend calculations.
- **Throws:** `InvalidOperationException` when no previous metrics exist for trend comparison.

#### `GetMetricsInTimeRange`

```csharp
public static IEnumerable<PerformanceMetrics> GetMetricsInTimeRange(this IPerformanceMonitoringService service, DateTime start, DateTime end)
```

Queries all recorded performance metrics whose timestamps fall within the specified inclusive time range.

- **Parameters:**
  - `service` — The `IPerformanceMonitoringService` instance being extended.
  - `start` — The inclusive start of the time range.
  - `end` — The inclusive end of the time range.
- **Returns:** An enumerable sequence of `PerformanceMetrics` ordered by timestamp.
- **Throws:** `ArgumentOutOfRangeException` when `end` is earlier than `start`.

#### `GetPerformanceAlerts`

```csharp
public static List<PerformanceAlert> GetPerformanceAlerts(this IPerformanceMonitoringService service, AlertThresholds thresholds)
```

Evaluates current metrics against the supplied thresholds and returns a list of alerts for any metrics that exceed their configured limits.

- **Parameters:**
  - `service` — The `IPerformanceMonitoringService` instance being extended.
  - `thresholds` — An `AlertThresholds` object defining limit values for each monitored dimension.
- **Returns:** A `List<PerformanceAlert>` containing zero or more alerts.
- **Throws:** `ArgumentNullException` when `thresholds` is `null`.

### Instance Properties (PerformanceMetricsWithTrends)

#### `Current`

```csharp
public PerformanceMetrics Current { get; }
```

Gets the most recent performance metrics snapshot.

#### `Previous`

```csharp
public PerformanceMetrics? Previous { get; }
```

Gets the immediately preceding performance metrics snapshot, or `null` if no prior snapshot exists.

#### `Timestamp`

```csharp
public DateTime Timestamp { get; }
```

Gets the UTC timestamp when the current metrics were captured.

#### `CpuChangePercent`

```csharp
public double CpuChangePercent { get; }
```

Gets the percentage change in CPU utilization between the previous and current snapshots. Positive values indicate increased usage.

#### `GpuChangePercent`

```csharp
public double GpuChangePercent { get; }
```

Gets the percentage change in GPU utilization between the previous and current snapshots.

#### `MemoryChangePercent`

```csharp
public double MemoryChangePercent { get; }
```

Gets the percentage change in memory consumption between the previous and current snapshots.

#### `ThroughputChangePercent`

```csharp
public double ThroughputChangePercent { get; }
```

Gets the percentage change in processing throughput between the previous and current snapshots.

#### `ExecutionTimeChangePercent`

```csharp
public double ExecutionTimeChangePercent { get; }
```

Gets the percentage change in average execution time between the previous and current snapshots.

### Instance Properties (PerformanceAlert)

#### `Type`

```csharp
public AlertType Type { get; }
```

Gets the category of the alert, indicating which performance dimension triggered it (e.g., CPU, GPU, memory, throughput, or execution time).

#### `Message`

```csharp
public string Message { get; }
```

Gets the human-readable description of the alert condition.

#### `CurrentValue`

```csharp
public double CurrentValue { get; }
```

Gets the metric value that exceeded the threshold.

#### `Threshold`

```csharp
public double Threshold { get; }
```

Gets the configured limit value that was breached.

#### `Timestamp`

```csharp
public DateTime Timestamp { get; }
```

Gets the UTC timestamp when the alert was generated.

### Constructor

#### `PerformanceAlert`

```csharp
public PerformanceAlert(AlertType type, string message, double currentValue, double threshold, DateTime timestamp)
```

Constructs a new `PerformanceAlert` instance with the specified alert details.

- **Parameters:**
  - `type` — The alert category.
  - `message` — The descriptive message.
  - `currentValue` — The observed value that triggered the alert.
  - `threshold` — The limit that was exceeded.
  - `timestamp` — The time of alert generation.
- **Throws:** `ArgumentException` when `message` is `null` or empty.

### Instance Methods (PerformanceAlert)

#### `ToString`

```csharp
public override string ToString()
```

Returns a formatted string representation of the alert, including its type, message, current value, threshold, and timestamp.

- **Returns:** A string summarizing the alert details.

## Usage

### Example 1: Recording Operations and Retrieving Metrics with Trends

```csharp
var service = new GpuPerformanceMonitoringService();
var operations = new List<PerformanceOperation>
{
    new PerformanceOperation("FilterApply", TimeSpan.FromMilliseconds(45), 1024 * 1024 * 2),
    new PerformanceOperation("ColorCorrect", TimeSpan.FromMilliseconds(12), 1024 * 512)
};

service.RecordOperations(operations);

// Allow time for metrics aggregation
Thread.Sleep(100);

var metricsWithTrends = service.GetMetricsWithTrends();
Console.WriteLine($"CPU change: {metricsWithTrends.CpuChangePercent:F2}%");
Console.WriteLine($"GPU change: {metricsWithTrends.GpuChangePercent:F2}%");
Console.WriteLine($"Throughput change: {metricsWithTrends.ThroughputChangePercent:F2}%");
```

### Example 2: Querying Historical Metrics and Generating Alerts

```csharp
var service = new GpuPerformanceMonitoringService();
var thresholds = new AlertThresholds
{
    MaxCpuPercent = 85.0,
    MaxGpuPercent = 90.0,
    MaxMemoryMb = 4096,
    MaxExecutionTimeMs = 200
};

// Retrieve metrics from the last hour
var recentMetrics = service.GetMetricsInTimeRange(
    DateTime.UtcNow.AddHours(-1),
    DateTime.UtcNow
);

foreach (var metric in recentMetrics)
{
    Console.WriteLine($"[{metric.Timestamp}] CPU: {metric.CpuPercent}%, Memory: {metric.MemoryMb}MB");
}

// Check for threshold violations
var alerts = service.GetPerformanceAlerts(thresholds);
foreach (var alert in alerts)
{
    Console.WriteLine(alert.ToString());
}
```

## Notes

- **Null handling:** `GetMetricsWithTrends` returns a `PerformanceMetricsWithTrends` object whose `Previous` property may be `null` when no prior snapshot exists. All change percentage properties will return `0.0` in that case.
- **Empty collections:** `GetMetricsInTimeRange` returns an empty enumerable when no metrics exist within the specified range; it does not throw.
- **Alert generation:** `GetPerformanceAlerts` returns an empty list when no thresholds are breached. Alerts are evaluated against the current snapshot only, not historical data.
- **Thread safety:** The static extension methods delegate to the underlying `IPerformanceMonitoringService` implementation. Thread safety depends on that implementation. `PerformanceMetricsWithTrends` and `PerformanceAlert` are immutable data transfer objects and are safe for concurrent read access.
- **Timestamp precision:** All timestamps are recorded and compared at UTC millisecond precision. When querying time ranges, boundary inclusivity means metrics exactly at `start` or `end` are included in results.
- **Change percent calculation:** Percentage changes are computed as `((current - previous) / previous) * 100`. When the previous value is zero, the change percent is reported as `0.0` to avoid division-by-zero errors.
