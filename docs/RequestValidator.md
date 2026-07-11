# RequestValidator

The `RequestValidator` class provides a fluent interface for defining and executing validation rules against incoming request data within the `gpu-image-processing` pipeline. It enables developers to chain multiple constraints—such as field presence, string length limits, and numeric range checks—onto specific fields, accumulating errors into a single `ValidationResult` if any rule fails. This component is essential for ensuring data integrity before expensive GPU operations are initiated, preventing resource waste on malformed inputs.

## API

### Constructors

#### `public RequestValidator()`
Initializes a new instance of the `RequestValidator` class. The validator starts in a valid state with an empty error list.

### Instance Methods

#### `public RequestValidator RequireField(string fieldName)`
Adds a rule to ensure the specified field exists and is not null in the target object.
*   **Parameters**: `fieldName` – The name of the field to check.
*   **Returns**: The current `RequestValidator` instance to allow method chaining.
*   **Throws**: None. Errors are accumulated in the `Errors` list upon execution of `Validate`.

#### `public RequestValidator ValidateStringLength(string fieldName, int minLength, int maxLength)`
Adds a rule to verify that a string field's length falls within the specified inclusive range. If the field is null, this rule typically fails unless combined with optional handling logic.
*   **Parameters**: 
    *   `fieldName` – The name of the string field to validate.
    *   `minLength` – The minimum allowed character count.
    *   `maxLength` – The maximum allowed character count.
*   **Returns**: The current `RequestValidator` instance.
*   **Throws**: None.

#### `public RequestValidator ValidateRange<T>(string fieldName, T min, T max)`
Adds a rule to ensure a numeric field value lies within the specified inclusive range.
*   **Parameters**: 
    *   `fieldName` – The name of the numeric field.
    *   `min` – The minimum acceptable value.
    *   `max` – The maximum acceptable value.
*   **Returns**: The current `RequestValidator` instance.
*   **Throws**: None. Type mismatches between the field value and generic type `T` may result in validation failure during execution.

#### `public RequestValidator AddRule(string fieldName, Func<object, bool> validator, string errorMessage)`
Registers a custom validation logic for a specific field.
*   **Parameters**: 
    *   `fieldName` – The target field name.
    *   `validator` – A delegate returning `true` if the value is valid, `false` otherwise.
    *   `errorMessage` – The message to append to `Errors` if the validator returns `false`.
*   **Returns**: The current `RequestValidator` instance.
*   **Throws**: None.

#### `public ValidationResult Validate(object request)`
Executes all registered rules against the provided `request` object.
*   **Parameters**: `request` – The object containing the data to validate.
*   **Returns**: A `ValidationResult` indicating success or failure. If failed, the `Errors` property on the validator instance is populated.
*   **Throws**: None.

### Static Methods

#### `public static Dictionary<string, object> SanitizeRequest(Dictionary<string, object> rawRequest)`
Performs basic sanitization on a raw request dictionary, such as trimming whitespace from string values and removing null entries where appropriate.
*   **Parameters**: `rawRequest` – The incoming key-value pair collection.
*   **Returns**: A new `Dictionary<string, object>` containing sanitized data.
*   **Throws**: `ArgumentNullException` if `rawRequest` is null.

### Properties

#### `public string FieldName`
Gets or sets the name of the field currently being processed or last reported in an error context.

#### `public Func<object, bool> Validator`
Gets or sets the custom validation delegate associated with the current rule context.

#### `public string ErrorMessage`
Gets or sets the error message associated with the last failed validation rule.

#### `public bool IsValid`
Gets a value indicating whether the last execution of `Validate` resulted in no errors.

#### `public List<string> Errors`
Gets the list of error messages accumulated during the most recent validation run. If validation passed, this list is empty.

### Static Properties

#### `public static ValidationResult Success`
Returns a pre-instantiated `ValidationResult` representing a successful validation outcome.

#### `public static ValidationResult Failure`
Returns a pre-instantiated `ValidationResult` template or factory reference used to construct failure outcomes (specific implementation depends on internal `ValidationResult` logic).

## Usage

### Example 1: Validating Image Processing Parameters
This example demonstrates chaining standard rules to validate dimensions and format before submitting a job to the GPU queue.

```csharp
public ValidationResult ValidateImageJob(ImageJobRequest request)
{
    var validator = new RequestValidator();

    var result = validator
        .RequireField("ImageId")
        .ValidateStringLength("Format", 3, 5) // e.g., "png", "jpeg"
        .ValidateRange("Width", 64, 8192)
        .ValidateRange("Height", 64, 8192)
        .AddRule("CompressionLevel", val => 
        {
            if (val is int level) return level >= 0 && level <= 100;
            return false;
        }, "CompressionLevel must be an integer between 0 and 100.")
        .Validate(request);

    return result;
}
```

### Example 2: Sanitizing and Validating Dynamic Device Configuration
This example shows how to sanitize incoming dynamic data before applying custom validation logic for device-specific constraints.

```csharp
public ValidationResult ProcessDeviceConfig(Dictionary<string, object> rawConfig)
{
    // Sanitize input first
    var cleanConfig = RequestValidator.SanitizeRequest(rawConfig);

    var validator = new RequestValidator();
    
    // Apply custom logic for GPU memory allocation
    validator.AddRule("GpuMemoryLimit", val => 
    {
        if (val is long mem) return mem > 0 && mem <= 32L * 1024 * 1024 * 1024; // Max 32GB
        return false;
    }, "GpuMemoryLimit exceeds hardware capabilities or is invalid.");

    var result = validator.Validate(cleanConfig);

    if (!result.IsSuccess)
    {
        // Handle errors using validator.Errors
        return result;
    }

    return RequestValidator.Success;
}
```

## Notes

*   **Statefulness**: The `RequestValidator` instance maintains state regarding the last validation run via the `Errors`, `IsValid`, and `FieldName` properties. Reusing the same instance for multiple distinct requests without re-instantiation or clearing state may lead to stale error data. It is recommended to instantiate a new validator per request scope.
*   **Thread Safety**: The instance methods and properties are not thread-safe. Do not share a single `RequestValidator` instance across multiple threads executing `Validate` or chaining rules concurrently. The static `SanitizeRequest` method is thread-safe as it operates solely on input parameters and returns a new dictionary.
*   **Null Handling**: The `RequireField` rule explicitly checks for nulls. However, `ValidateStringLength` and `ValidateRange` may behave unpredictably if the target field is null depending on the internal implementation of the type check; it is best practice to call `RequireField` before applying type-specific constraints.
*   **Error Accumulation**: Validation continues through all registered rules even if an earlier rule fails. This ensures the caller receives a comprehensive list of all data issues in a single pass rather than fixing errors one by one.
