# ConfigurationUtilitiesJsonExtensions

Provides extension methods for serializing and deserializing configuration objects to and from JSON, along with utility properties for environment-specific paths and runtime settings used in GPU image processing pipelines.

## API

### `ToJson`
Serializes the current configuration object to a JSON string.

```csharp
public static string ToJson(this ConfigurationUtilitiesJsonExtensions config)
```

**Returns**
A JSON string representation of the configuration.

**Throws**
`ArgumentNullException` if `config` is `null`.

---

### `FromJson`
Deserializes a JSON string into a new configuration object.

```csharp
public static void FromJson(this ConfigurationUtilitiesJsonExtensions config, string json)
```

**Parameters**
- `json`: The JSON string to deserialize.

**Throws**
- `ArgumentNullException` if `json` is `null`.
- `JsonException` if the JSON is malformed or cannot be mapped to the configuration type.

---

### `TryFromJson`
Attempts to deserialize a JSON string into a new configuration object. Returns `true` if successful, otherwise `false`.

```csharp
public static bool TryFromJson(this ConfigurationUtilitiesJsonExtensions config, string json, out ConfigurationUtilitiesJsonExtensions? result)
```

**Parameters**
- `json`: The JSON string to deserialize.
- `result`: When this method returns, contains the deserialized configuration object if successful; otherwise, `null`.

**Returns**
`true` if deserialization succeeds; otherwise, `false`.

**Throws**
`ArgumentNullException` if `json` is `null`.

---

### `Environment`
Gets or sets the environment identifier (e.g., "Development", "Production").

```csharp
public string? Environment { get; set; }
```

**Remarks**
Used to differentiate behavior across deployment environments.

---

### `DataDirectory`
Gets or sets the root directory for data storage.

```csharp
public string? DataDirectory { get; set; }
```

**Remarks**
Default paths for input/output files may be resolved relative to this directory.

---

### `LogDirectory`
Gets or sets the directory where log files are written.

```csharp
public string? LogDirectory { get; set; }
```

**Remarks**
If `null`, logging may fall back to a default location or be disabled.

---

### `TempDirectory`
Gets or sets the directory for temporary file storage.

```csharp
public string? TempDirectory { get; set; }
```

**Remarks**
Used for intermediate files during image processing.

---

### `MaxConcurrentOperations`
Gets or sets the maximum number of concurrent GPU operations allowed.

```csharp
public int MaxConcurrentOperations { get; set; }
```

**Remarks**
Affects parallelism and resource allocation.

---

### `OperationTimeoutSeconds`
Gets or sets the timeout in seconds for GPU operations.

```csharp
public int OperationTimeoutSeconds { get; set; }
```

**Remarks**
Prevents indefinite hangs during processing.

---

### `PreferredDeviceId`
Gets or sets the preferred GPU device identifier.

```csharp
public int PreferredDeviceId { get; set; }
```

**Remarks**
Used to select a specific GPU when multiple are available.

---
### `LogLevel`
Gets or sets the minimum log severity level.

```csharp
public string? LogLevel { get; set; }
```

**Remarks**
Accepts values like "Debug", "Info", "Warning", "Error".

---
### `EnablePerformanceLogging`
Gets or sets a value indicating whether performance metrics are logged.

```csharp
public bool EnablePerformanceLogging { get; set; }
```

---
### `EnableDebugLogging`
Gets or sets a value indicating whether debug-level logs are enabled.

```csharp
public bool EnableDebugLogging { get; set; }
```

---
### `CacheSizeMb`
Gets or sets the maximum GPU cache size in megabytes.

```csharp
public int CacheSizeMb { get; set; }
```

---
### `UseGpuAcceleration`
Gets or sets a value indicating whether GPU acceleration is enabled.

```csharp
public bool UseGpuAcceleration { get; set; }
```

---
### `DefaultProfile`
Gets or sets the name of the default image processing profile.

```csharp
public string? DefaultProfile { get; set; }
```

**Remarks**
Used when no profile is explicitly specified.

## Usage

### Serializing Configuration to JSON
```csharp
var config = new ConfigurationUtilitiesJsonExtensions
{
    Environment = "Production",
    DataDirectory = "/var/data",
    LogDirectory = "/var/log",
    MaxConcurrentOperations = 4,
    PreferredDeviceId = 0,
    LogLevel = "Info",
    UseGpuAcceleration = true,
    DefaultProfile = "high_quality"
};

string json = config.ToJson();
File.WriteAllText("config.json", json);
```

### Deserializing Configuration from JSON
```csharp
string json = File.ReadAllText("config.json");
var config = new ConfigurationUtilitiesJsonExtensions();
config.FromJson(json);

Console.WriteLine($"Environment: {config.Environment}");
Console.WriteLine($"Max concurrent operations: {config.MaxConcurrentOperations}");
```

## Notes

- The `ToJson`, `FromJson`, and `TryFromJson` methods are thread-safe for concurrent calls on different instances. However, modifying the same instance from multiple threads without synchronization may lead to inconsistent state.
- `FromJson` throws on invalid JSON; use `TryFromJson` to avoid exceptions during deserialization in untrusted input scenarios.
- Path properties (`DataDirectory`, `LogDirectory`, `TempDirectory`) are not validated by these methods. Ensure directories exist and are writable before use.
- Numeric properties (`MaxConcurrentOperations`, `OperationTimeoutSeconds`, etc.) are not range-validated by the JSON methods. Invalid values may cause runtime errors during processing.
