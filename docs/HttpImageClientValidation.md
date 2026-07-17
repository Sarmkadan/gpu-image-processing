# HttpImageClientValidation

Provides validation utilities for HTTP image client operations, including URL, file path, HTTP status codes, timeouts, and retry configurations. The class offers static methods to validate individual components and comprehensive validation for client configurations.

## API

### `public static IReadOnlyList<string> Validate`

Validates the entire HTTP image client configuration. Returns a list of validation error messages; an empty list indicates all validations passed.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---

### `public static bool IsValid`

Determines whether the provided HTTP image client configuration is valid. Returns `true` if all validations pass; otherwise, `false`.

- **Parameters**: None
- **Return value**: `bool` – `true` if valid; otherwise, `false`.
- **Throws**: None

---
### `public static void EnsureValid`

Validates the provided HTTP image client configuration and throws an `InvalidOperationException` if any validation fails.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: `InvalidOperationException` – If any validation fails.

---
### `public static IReadOnlyList<string> ValidateUrl`

Validates the provided image URL. Checks for null, empty, or malformed URLs.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---
### `public static IReadOnlyList<string> ValidateFilePath`

Validates the provided file path. Checks for null, empty, or invalid path characters.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---
### `public static IReadOnlyList<string> ValidateFileExists`

Validates whether the file at the provided path exists. Returns errors if the file is missing or inaccessible.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---
### `public static IReadOnlyList<string> ValidateOutputDirectory`

Validates the provided output directory path. Checks for null, empty, or invalid directory paths, and verifies write permissions.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---
### `public static IReadOnlyList<string> ValidateHttpStatusCode`

Validates the provided HTTP status code. Ensures the code is within the valid range (100–599).

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---
### `public static IReadOnlyList<string> ValidateTimeout`

Validates the provided timeout value. Ensures the timeout is positive and within acceptable bounds.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

---
### `public static IReadOnlyList<string> ValidateMaxRetries`

Validates the provided maximum retry count. Ensures the value is non-negative and within acceptable limits.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – Collection of error messages; empty if valid.
- **Throws**: None

## Usage

```csharp
// Example 1: Validate a URL before processing
var urlErrors = HttpImageClientValidation.ValidateUrl();
if (urlErrors.Count == 0)
{
    Console.WriteLine("URL is valid.");
}
else
{
    foreach (var error in urlErrors)
    {
        Console.WriteLine($"URL error: {error}");
    }
}

// Example 2: Ensure client configuration is valid
try
{
    HttpImageClientValidation.EnsureValid();
    Console.WriteLine("Client configuration is valid.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

## Notes

All validation methods are stateless and thread-safe. They operate on ambient or context-specific values (e.g., current configuration) rather than instance state, ensuring consistent behavior across concurrent calls. Edge cases include handling of null or empty strings, invalid file paths, unreachable directories, and out-of-range numeric values. No persistent state is modified during validation, and no external resources are accessed beyond filesystem checks (e.g., `ValidateFileExists`, `ValidateOutputDirectory`).
