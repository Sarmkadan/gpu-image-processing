# DataConversionUtilitiesValidation

Utility class providing validation and normalization methods for data conversion operations, particularly for GPU image processing pipelines where numeric ranges must be validated and normalized to expected domains (e.g., [0, 1] or [0, 255]).

## API

### `public static bool IsValid`

Determines whether a given value is within the valid range for normalization or denormalization operations.

- **Parameters**:
  - `value`: The numeric value to validate.
- **Return value**: `true` if the value is valid; otherwise, `false`.
- **Exceptions**: None.

### `public static void EnsureValid`

Throws an exception if the provided value is outside the valid range for conversion operations.

- **Parameters**:
  - `value`: The numeric value to validate.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if the value is invalid.

### `public static IReadOnlyList<string> Validate(short value)`

Validates a `short` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `short` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(ushort value)`

Validates a `ushort` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `ushort` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(int value)`

Validates an `int` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `int` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(uint value)`

Validates a `uint` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `uint` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(long value)`

Validates a `long` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `long` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(ulong value)`

Validates a `ulong` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `ulong` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(float value)`

Validates a `float` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `float` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(double value)`

Validates a `double` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `double` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> Validate(decimal value)`

Validates a `decimal` value for conversion operations and returns a list of validation error messages.

- **Parameters**:
  - `value`: The `decimal` value to validate.
- **Return value**: An empty list if the value is valid; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> ValidateNormalize(float value)`

Validates and normalizes a `float` value to the range [0, 1], returning validation error messages if invalid.

- **Parameters**:
  - `value`: The `float` value to validate and normalize.
- **Return value**: An empty list if the value is valid and normalized; otherwise, a list containing error messages.
- **Exceptions**: None.

### `public static IReadOnlyList<string> ValidateDenormalize(float value)`

Validates and denormalizes a `float` value from the range [0, 1] to [0, 255], returning validation error messages if invalid.

- **Parameters**:
  - `value`: The `float` value to validate and denormalize.
- **Return value**: An empty list if the value is valid and denormalized; otherwise, a list containing error messages.
- **Exceptions**: None.

## Usage

```csharp
// Example 1: Validating and normalizing pixel intensity values
var intensity = 0.75f;
var errors = DataConversionUtilitiesValidation.ValidateNormalize(intensity);
if (errors.Count == 0)
{
    var normalized = intensity; // Assume normalization applied here
    Console.WriteLine($"Normalized intensity: {normalized}");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Example 2: Batch validation of image channel values
var channelValues = new float[] { 0.0f, 0.5f, 1.0f, 1.1f };
var allValid = true;
foreach (var value in channelValues)
{
    var validationErrors = DataConversionUtilitiesValidation.Validate(value);
    if (validationErrors.Count > 0)
    {
        allValid = false;
        Console.WriteLine($"Invalid channel value {value}:");
        foreach (var error in validationErrors)
        {
            Console.WriteLine($"- {error}");
        }
    }
}
if (allValid)
{
    Console.WriteLine("All channel values are valid.");
}
```

## Notes

- **Thread safety**: All methods are stateless and thread-safe; no shared mutable state is accessed.
- **Edge cases**: Floating-point values at the exact boundaries (e.g., 0.0f or 1.0f) are considered valid. Values slightly outside due to floating-point imprecision (e.g., 1.0000001f) may be rejected depending on tolerance settings internal to the implementation.
- **Performance**: Validation methods are designed for low overhead and are suitable for use within tight GPU data processing loops, though callers should batch validation where possible to minimize overhead.
- **Range semantics**: Normalization maps [0, 1] to itself; denormalization maps [0, 1] to [0, 255]. Values outside these ranges are rejected unless explicitly handled by the caller.
