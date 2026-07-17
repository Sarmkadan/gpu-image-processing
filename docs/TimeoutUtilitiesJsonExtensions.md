# TimeoutUtilitiesJsonExtensions

Provides JSON serialization and conversion helpers for the `TimeoutConfiguration` type, enabling easy persistence, transmission, and interpretation of timeout values in GPU‑image‑processing workflows.

## API

### ToJson
```csharp
public static string ToJson(this TimeoutConfiguration config)
```
Serializes a `TimeoutConfiguration` instance to its JSON representation.  
- **Parameters**  
  - `config`: The timeout configuration to serialize. If `null`, an `ArgumentNullException` is thrown.  
- **Return value**  
  A JSON string containing the timeout’s millisecond value.  
- **Exceptions**  
  - `ArgumentNullException` if `config` is `null`.  
  - Any exception thrown by the underlying JSON serializer (e.g., `JsonSerializationException`) is propagated.

### FromJson
```csharp
public static TimeoutConfiguration? FromJson(this string json)
```
Deserializes a JSON string into a `TimeoutConfiguration` instance.  
- **Parameters**  
  - `json`: The JSON string to parse. If `null` or empty, the method returns `null`.  
- **Return value**  
  A `TimeoutConfiguration` populated with the value from JSON, or `null` if the input is `null`, empty, or does not contain a valid numeric timeout.  
- **Exceptions**  
  - None; malformed JSON results in a `null` return rather than an exception.

### TryFromJson
```csharp
public static bool TryFromJson(this string json, out TimeoutConfiguration? result)
```
Attempts to parse a JSON string into a `TimeoutConfiguration`, indicating success via the return value.  
- **Parameters**  
  - `json`: The JSON string to parse.  
  - `result`: When the method returns `true`, contains the deserialized `TimeoutConfiguration`; otherwise `null`.  
- **Return value**  
  `true` if `json` is non‑null, non‑empty, and contains a valid numeric timeout; otherwise `false`.  
- **Exceptions**  
  - None; invalid input yields `false` and a `null` output.

### Milliseconds
```csharp
public double Milliseconds { get; }
```
Gets the timeout value expressed in milliseconds.  
- **Return value**  
  A `double` representing the timeout duration. The value is always non‑negative; a value of `0` indicates an immediate timeout.  
- **Exceptions**  
  - None.

### Formatted
```csharp
public string? Formatted { get; }
```
Gets a human‑readable string representation of the timeout (e.g., `"5.00s"` or `"250ms"`).  
- **Return value**  
  A formatted string, or `null` if the underlying `Milliseconds` value is not a finite number.  
- **Exceptions**  
  - None.

### ToTimeSpan
```csharp
public TimeSpan ToTimeSpan()
```
Converts the timeout to a `System.TimeSpan` instance.  
- **Return value**  
  A `TimeSpan` whose total milliseconds equal the `Milliseconds` property.  
- **Exceptions**  
  - None; the conversion is exact for all finite `Milliseconds` values.

## Usage

### Serializing a timeout for storage
```csharp
TimeoutConfiguration timeout = new TimeoutConfiguration { Milliseconds = 1234.5 };
string json = timeout.ToJson(); // {"Milliseconds":1234.5}
// Store or transmit `json` as needed.
```

### Deserializing with fallback
```csharp
string jsonFromCache = GetCachedJson(); // may be null or invalid
if (TimeoutUtilitiesJsonExtensions.TryFromJson(jsonFromCache, out var timeout))
{
    TimeSpan span = timeout.ToTimeSpan();
    ApplyTimeout(span);
}
else
{
    // Use a default timeout when cached data is missing or malformed
    ApplyTimeout(TimeSpan.FromSeconds(30));
}
```

## Notes
- The JSON format produced by `ToJson` and consumed by `FromJson`/`TryFromJson` is a simple object with a single `Milliseconds` property; future extensions to the format will break compatibility unless versioning is added.  
- `FromJson` returns `null` for empty or whitespace‑only input, whereas `TryFromJson` treats the same input as a failure (`false`).  
- All members are pure (no hidden state) and therefore thread‑safe; concurrent calls from multiple threads will not cause race conditions.  
- The `Milliseconds` property accepts any `double` value, including fractional milliseconds; however, extremely large values may exceed the range of `TimeSpan` when converted via `ToTimeSpan`, resulting in an `OverflowException` from the `TimeSpan` constructor. Callers should validate that `Milliseconds` lies within `TimeSpan.MaxValue.TotalMilliseconds` if such a conversion is required.  
- The `Formatted` property returns `null` for non‑finite values (`NaN`, `Infinity`) to avoid producing misleading output.  
- No static state is maintained in `TimeoutUtilitiesJsonExtensions`; thus, the type does not require explicit disposal or synchronization.
