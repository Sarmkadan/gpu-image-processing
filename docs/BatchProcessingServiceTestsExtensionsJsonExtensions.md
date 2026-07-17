# BatchProcessingServiceTestsExtensionsJsonExtensions

Provides serialization and deserialization extensions for the `BatchProcessingServiceTestsExtensions` type, enabling conversion to and from JSON for test configuration persistence and inter-process communication. This utility class supports test automation scenarios where batch processing parameters need to be stored, retrieved, or shared across test runs.

## API

### `public int DefaultImageCount`
Gets or sets the default number of images to process in a batch during test execution. This value is used when no explicit image count is specified in test configurations.

### `public int DefaultFilterCount`
Gets or sets the default number of filters to apply to each image during test execution. This value is used when no explicit filter count is provided in test configurations.

### `public bool EnableVerboseOutput`
Gets or sets a flag indicating whether verbose logging should be enabled during test execution. When `true`, additional diagnostic information is emitted to aid in debugging.

### `public static string ToJson(this BatchProcessingServiceTestsExtensions source)`
Serializes the given `BatchProcessingServiceTestsExtensions` instance to a JSON string.

**Parameters:**
- `source`: The instance to serialize. Must not be `null`.

**Returns:**
A JSON-formatted string representing the serialized object.

**Throws:**
- `ArgumentNullException`: Thrown if `source` is `null`.

### `public static BatchProcessingServiceTestsExtensions? FromJson(string json)`
Deserializes a JSON string into a `BatchProcessingServiceTestsExtensions` instance.

**Parameters:**
- `json`: The JSON string to deserialize. Must not be `null` or empty.

**Returns:**
A `BatchProcessingServiceTestsExtensions` instance if deserialization succeeds; otherwise, `null`.

**Throws:**
- `ArgumentException`: Thrown if `json` is `null` or whitespace.
- `JsonException`: Thrown if the JSON is malformed or does not conform to the expected schema.

### `public static bool TryFromJson(string json, out BatchProcessingServiceTestsExtensions? result)`
Attempts to deserialize a JSON string into a `BatchProcessingServiceTestsExtensions` instance without throwing exceptions.

**Parameters:**
- `json`: The JSON string to deserialize. Must not be `null` or empty.
- `result`: Output parameter containing the deserialized instance if successful; otherwise, `null`.

**Returns:**
`true` if deserialization succeeds; otherwise, `false`.

**Remarks:**
- Does not throw exceptions for invalid input. Returns `false` instead.

## Usage

### Example 1: Serializing Test Configuration
