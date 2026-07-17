# ImageProcessingConfiguration

Overview of the configuration object that governs limits and performance thresholds for image processing operations in the GPU‑image‑processing library. Instances hold constraints on image dimensions and a threshold for detecting slow‑running operations, and they provide built‑in JSON serialization helpers.

## API

### MinImageWidth
**Purpose** – Specifies the minimum permissible width (in pixels) for images that will be processed.  
**Parameters** – None.  
**Return value** – The current minimum width as an `int`.  
**Exceptions** – Setting the field to a negative value may cause downstream validation to throw an `ArgumentOutOfRangeException` during processing; the field itself does not throw.

### MaxImageWidth
**Purpose** – Specifies the maximum permissible width (in pixels) for images that will be processed.  
**Parameters** – None.  
**Return value** – The current maximum width as an `int`.  
**Exceptions** – Assigning a value less than `MinImageWidth` or a negative value) can result in processing; the field does not throw.

### MinImageHeight
**Purpose** – Specifies maximum width as an `int`.  
**Exceptions** – Setting the field to a value less than `MinImageWidth` or to a negative number may result in an `ArgumentOutOfRangeException` when the configuration is used.

### MinImageHeight
**Purpose** – Specifies the minimum permissible height (in pixels) for images that will be processed.  
**Parameters** – None.  
**Return value** – The current minimum height as an `int`.  
**Exceptions** – Assigning a negative value may lead to an `ArgumentOutOfRangeException` during validation.

### MaxImageHeight
**Purpose** – Specifies the maximum permissible height (in pixels) for images that will be processed.  
**Parameters** – None.  
**Return value** – The current maximum height as an `int`.  
**Exceptions** – Setting a value below `MinImageHeight` or a negative value may cause an `ArgumentOutOfRangeException` when the configuration is applied.

### SlowOperationThresholdMs
**Purpose** – Defines the time (in milliseconds) after which an operation is considered “slow” and may trigger logging or fallback behavior.  
**Parameters** – None.  
**Return value** – The threshold value as an `int`.  
**Exceptions** – Setting a negative value may cause the library to treat every operation as slow; no exception is thrown by the field itself.

### ToJson
**Purpose** – Serializes the current instance to a JSON string.  
**Parameters** – None.  
**Return value** – A JSON‑encoded string representing the configuration.  
**Exceptions** – Throws a `JsonException` if serialization fails (e.g., due to circular references, which should not occur with this type).

### FromJson
**Purpose** – Deserializes a JSON string into a new `ImageProcessingConfiguration` instance.  
**Parameters** – `json`: The JSON string to parse.  
**Return value** – A new `ImageProcessingConfiguration` populated with the values from `json`, or `null` if the input is `null` or invalid.  
**Exceptions** – Throws an `ArgumentNullException` if `json` is `null`. Throws a `JsonException` if the JSON is malformed or does not match the expected schema.

### TryFromJson
**Purpose** – Attempts to parse a JSON string into an `ImageProcessingConfiguration` without throwing exceptions on failure.  
**Parameters** – `json`: The JSON string to parse. `result`: When the method returns `true`, receives the deserialized configuration; otherwise receives `null`.  
**Return value** – `true` if parsing succeeded; `false` if the input is `null`, malformed, or does not conform to the expected schema.  
**Exceptions** – None; all error conditions are reported via the return value.

## Usage

### Example 1: Creating, configuring, and serializing
```csharp
using GpuImageProcessing;

// Create a default configuration
var config = new ImageProcessingConfiguration
{
    MinImageWidth = 64,
    MaxImageWidth = 4096,
    MinImageHeight = 64,
    MaxImageHeight = 4096,
    SlowOperationThresholdMs = 200
};

// Serialize to JSON for storage or transmission
string json = config.ToJson();
// json now contains something like:
// {"MinImageWidth":64,"MaxImageWidth":4096,"MinImageHeight":64,"MaxImageHeight":4096,"SlowOperationThresholdMs":200}

// Later, deserialize back into an object
ImageProcessingConfiguration? restored = ImageProcessingConfiguration.FromJson(json);
// restored holds the same values as config
```

### Example 2: Safe parsing with TryFromJson
```csharp
using GpuImageProcessing;

string userSuppliedJson = GetJsonFromUser(); // could be invalid or empty

if (ImageProcessingConfiguration.TryFromJson(userSuppliedJson, out var config))
{
    // Use config safely
    ProcessImage(image, config);
}
else
{
    // Fallback to defaults or notify the user
    config = new ImageProcessingConfiguration
    {
        MinImageWidth = 128,
        MaxImageWidth = 2048,
        MinImageHeight = 128,
        MaxImageHeight = 2048,
        SlowOperationThresholdMs = 150
    };
    Log.Warning("Invalid configuration JSON supplied; using defaults.");
}
```

## Notes
- The mutable fields (`MinImageWidth`, `MaxImageWidth`, `MinImageHeight`, `MaxImageHeight`, `SlowOperationThresholdMs`) are not thread‑safe. If the same instance is accessed concurrently from multiple threads, external synchronization is required.
- The static JSON methods (`ToJson`, `FromJson`, `TryFromJson`) are thread‑safe; they rely only on their input parameters and do not modify shared state.
- Setting a minimum greater than its corresponding maximum (e.g., `MinImageWidth > MaxImageWidth`) does not cause an immediate exception, but subsequent processing steps may reject images or throw validation errors. It is the caller’s responsibility to maintain a coherent range.
- Negative values for any dimension or the threshold are technically allowed by the type but will generally lead to undefined behavior or exceptions during image validation; treat them as invalid inputs.
- `FromJson` returns `null` for null or unparseable input, whereas `TryFromJson` returns `false` and populates the output parameter with `null` in the same scenarios. Choose the method that matches your error‑handling preference.
