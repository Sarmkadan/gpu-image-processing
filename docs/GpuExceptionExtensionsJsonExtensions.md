# GpuExceptionExtensionsJsonExtensions

Provides JSON serialization and deserialization utilities for GPU exception detection configuration. This class enables round-tripping of `GpuExceptionExtensionsConfig` objects to and from JSON strings with configurable formatting.

## API

### `Type` (property)

The type discriminator for the configuration object.

- **Type**: `string`
- **Access**: Read-only (get-only property)
- **Value**: Always returns `"GpuExceptionExtensions"`


### `IsTimeoutDetectionEnabled` (property)

Controls whether timeout detection is enabled for GPU operations.

- **Type**: `bool`
- **Access**: Read-only (init-only property)
- **Default**: `true`
- **Description**: When `true`, the GPU runtime will monitor for operation timeouts and raise exceptions when detected.


### `IsMemoryDetectionEnabled` (property)

Controls whether memory detection is enabled for GPU operations.

- **Type**: `bool`
- **Access**: Read-only (init-only property)
- **Default**: `true`
- **Description**: When `true`, the GPU runtime will monitor for memory-related issues (leaks, over-allocation) and raise exceptions when detected.


### `IsComputePipelineDetectionEnabled` (property)

Controls whether compute pipeline detection is enabled for GPU operations.

- **Type**: `bool`
- **Access**: Read-only (init-only property)
- **Default**: `true`
- **Description**: When `true`, the GPU runtime will monitor for compute pipeline errors and raise exceptions when detected.


### `ToJson(GpuExceptionExtensionsConfig config, bool indented = false)` (method)


Serializes a `GpuExceptionExtensionsConfig` object to a JSON string.


- **Parameters**:
  - `config`: The configuration to serialize. Must not be null.
  - `indented`: Whether to format the JSON with indentation for readability. Defaults to `false`.
- **Returns**: A JSON string representing the configuration.
- **Throws**:
  - `ArgumentNullException`: If `config` is null.
- **Thread-safety**: Safe for concurrent calls.
- **Example output**: `{"type":"GpuExceptionExtensions","isTimeoutDetectionEnabled":true,"isMemoryDetectionEnabled":true,"isComputePipelineDetectionEnabled":true}`


### `FromJson(string json)` (method)

Deserializes a JSON string to a `GpuExceptionExtensionsConfig` object.

- **Parameters**:
  - `json`: The JSON string to deserialize. Can be null or empty.
- **Returns**:
  - The deserialized configuration if successful.
  - `null` if the input JSON is null or empty.
- **Throws**:
  - `JsonException`: If the JSON is invalid or cannot be deserialized.
- **Thread-safety**: Safe for concurrent calls.

### `TryFromJson(string json, out GpuExceptionExtensionsConfig? config)` (method)

Attempts to deserialize a JSON string to a `GpuExceptionExtensionsConfig` object without throwing exceptions.

- **Parameters**:
  - `json`: The JSON string to deserialize. Can be null or empty.
  - `config`: Receives the deserialized configuration if successful.
- **Returns**:
  - `true` if deserialization succeeded.
  - `false` if deserialization failed.
- **Post-conditions**:
  - If successful, `config` contains the deserialized object.
  - If failed, `config` is set to `null`.
- **Thread-safety**: Safe for concurrent calls.

## Usage

### Serializing configuration to JSON

```csharp
using GpuImageProcessing.Core;

var config = new GpuExceptionExtensionsConfig
{
    IsTimeoutDetectionEnabled = true,
    IsMemoryDetectionEnabled = false,
    IsComputePipelineDetectionEnabled = true
};

string json = GpuExceptionExtensionsJsonExtensions.ToJson(config, indented: true);
// Result:
// {
//   "type": "GpuExceptionExtensions",
//   "isTimeoutDetectionEnabled": true,
//   "isMemoryDetectionEnabled": false,
//   "isComputePipelineDetectionEnabled": true
// }
```

### Deserializing configuration from JSON

```csharp
using GpuImageProcessing.Core;

string json = "{\"type\":\"GpuExceptionExtensions\",\"isTimeoutDetectionEnabled\":true,\"isMemoryDetectionEnabled\":false,\"isComputePipelineDetectionEnabled\":true}";

var config = GpuExceptionExtensionsJsonExtensions.FromJson(json);
if (config != null)
{
    Console.WriteLine($"Timeout detection enabled: {config.IsTimeoutDetectionEnabled}");
    Console.WriteLine($"Memory detection enabled: {config.IsMemoryDetectionEnabled}");
}
```

## Notes

- **Thread-safety**: All public members are thread-safe and can be called concurrently from multiple threads.
- **Null handling**: `FromJson` returns `null` for null or empty input, while `TryFromJson` returns `true` with `config` set to `null` for the same input.
- **Error handling**: `FromJson` throws `JsonException` on invalid JSON, while `TryFromJson` returns `false` without throwing.
- **CamelCase serialization**: JSON properties are serialized using camelCase naming convention (e.g., `IsTimeoutDetectionEnabled` becomes `isTimeoutDetectionEnabled`).
- **Type discriminator**: The `Type` property always returns `"GpuExceptionExtensions"` and is used to identify the configuration type during deserialization.
- **Immutable configuration**: The `GpuExceptionExtensionsConfig` class uses init-only setters, making instances immutable after construction.