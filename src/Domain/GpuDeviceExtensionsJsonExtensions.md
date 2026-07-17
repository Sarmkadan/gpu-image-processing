# GpuDeviceExtensionsJsonExtensions

Provides System.Text.Json serialization and deserialization helpers for the `GpuDeviceExtensionsConfig` configuration class, enabling JSON round-tripping of GPU device extension settings.

## API

### Type

- **Type**: `public string`
- **Description**: Gets the type identifier for this configuration. Always returns the constant value `"GpuDeviceExtensions"`.



### IsMemoryExtensionsEnabled

- **Type**: `public bool`
- **Description**: Gets or sets whether memory extension methods are enabled for GPU device operations.
- **Default**: `true`



### IsColorSpaceDetectionEnabled

- **Type**: `public bool`
- **Description**: Gets or sets whether color space detection is enabled for GPU device operations.
- **Default**: `true`



### IsDeviceTypeDisplayEnabled

- **Type**: `public bool`
- **Description**: Gets or sets whether device type display name formatting is enabled for GPU device operations.
- **Default**: `true`


### DefaultMemoryUnit

- **Type**: `public string`
- **Description**: Gets or sets the default memory unit used for extension method outputs (e.g., `"MB"`, `"GB"`).
- **Default**: `"MB"`


### ToJson

- **Signature**: `public static string ToJson(this GpuDeviceExtensionsConfig value, bool indented = false)`
- **Purpose**: Serializes a `GpuDeviceExtensionsConfig` instance to a JSON string.
- **Parameters**:
  - `value`: The configuration to serialize. Must not be null.
  - `indented`: If true, formats the JSON with indentation for readability; otherwise, produces compact JSON.
- **Return value**: A JSON string representing the configuration.
- **Exceptions**: Throws `ArgumentNullException` if `value` is null.


### FromJson

- **Signature**: `public static GpuDeviceExtensionsConfig? FromJson(string json)`
- **Purpose**: Deserializes a JSON string to a `GpuDeviceExtensionsConfig` instance.
- **Parameters**:
  - `json`: The JSON string to deserialize. If null or empty, returns null.
- **Return value**: The deserialized configuration, or null if the input is null or empty.
- **Exceptions**: Throws `JsonException` if the JSON is invalid or cannot be deserialized.


### TryFromJson

- **Signature**: `public static bool TryFromJson(string json, out GpuDeviceExtensionsConfig? value)`
- **Purpose**: Attempts to deserialize a JSON string to a `GpuDeviceExtensionsConfig` instance without throwing exceptions.
- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `value`: Receives the deserialized configuration if successful.
- **Return value**: Returns true if deserialization succeeded; otherwise, false.
- **Output**: Sets `value` to the deserialized configuration on success, or null on failure.


## Usage

### Serializing a configuration

```csharp
var config = new GpuDeviceExtensionsConfig
{
    IsMemoryExtensionsEnabled = false,
    IsColorSpaceDetectionEnabled = true,
    DefaultMemoryUnit = "GB"
};

string jsonCompact = config.ToJson(); // Compact JSON
string jsonIndented = config.ToJson(indented: true); // Pretty-printed JSON
```

### Deserializing a configuration

```csharp
string json = @"{
  "type": "GpuDeviceExtensions",
  "isMemoryExtensionsEnabled": false,
  "isColorSpaceDetectionEnabled": true,
  "isDeviceTypeDisplayEnabled": true,
  "defaultMemoryUnit": "GB"
}";

// Safe deserialization with null handling
var config = GpuDeviceExtensionsJsonExtensions.FromJson(json);
if (config != null)
{
    Console.WriteLine($"Loaded config: {config.Type}, Memory unit: {config.DefaultMemoryUnit}");
}

// Safe deserialization with error handling
if (GpuDeviceExtensionsJsonExtensions.TryFromJson(json, out var parsedConfig))
{
    Console.WriteLine("Successfully parsed configuration");
}
else
{
    Console.WriteLine("Failed to parse configuration");
}
```

## Notes

- The class uses camelCase property naming policy for JSON serialization to match JavaScript conventions.
- `FromJson` returns null for empty or null input rather than throwing, making it suitable for optional configuration scenarios.
- `TryFromJson` provides a non-throwing alternative for parsing untrusted JSON input.
- The serialization methods are thread-safe as they do not mutate shared state; each call creates a new `JsonSerializerOptions` instance when indentation is requested.
- JSON parsing errors from `FromJson` and `TryFromJson` are surfaced as `JsonException` without additional wrapping, preserving the underlying error details for debugging.