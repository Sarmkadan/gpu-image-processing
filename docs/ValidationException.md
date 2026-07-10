# ValidationException

`ValidationException` is an exception type used to encapsulate validation errors that occur during GPU image processing operations. It provides structured details about which entity failed validation and the specific validation errors encountered, enabling precise error handling and reporting.

## API

### `public string? EntityName`
Gets or sets the name of the entity that failed validation. This is typically the name of the object or parameter being validated (e.g., `"ImageProcessor"`, `"FilterParameters"`).

### `public Dictionary<string, string>? ValidationErrors`
Gets or sets a dictionary where keys represent field or property names and values describe the validation failure reason. For example, `{ "Threshold", "must be between 0 and 1" }`.

### `public ValidationException()`
Initializes a new instance of the `ValidationException` class with default values. The `EntityName` and `ValidationErrors` properties will be `null`.

### `public ValidationException(string? entityName)`
Initializes a new instance of the `ValidationException` class with the specified entity name. The `ValidationErrors` property will be `null`.

Parameter:
- `entityName` (string?): The name of the entity that failed validation.

### `public override string ToString()`
Returns a string representation of the exception, including the entity name (if set) and all validation errors (if any). The format is:
```
ValidationException: [EntityName]
[Key1]: [Error1]
[Key2]: [Error2]
...
```

Return value:
- (string): A formatted string containing validation details.

## Usage

### Example 1: Basic Usage with Entity Name
```csharp
try
{
    var processor = new ImageProcessor();
    processor.ApplyFilter(new FilterParameters { Threshold = 1.5f });
}
catch (ValidationException ex) when (ex.EntityName == "FilterParameters")
{
    Console.WriteLine($"Validation failed for {ex.EntityName}:");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"{error.Key}: {error.Value}");
    }
    // Output:
    // Validation failed for FilterParameters:
    // Threshold: must be between 0 and 1
}
```

### Example 2: Constructing with Validation Errors
```csharp
var errors = new Dictionary<string, string>
{
    { "Width", "must be a positive integer" },
    { "Height", "must be a multiple of 2" }
};
throw new ValidationException("ImageDimensions")
{
    ValidationErrors = errors
};
```

## Notes

- **Thread Safety**: This type is not thread-safe. If multiple threads access or modify the same `ValidationException` instance concurrently, external synchronization is required.
- **Null Handling**: `EntityName` and `ValidationErrors` may be `null`. Always check for `null` before accessing `ValidationErrors` to avoid `NullReferenceException`.
- **Dictionary Mutability**: The `ValidationErrors` dictionary is mutable after construction. If shared across threads, ensure proper synchronization to prevent race conditions during modification.
- **Serialization**: If this exception is serialized (e.g., across app domains or services), ensure all properties (`EntityName`, `ValidationErrors`) are serializable. The default `Dictionary<string, string>` is serializable in .NET.
