# MetricsPublisherStatsExtensions

The `MetricsPublisherStatsExtensions` class provides extension methods for enhancing a `MetricsPublisher` with Prometheus formatting and metric validation capabilities.

## API

### Constructor
`MetricsPublisherStatsExtensions(MetricsPublisher publisher)`
- **publisher**: The metrics publisher to extend.

### Methods
#### `FormatMetricAsPrometheusLine(string name, double value, Dictionary<string, string> tags)`
Formats the metric as a Prometheus exposition line.
- **name**: The name of the metric.
- **value**: The value of the metric.
- **tags**: The tags associated with the metric.

#### `ValidateMetricValue(string name, double value, Dictionary<string, string> tags)`
Validates and clamps the metric value.
- **name**: The name of the metric.
- **value**: The value of the metric.
- **tags**: The tags associated with the metric.