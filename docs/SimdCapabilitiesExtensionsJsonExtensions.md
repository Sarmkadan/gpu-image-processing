# SimdCapabilitiesExtensionsJsonExtensions

Provides JSON serialization helpers for `SimdCapabilitiesExtensions` instances, allowing configuration of which capability flags are included in the serialized output and conversion to/from JSON format.

## API

### IsVectorWidthSupportEnabled
**Purpose**  
Gets or sets a flag indicating whether the vector width support information should be included when serializing a `SimdCapabilitiesExtensions` instance to JSON.

**Parameters**  
None.

**Return Value**  
`true` if vector width support is included in the JSON output; otherwise `false`.

**Exceptions**  
None.

### IsOptimalSimdLevelEnabled
**Purpose**  
Gets or sets a flag indicating whether the optimal SIMD level information should be included when serializing a `SimdCapabilitiesExtensions` instance to JSON.

**Parameters**  
None.

**Return Value**  
`true` if optimal SIMD level is included in the JSON output; otherwise `false`.

**Exceptions**  
None.

### IsSimdAvailabilityEnabled
**Purpose**  
Gets or sets a flag indicating whether SIMD availability information should be included when serializing a `SimdCapabilitiesExtensions` instance to JSON.

**Parameters**  
None.

**Return Value**  
`true` if SIMD availability is included in the JSON output; otherwise `false`.

**Exceptions**  
None.

### IsFriendlyStringEnabled
**Purpose**  
Gets or sets a flag indicating whether a human‑readable friendly string representation of the capabilities should be included when serializing a `SimdCapabilitiesExtensions` instance to JSON.

**Parameters**  
None.

**Return Value**  
`true` if the friendly string is included in the JSON output; otherwise `false`.

**Exceptions**  
None.

### ToJson
**Purpose**  
Serializes a `SimdCapabilitiesExtensions` instance to a JSON string, respecting the current flag settings (`IsVectorWidthSupportEnabled`, `IsOptimalSimdLevelEnabled`, `IsSimdAvailabilityEnabled`, `IsFriendlyStringEnabled`).

**Parameters**  
- `value`: The `SimdCapabilitiesExtensions` instance to serialize.

**Return Value**  
A JSON‑encoded string representing the instance.

**Exceptions**  
- `ArgumentNullException` if `value` is `null`.  
- `JsonSerializationException` if an error occurs during serialization.

### FromJson
**Purpose**  
Deserializes a JSON string into a `SimdCapabilitiesExtensions` instance. Returns `null` if the input cannot be parsed.

**Parameters**  
- `json`: The JSON string to deserialize.

**Return Value**  
A `SimdCapabilitiesExtensions` object populated from the JSON, or `null` if deserialization fails.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`.  
- `JsonException` if the JSON is malformed and cannot be read.

### TryFromJson
**Purpose**  
Attempts to deserialize a JSON string into a `SimdCapabilitiesExtensions` instance, indicating success via a Boolean return value.

**Parameters**  
- `json`: The JSON string to deserialize.  
- `result`: When the method returns `true`, contains the deserialized `SimdCapabilitiesExtensions` object; otherwise `null`.

**Return Value**  
`true` if `json` was successfully parsed; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `json` is `null`.  
(The method does not throw for malformed JSON; instead it returns `false`.)

## Usage

```csharp
using GpuImageProcessing.Json;

// Create a capabilities instance and configure which fields to serialize.
var caps = new SimdCapabilitiesExtensions
{
    // Assume properties are set elsewhere …
};

var jsonExt = new SimdCapabilitiesExtensionsJsonExtensions
{
    IsVectorWidthSupportEnabled = true,
    IsOptimalSimdLevelEnabled   = false,
    IsSimdAvailabilityEnabled   = true,
    IsFriendlyStringEnabled     = true
};

string json = jsonExt.ToJson(caps);
// json now contains a JSON representation that includes vector width,
 // SIMD availability, and the friendly string, but omits the optimal SIMD level.
```

```csharp
using GpuImageProcessing.Json;

string json = @"{""VectorWidth"":256,""IsSimdAvailable"":true,""FriendlyString"":""AVX2""}";

if (SimdCapabilitiesExtensionsJsonExtensions.TryFromJson(json, out var caps))
{
    // Use caps safely …
    Console.WriteLine(caps.FriendlyString);
}
else
{
    // Handle invalid JSON …
    Console.WriteLine("Failed to parse capabilities JSON.");
}
```

## Notes

- The four Boolean properties are instance members; modifying them on one instance does not affect others. If the same `SimdCapabilitiesExtensionsJsonExtensions` instance is shared across threads, concurrent reads/writes to these properties are not thread‑safe; external synchronization is required.
- The static methods `ToJson`, `FromJson`, and `TryFromJson` do not retain any state and are safe to call concurrently from multiple threads.
- `FromJson` returns `null` for empty or whitespace‑only input, as well as for JSON that does not map to the expected schema. Callers should check for `null` before using the result.
- `TryFromJson` follows the common .NET try‑parse pattern: it never throws for malformed JSON; instead it returns `false` and assigns `null` to the output argument. It still throws `ArgumentNullException` for a `null` input to enforce argument validation.
- When all four flags are `false`, `ToJson` will produce an empty JSON object (`{}`). This is a valid representation but may be undesirable depending on consumer expectations.
