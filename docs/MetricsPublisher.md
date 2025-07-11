# MetricsPublisher

A lightweight utility for publishing metrics to a remote endpoint, enabling real-time monitoring and telemetry collection for GPU image processing workloads. Supports both synchronous and asynchronous operations with configurable formatting and authentication.

## API

### `public MetricsPublisher`

Initializes a new instance of the `MetricsPublisher` class with default settings. The publisher is inactive until endpoints are registered and metrics are explicitly recorded.

### `public void RegisterEndpoint(string url, string apiKey = null)`

Registers a remote endpoint for publishing metrics. The endpoint URL and optional API key are stored for subsequent metric submissions.

- **Parameters**
  - `url` (string): The base URL of the metrics collection service.
  - `apiKey` (string, optional): The authentication key for the endpoint. If omitted, no key is used.
- **Throws**
  - `ArgumentNullException`: If `url` is null or empty.

### `public void RecordMetric(string name, double value, Dictionary<string, string> tags = null)`

Records a single metric value with optional contextual tags. The metric is queued for asynchronous publishing.

- **Parameters**
  - `name` (string): The identifier for the metric (e.g., "gpu-utilization").
  - `value` (double): The numeric value of the metric.
  - `tags` (Dictionary<string, string>, optional): A collection of key-value pairs providing additional context (e.g., `{"gpu", "nvidia-rtx-4090"}`).
- **Throws**
  - `ArgumentNullException`: If `name` is null or empty.
  - `ArgumentException`: If `value` is not a valid number.

### `public void RecordTiming(string name, TimeSpan duration, Dictionary<string, string> tags = null)`

Records a timing metric representing the duration of an operation. The value is converted to milliseconds before publishing.

- **Parameters**
  - `name` (string): The identifier for the timing metric (e.g., "image-processing-time").
  - `duration` (TimeSpan): The elapsed time of the operation.
  - `tags` (Dictionary<string, string>, optional): Contextual metadata for the timing event.
- **Throws**
  - `ArgumentNullException`: If `name` is null or empty.
  - `ArgumentOutOfRangeException`: If `duration` is negative.

### `public async Task FlushAsync()`

Forces an immediate flush of all queued metrics to the registered endpoint. Blocks until all metrics are published or the operation times out.

- **Returns**
  - `Task`: A task representing the asynchronous flush operation.
- **Throws**
  - `InvalidOperationException`: If no endpoint is registered.
  - `HttpRequestException`: If the HTTP request to the endpoint fails.

### `public async ValueTask DisposeAsync()`

Releases all resources used by the `MetricsPublisher`, including flushing any pending metrics and closing network connections. Called automatically when the object is used in a `using` statement.

- **Returns**
  - `ValueTask`: A task representing the asynchronous disposal.

### `public string Url`

Gets the base URL of the registered metrics endpoint. Returns `null` if no endpoint is registered.

### `public MetricsFormat Format`

Gets or sets the serialization format for metrics (e.g., JSON, Prometheus). Defaults to JSON.

### `public string ApiKey`

Gets the API key used for authenticating with the metrics endpoint. Returns `null` if no key is set.

### `public bool IsActive`

Indicates whether the publisher is actively recording and publishing metrics. Set to `false` to temporarily disable metric collection.

### `public DateTime RegisteredAt`

Gets the timestamp when the endpoint was registered. Returns `DateTime.MinValue` if no endpoint is registered.

### `public string Name`

Gets or sets the name of the metrics stream (e.g., "gpu-image-processing-worker"). Used for identification in the metrics service.

### `public double Value`

Gets or sets the numeric value of the current metric. Used in conjunction with `RecordMetric` for ad-hoc metric publishing.

### `public Dictionary<string, string> Tags`

Gets or sets a collection of key-value pairs providing contextual metadata for the current metric. Used in conjunction with `RecordMetric` for ad-hoc metric publishing.

### `public DateTime Timestamp`

Gets or sets the timestamp for the current metric. Defaults to `DateTime.UtcNow` when recording metrics.

## Usage

### Example 1: Basic Metric Publishing
