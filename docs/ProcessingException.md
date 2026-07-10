# ProcessingException

The `ProcessingException` class represents an error that occurs during GPU-accelerated image processing operations. It provides structured context about the failure, including the source image, the filter being applied, and any invalid configuration parameters. This exception is typically thrown by image processing pipelines when an operation cannot be completed due to invalid input, unsupported filter combinations, or transient processing failures.

## API

### Constructors

#### `public ProcessingException()`

Initializes a new instance of the `ProcessingException` class with default property values. All nullable properties are set to `null`.

#### `public ProcessingException(string? imagePath, string? filterName, int? attemptNumber, string? filterType, string[]? invalidParameters, string? imageFormat)`

Initializes a new instance of the `ProcessingException` class and populates all contextual properties from the provided arguments. This constructor is the primary way to create a fully detailed exception.

**Parameters:**

- `imagePath` – The file path of the image being processed when the error occurred.
- `filterName` – The name of the filter that was being applied.
- `attemptNumber` – The zero-based attempt number if the operation was retried.
- `filterType` – The type or category of the filter (e.g., "convolution", "morphological").
- `invalidParameters` – An array of parameter names that were found to be invalid.
- `imageFormat` – The format of the image (e.g., "PNG", "JPEG").

**Throws:**  
No exceptions are thrown by this constructor.

### Properties

#### `public string? ImagePath { get; }`

Gets the file path of the image that was being processed when the exception occurred. May be `null` if the image was not loaded from a file.

#### `public string? FilterName { get; }`

Gets the name of the filter that was active at the time of the failure. `null` if the error is not filter-specific.

#### `public int? AttemptNumber { get; }`

Gets the zero-based attempt number for retried operations. `null` if the operation was not retried or the attempt number is unknown.

#### `public string? FilterType { get; }`

Gets the type or category of the filter (e.g., "blur", "edge detection"). `null` if the filter type is not applicable.

#### `public string[]? InvalidParameters { get; }`

Gets an array of parameter names that were determined to be invalid or out of range. `null` if no parameters were identified as invalid.

#### `public string? ImageFormat { get; }`

Gets the format of the image (e.g., "BMP", "TIFF"). `null` if the format is unknown or not applicable.

## Usage

### Example 1: Catching and inspecting a processing exception

```csharp
using GpuImageProcessing;

try
{
    var processor = new ImageProcessor();
    processor.ApplyFilter("input.png", "sharpen", new Dictionary<string, object>
    {
        ["intensity"] = 1.5,
        ["radius"] = -1   // invalid value
    });
}
catch (ProcessingException ex)
{
    Console.WriteLine($"Error processing {ex.ImagePath ?? "unknown"}");
    Console.WriteLine($"Filter: {ex.FilterName} (type: {ex.FilterType})");
    if (ex.InvalidParameters is { Length: > 0 })
    {
        Console.WriteLine("Invalid parameters: " + string.Join(", ", ex.InvalidParameters));
    }
    Console.WriteLine($"Attempt: {ex.AttemptNumber?.ToString() ?? "N/A"}");
}
```

### Example 2: Throwing a detailed processing exception

```csharp
public void ApplyCustomFilter(string imagePath, string filterName, int attempt)
{
    try
    {
        // ... processing logic ...
    }
    catch (InvalidOperationException inner)
    {
        throw new ProcessingException(
            imagePath: imagePath,
            filterName: filterName,
            attemptNumber: attempt,
            filterType: "custom",
            invalidParameters: new[] { "threshold" },
            imageFormat: "PNG"
        );
    }
}
```

## Notes

- All properties are read-only after construction. To modify the exception context, create a new instance with the desired values.
- The `InvalidParameters` array, if non-null, is not copied defensively. Callers should not modify the array after passing it to the constructor.
- Thread safety: Instances of `ProcessingException` are immutable after construction. Reading properties from multiple threads concurrently is safe. However, the `InvalidParameters` array reference is shared; if the array is later modified by another thread, the exception object may reflect those changes. To avoid this, pass a copy of the array or ensure the array is not mutated after the exception is created.
- Edge case: When `ImagePath` is `null`, the exception may have been thrown for an in-memory image or a stream-based source. Code that logs or displays the path should handle `null` gracefully.
- The `AttemptNumber` property is `null` for non-retried operations. A value of `0` indicates the first attempt failed; higher values indicate subsequent retries.
- The `FilterType` and `FilterName` properties are independent. `FilterName` is the specific filter identifier, while `FilterType` groups filters by category. Both may be `null` if the error is not filter-related.
