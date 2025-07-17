# ImageProcessingException

`ImageProcessingException` is the base exception class for all errors originating from the GPU image processing pipeline. It encapsulates an error code (both as a human-readable string and an optional numeric identifier), the timestamp of the failure, and an optional path to the file that caused the exception. Two derived types, `ImageFileException` and `InvalidImageException`, provide more specific categorisation for file-access problems and content-validity failures respectively.

## API

### Properties

#### `ErrorCode`
`public string ErrorCode { get; }`

A mandatory, non-empty string that uniquely identifies the error condition. This code is intended for programmatic handling and logging correlation.

#### `ErrorCode_Numeric`
`public int? ErrorCode_Numeric { get; }`

An optional integer representation of the error code. When non-null, it can be used for switch-based dispatch or integration with systems that expect numeric error identifiers. Null indicates that no numeric mapping exists for the current `ErrorCode`.

#### `OccurredAt`
`public DateTime OccurredAt { get; }`

The UTC timestamp at which the exception was instantiated. Captured once during construction and never modified.

#### `FilePath`
`public string? FilePath { get; }`

The absolute or relative path of the image file that was being processed when the error occurred. Null when the operation did not involve a specific file (e.g., a pipeline configuration error).

### Constructors

#### `ImageProcessingException()`
Protected parameterless constructor. Initialises `OccurredAt` to `DateTime.UtcNow` and sets `ErrorCode` to a default general-failure code. Intended for use by derived classes and serialisation frameworks.

#### `ImageProcessingException(string errorCode)`
Creates an instance with the given `errorCode`. `ErrorCode_Numeric` remains null and `FilePath` remains null.

| Parameter | Type     | Description                              |
|-----------|----------|------------------------------------------|
| `errorCode` | `string` | The error code string. Must not be null. |

**Throws** `ArgumentNullException` when `errorCode` is null.

#### `ImageProcessingException(string errorCode, int errorCodeNumeric)`
Creates an instance with both a string and numeric error code. `FilePath` remains null.

| Parameter          | Type     | Description                                |
|--------------------|----------|--------------------------------------------|
| `errorCode`        | `string` | The error code string. Must not be null.   |
| `errorCodeNumeric` | `int`    | The numeric error identifier.              |

**Throws** `ArgumentNullException` when `errorCode` is null.

#### `ImageProcessingException(string errorCode, string filePath)`
Creates an instance with an error code and the path of the offending file. `ErrorCode_Numeric` remains null.

| Parameter   | Type     | Description                              |
|-------------|----------|------------------------------------------|
| `errorCode` | `string` | The error code string. Must not be null. |
| `filePath`  | `string` | The file path. Can be null.              |

**Throws** `ArgumentNullException` when `errorCode` is null.

#### `ImageProcessingException(string errorCode, int errorCodeNumeric, string filePath)`
Full constructor accepting all three data elements.

| Parameter          | Type     | Description                              |
|--------------------|----------|------------------------------------------|
| `errorCode`        | `string` | The error code string. Must not be null. |
| `errorCodeNumeric` | `int`    | The numeric error identifier.            |
| `filePath`         | `string` | The file path. Can be null.              |

**Throws** `ArgumentNullException` when `errorCode` is null.

### Methods

#### `ToString()`
`public override string ToString()`

Returns a formatted string containing the exception type name, `ErrorCode`, `ErrorCode_Numeric` (if present), `FilePath` (if present), `OccurredAt`, and the standard exception message and stack trace. The format is stable and suitable for diagnostic logs.

### Derived Types

#### `ImageFileException`
Represents errors related to file-system operations on an image file (e.g., missing file, access denied, lock contention). Inherits all members of `ImageProcessingException` and adds no further public surface beyond its own constructors.

#### `InvalidImageException`
Represents errors where the file exists and is accessible but its content is not a valid image according to the expected format, dimensions, or colour space. Inherits all members of `ImageProcessingException` and adds no further public surface beyond its own constructors.

## Usage

### Example 1: Catching and logging a GPU processing failure

```csharp
try
{
    processor.ApplyGaussianBlur("scan.png", radius: 3.0f);
}
catch (ImageProcessingException ex)
{
    Console.WriteLine($"Processing failed at {ex.OccurredAt:O}");
    Console.WriteLine($"Error code: {ex.ErrorCode} (numeric: {ex.ErrorCode_Numeric})");

    if (ex.FilePath is not null)
    {
        Console.WriteLine($"Offending file: {ex.FilePath}");
    }

    // Switch on numeric code for programmatic recovery
    switch (ex.ErrorCode_Numeric)
    {
        case 1001:
            // Retry with reduced parameters
            break;
        case 2002:
            // Notify user about unsupported format
            break;
        default:
            throw; // Unhandled, re-throw
    }
}
```

### Example 2: Throwing a specific derived exception

```csharp
public void ValidateImage(string path, byte[] data)
{
    if (!File.Exists(path))
    {
        throw new ImageFileException("FILE_NOT_FOUND", 4001, path);
    }

    if (!HasValidHeader(data))
    {
        throw new InvalidImageException("BAD_HEADER", 5003, path);
    }

    // Proceed with GPU upload...
}
```

## Notes

- **Immutability**: All public properties are read-only and set exclusively during construction. Instances are safe to share across threads without synchronisation.
- **Serialisation**: The protected parameterless constructor supports deserialisation scenarios. Serialisers that invoke this constructor will obtain an instance with `OccurredAt` set to the moment of deserialisation rather than the original failure time; consumers relying on exact timestamps should prefer persisting the `OccurredAt` value separately.
- **Error code uniqueness**: No central registry validates uniqueness of `ErrorCode` values. Callers must ensure that codes thrown from different pipeline stages do not collide unintentionally.
- **Null `FilePath`**: Operations that fail during initialisation or configuration (before any file is opened) will produce exceptions with `FilePath` set to null. Downstream code must null-check before using the path for user feedback or cleanup.
- **Derived type distinction**: `ImageFileException` indicates an environmental or permissions problem that may succeed later; `InvalidImageException` indicates a permanent content problem. Catch blocks should order these before the base `ImageProcessingException` to apply appropriate recovery logic.
- **Thread safety**: Construction and property reads are thread-safe. The `ToString()` method reads immutable state and is safe for concurrent calls.
