# ImageUtilitiesValidation

`ImageUtilitiesValidation` is a static utility class that provides centralized validation logic for image processing configuration and file-related parameters. It exposes methods to verify the correctness of image utility settings, file paths, access permissions, output directories, image dimensions, scale factors, and output filename parameters. Each validation method returns a collection of error messages, allowing callers to inspect all issues at once rather than failing on the first error.

## API

### ValidateImageUtilitiesConfiguration

```csharp
public static IReadOnlyList<string> ValidateImageUtilitiesConfiguration
```

A read-only property that validates the entire image utilities configuration and returns a list of all detected validation errors. An empty list indicates a valid configuration.

**Return value:** A read-only list of error messages. Each string describes a specific validation failure. Returns an empty list when the configuration is valid.

**Exceptions:** None. This property does not throw.

---

### IsImageUtilitiesConfigurationValid

```csharp
public static bool IsImageUtilitiesConfigurationValid
```

A read-only property that provides a quick boolean check for whether the image utilities configuration is valid.

**Return value:** `true` if the configuration passes all validation checks; `false` otherwise.

**Exceptions:** None. This property does not throw.

---

### EnsureImageUtilitiesConfigurationValid

```csharp
public static void EnsureImageUtilitiesConfigurationValid()
```

Performs a full validation of the image utilities configuration and throws an exception if any validation errors are found.

**Parameters:** None.

**Return value:** None.

**Exceptions:** Throws an exception (typically `InvalidOperationException` or a custom validation exception) when the configuration is invalid. The exception message aggregates all validation errors.

---

### ValidateFilePath

```csharp
public static IReadOnlyList<string> ValidateFilePath(string filePath)
```

Validates a single file path for correctness, checking for null, empty, invalid characters, and other path-related constraints.

**Parameters:**
- `filePath` — The file path string to validate.

**Return value:** A read-only list of error messages. An empty list indicates the file path is valid.

**Exceptions:** None. This method does not throw.

---

### ValidateImageFileAccess

```csharp
public static IReadOnlyList<string> ValidateImageFileAccess(string filePath)
```

Validates that an image file at the specified path exists and is accessible for reading. This goes beyond structural path validation to verify actual file system access.

**Parameters:**
- `filePath` — The path to the image file to check.

**Return value:** A read-only list of error messages describing any access problems (file not found, insufficient permissions, etc.). An empty list indicates the file is accessible.

**Exceptions:** None. This method does not throw.

---

### ValidateOutputDirectory

```csharp
public static IReadOnlyList<string> ValidateOutputDirectory(string directoryPath)
```

Validates that the specified output directory exists or can be created, and is writable.

**Parameters:**
- `directoryPath` — The path to the output directory to validate.

**Return value:** A read-only list of error messages. An empty list indicates the output directory is valid and accessible.

**Exceptions:** None. This method does not throw.

---

### ValidateImageDimensions

```csharp
public static IReadOnlyList<string> ValidateImageDimensions(int width, int height)
```

Validates that the given image dimensions are within acceptable bounds for processing (e.g., positive values, not exceeding maximum supported dimensions).

**Parameters:**
- `width` — The image width in pixels.
- `height` — The image height in pixels.

**Return value:** A read-only list of error messages. An empty list indicates the dimensions are valid.

**Exceptions:** None. This method does not throw.

---

### ValidateScaleFactor

```csharp
public static IReadOnlyList<string> ValidateScaleFactor(double scaleFactor)
```

Validates that a scale factor is within the allowed range for image resizing operations.

**Parameters:**
- `scaleFactor` — The scaling multiplier to validate (e.g., 0.5 for half size, 2.0 for double size).

**Return value:** A read-only list of error messages. An empty list indicates the scale factor is valid.

**Exceptions:** None. This method does not throw.

---

### ValidateOutputFilenameParameters

```csharp
public static IReadOnlyList<string> ValidateOutputFilenameParameters(string baseFilename, string suffix, string extension)
```

Validates the components used to construct an output filename, ensuring each part is well-formed and the combination produces a valid filename.

**Parameters:**
- `baseFilename` — The base name for the output file.
- `suffix` — An optional suffix to append to the base name.
- `extension` — The file extension (with or without a leading dot).

**Return value:** A read-only list of error messages. An empty list indicates all filename parameters are valid.

**Exceptions:** None. This method does not throw.

---

## Usage

### Example 1: Validating Configuration Before Processing

```csharp
// Check if the image utilities configuration is valid before starting a batch job
if (!ImageUtilitiesValidation.IsImageUtilitiesConfigurationValid)
{
    var errors = ImageUtilitiesValidation.ValidateImageUtilitiesConfiguration;
    foreach (var error in errors)
    {
        Console.WriteLine($"Configuration error: {error}");
    }
    return;
}

// Proceed with processing
Console.WriteLine("Configuration is valid. Starting image processing...");
```

### Example 2: Validating User-Provided Parameters

```csharp
public void ProcessImage(string inputPath, string outputDir, int width, int height, double scale)
{
    // Collect all validation errors
    var errors = new List<string>();

    errors.AddRange(ImageUtilitiesValidation.ValidateFilePath(inputPath));
    errors.AddRange(ImageUtilitiesValidation.ValidateImageFileAccess(inputPath));
    errors.AddRange(ImageUtilitiesValidation.ValidateOutputDirectory(outputDir));
    errors.AddRange(ImageUtilitiesValidation.ValidateImageDimensions(width, height));
    errors.AddRange(ImageUtilitiesValidation.ValidateScaleFactor(scale));

    if (errors.Count > 0)
    {
        throw new ArgumentException($"Invalid parameters: {string.Join("; ", errors)}");
    }

    // All parameters are valid — proceed with processing
    Console.WriteLine("All parameters validated successfully.");
}
```

---

## Notes

- All validation methods return error lists rather than throwing exceptions, giving callers full control over error handling and aggregation. The sole exception is `EnsureImageUtilitiesConfigurationValid`, which throws when the configuration is invalid.
- `ValidateImageFileAccess` performs actual file system checks and may be affected by transient conditions such as network latency or temporary permission changes. Callers should be aware that a valid result at one moment does not guarantee continued access.
- `ValidateOutputDirectory` may attempt to create the directory if it does not exist, depending on the underlying implementation. If directory creation fails, the error is reported in the returned list.
- `ValidateImageDimensions` likely enforces minimum and maximum dimension constraints. Passing zero or negative values will produce validation errors.
- `ValidateScaleFactor` typically rejects non-positive values and may enforce an upper bound to prevent excessive memory allocation.
- All members are static and thread-safe by design, as they operate on immutable inputs and do not mutate shared state. No synchronization is required when calling these methods from multiple threads concurrently.
