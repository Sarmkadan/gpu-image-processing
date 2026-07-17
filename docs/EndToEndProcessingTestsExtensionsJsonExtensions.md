# EndToEndProcessingTestsExtensionsJsonExtensions

Provides JSON serialization and deserialization extension methods for the `EndToEndProcessingTestsExtensions` type, along with three boolean instance properties that control which test methods are enabled. This static class is designed to convert an `EndToEndProcessingTestsExtensions` object to and from its JSON representation, supporting case‑insensitive property matching and automatic trimming of input strings.

## API

### `IsRunAllTestsEnabled` (instance property)

```csharp
public bool IsRunAllTestsEnabled { get; }
```

Gets a value indicating whether the "run all tests" mode is enabled.

- **Returns**: `true` if all end‑to‑end tests should be executed; otherwise `false`.

### `IsGetTestMethodNamesEnabled` (instance property)

```csharp
public bool IsGetTestMethodNamesEnabled { get; }
```

Gets a value indicating whether the retrieval of test method names is enabled.

- **Returns**: `true` if test method names can be enumerated; otherwise `false`.

### `IsAllTestsPassEnabled` (instance property)

```csharp
public bool IsAllTestsPassEnabled { get; }
```

Gets a value indicating whether the "all tests pass" check is enabled.

- **Returns**: `true` if the pass condition for all tests is active; otherwise `false`.

### `ToJson` (static method)

```csharp
public static string ToJson(this EndToEndProcessingTestsExtensions extensions)
```

Serializes the specified `EndToEndProcessingTestsExtensions` instance to a JSON string.

- **Parameters**:
  - `extensions` – The object to serialize. Must not be `null`.
- **Returns**: A JSON string representing the object.
- **Throws**: `ArgumentNullException` if `extensions` is `null`.

### `FromJson` (static method)

```csharp
public static EndToEndProcessingTestsExtensions? FromJson(string json)
```

Deserializes a JSON string into an `EndToEndProcessingTestsExtensions` instance. The input is trimmed before parsing, and property matching is case‑insensitive.

- **Parameters**:
  - `json` – The JSON string to deserialize. Must not be `null` or empty.
- **Returns**: A new `EndToEndProcessingTestsExtensions` instance if deserialization succeeds; `null` if the JSON is invalid or cannot be mapped.
- **Throws**: `ArgumentNullException` if `json` is `null`. `ArgumentException` if `json` is empty or consists only of whitespace after trimming.

### `TryFromJson` (static method)

```csharp
public static bool TryFromJson(string json, out EndToEndProcessingTestsExtensions? result)
```

Attempts to deserialize a JSON string into an `EndToEndProcessingTestsExtensions` instance. The input is trimmed before parsing, and property matching is case‑insensitive.

- **Parameters**:
  - `json` – The JSON string to deserialize. Must not be `null` or empty.
  - `result` – When this method returns, contains the deserialized object if successful, or `null` if deserialization failed.
- **Returns**: `true` if deserialization succeeded; `false` otherwise.
- **Throws**: `ArgumentNullException` if `json` is `null`. `ArgumentException` if `json` is empty or consists only of whitespace after trimming.

## Usage

### Example 1: Serialize and deserialize with `ToJson` / `FromJson`

```csharp
using GpuImageProcessing;

var config = new EndToEndProcessingTestsExtensions
{
    IsRunAllTestsEnabled = true,
    IsGetTestMethodNamesEnabled = false,
    IsAllTestsPassEnabled = true
};

// Serialize to JSON
string json = config.ToJson();
Console.WriteLine(json);
// Output: {"IsRunAllTestsEnabled":true,"IsGetTestMethodNamesEnabled":false,"IsAllTestsPassEnabled":true}

// Deserialize back
var restored = EndToEndProcessingTestsExtensionsJsonExtensions.FromJson(json);
if (restored != null)
{
    Console.WriteLine(restored.IsRunAllTestsEnabled); // True
}
```

### Example 2: Safe deserialization with `TryFromJson`

```csharp
using GpuImageProcessing;

string invalidJson = "{ \"IsRunAllTestsEnabled\": \"notabool\" }";

if (EndToEndProcessingTestsExtensionsJsonExtensions.TryFromJson(invalidJson, out var result))
{
    Console.WriteLine("Deserialization succeeded.");
}
else
{
    Console.WriteLine("Deserialization failed. Result is null.");
    // Output: Deserialization failed. Result is null.
}

// Valid JSON with case‑insensitive property names
string validJson = "{ \"isrunalltestsenabled\": true, \"isgettestmethodnamesenabled\": false, \"isalltestspassenabled\": true }";
if (EndToEndProcessingTestsExtensionsJsonExtensions.TryFromJson(validJson, out var config))
{
    Console.WriteLine(config.IsRunAllTestsEnabled); // True
}
```

## Notes

- All static methods are thread‑safe; they do not modify any shared state. The instance properties (`IsRunAllTestsEnabled`, `IsGetTestMethodNamesEnabled`, `IsAllTestsPassEnabled`) belong to the `EndToEndProcessingTestsExtensions` object and are not inherently thread‑safe.
- Input JSON strings are trimmed of leading and trailing whitespace before parsing. An empty string after trimming causes an `ArgumentException`.
- Property name matching during deserialization is case‑insensitive. For example, `"isrunalltestsenabled"` and `"IsRunAllTestsEnabled"` are treated equivalently.
- If the JSON contains properties that do not exist on `EndToEndProcessingTestsExtensions`, they are silently ignored.
- `FromJson` returns `null` when the JSON is structurally valid but cannot be mapped (e.g., type mismatch). `TryFromJson` returns `false` in the same scenario and sets `result` to `null`.
- Both `FromJson` and `TryFromJson` throw `ArgumentNullException` for a `null` input and `ArgumentException` for an empty or whitespace‑only input.
