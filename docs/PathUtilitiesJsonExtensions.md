# PathUtilitiesJsonExtensions

Provides JSON serialization and deserialization extensions for `PathUtilitiesConfiguration`, enabling configuration persistence and cross-platform path handling.

## API

### `public static string ToJson(PathUtilitiesConfiguration configuration)`

Serializes a `PathUtilitiesConfiguration` instance to a JSON string.

- **Parameters**
  - `configuration`: The configuration to serialize.
- **Returns**
  - A JSON string representation of the configuration.
- **Throws**
  - `ArgumentNullException`: If `configuration` is `null`.

---

### `public static PathUtilitiesConfiguration? FromJson(string json)`

Deserializes a JSON string into a `PathUtilitiesConfiguration` instance.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - The deserialized `PathUtilitiesConfiguration`, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---

### `public static bool TryFromJson(string json, out PathUtilitiesConfiguration? configuration)`

Attempts to deserialize a JSON string into a `PathUtilitiesConfiguration` instance without throwing exceptions.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `configuration`: Outputs the deserialized configuration, or `null` on failure.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `public string[] SupportedExtensions { get; }`

Gets the file extensions supported by the path utilities configuration.

- **Returns**
  - An array of supported file extensions (e.g., `".png"`, `".jpg"`).

---
### `public bool DefaultPathNormalization { get; }`

Indicates whether default path normalization is enabled.

- **Returns**
  - `true` if default path normalization is enabled; otherwise, `false`.

---
### `public bool CrossPlatformSupport { get; }`

Indicates whether cross-platform path handling is enabled.

- **Returns**
  - `true` if cross-platform support is enabled; otherwise, `false`.

## Usage

### Serializing a configuration

```csharp
var config = new PathUtilitiesConfiguration
{
    SupportedExtensions = new[] { ".png", ".jpg", ".jpeg" },
    DefaultPathNormalization = true,
    CrossPlatformSupport = true
};

string json = PathUtilitiesJsonExtensions.ToJson(config);
Console.WriteLine(json);
```

### Deserializing a configuration

```csharp
string json = """
{
    "SupportedExtensions": [".png", ".jpg"],
    "DefaultPathNormalization": true,
    "CrossPlatformSupport": false
}
""";

if (PathUtilitiesJsonExtensions.TryFromJson(json, out var config))
{
    Console.WriteLine($"Extensions: {string.Join(", ", config.SupportedExtensions)}");
}
else
{
    Console.WriteLine("Failed to deserialize configuration.");
}
```

## Notes

- **Thread Safety**: All methods are thread-safe as they operate on immutable data or perform atomic operations.
- **Null Handling**: Methods explicitly guard against `null` inputs; passing `null` will throw `ArgumentNullException`.
- **Deserialization Failures**: `FromJson` returns `null` on failure, while `TryFromJson` provides a non-exceptional path. Use `TryFromJson` in performance-sensitive or high-reliability contexts.
- **Cross-Platform Paths**: When `CrossPlatformSupport` is `true`, paths are normalized to use forward slashes (`/`) on all platforms. Disable this for platform-specific path handling.
