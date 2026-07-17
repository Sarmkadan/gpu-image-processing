# MetricsUtilitiesValidation

The `MetricsUtilitiesValidation` class provides a static set of utility methods designed to enforce data integrity rules for metric configurations within the GPU image processing pipeline. It offers three distinct operational modes for validation: returning a list of error messages for diagnostic purposes, performing a boolean check for quick verification, and throwing exceptions immediately to halt execution if invalid states are detected. These methods are overloaded to support various input signatures required by different stages of the image processing workflow.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(...)
```
*Note: This member is overloaded with three distinct signatures.*

Executes a validation routine against the provided metrics data and returns a collection of error messages describing any violations found. If the input is valid, the returned list is empty. This method is non-destructive and does not throw exceptions for validation failures, making it suitable for pre-flight checks or user interface feedback.

*   **Parameters**: Varies by overload (accepts different combinations of metric objects, configuration strings, or numerical thresholds).
*   **Return Value**: An `IReadOnlyList<string>` containing descriptive error messages. Returns an empty list if validation passes.
*   **Exceptions**: Throws only for critical system errors (e.g., null reference on internal infrastructure), not for validation failures.

### IsValid

```csharp
public static bool IsValid(...)
```
*Note: This member is overloaded with three distinct signatures.*

Performs a lightweight validation check on the provided metrics data and returns a boolean result. This method is optimized for performance in tight loops or conditional guards where the specific reason for failure is not required, only the pass/fail status.

*   **Parameters**: Varies by overload (mirrors the parameter sets of the `Validate` methods).
*   **Return Value**: `true` if the data meets all validation criteria; `false` otherwise.
*   **Exceptions**: Throws only for critical system errors.

### EnsureValid

```csharp
public static void EnsureValid(...)
```
*Note: This member is overloaded with three distinct signatures.*

Validates the provided metrics data and immediately halts execution by throwing an exception if any validation rules are violated. This method is intended for use in constructor guards, API entry points, or critical sections where proceeding with invalid data would compromise the stability of the GPU processing context.

*   **Parameters**: Varies by overload (mirrors the parameter sets of the `Validate` methods).
*   **Return Value**: `void`. Returns normally only if validation succeeds.
*   **Exceptions**: Throws a validation-specific exception (e.g., `ArgumentException` or a custom `MetricsValidationException`) containing details of the first encountered error if validation fails.

## Usage

### Diagnostic Validation with Error Reporting
Use the `Validate` method when you need to collect all potential issues before presenting them to a user or logging them, rather than failing on the first error.

```csharp
using GpuImageProcessing.Utilities;

public void ConfigureMetrics(MetricsConfig config)
{
    var errors = MetricsUtilitiesValidation.Validate(config);

    if (errors.Count > 0)
    {
        Console.WriteLine("Configuration invalid:");
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error}");
        }
        // Fallback to defaults or abort logic
        return;
    }

    // Proceed with valid configuration
    ApplyConfig(config);
}
```

### Guard Clause in Critical Path
Use the `EnsureValid` method within service constructors or command handlers to enforce strict data contracts and prevent the instantiation of invalid processing states.

```csharp
using GpuImageProcessing.Utilities;

public class ImageProcessingJob
{
    public ImageProcessingJob(MetricsData data, double threshold)
    {
        // Throws immediately if data or threshold is invalid, 
        // preventing the creation of an unstable job instance.
        MetricsUtilitiesValidation.EnsureValid(data, threshold);
        
        InitializeGpuContext(data, threshold);
    }
}
```

## Notes

*   **Overload Resolution**: The class contains three overloads for each method type (`Validate`, `IsValid`, `EnsureValid`). Ensure the correct overload is selected based on the specific combination of parameters (e.g., single object vs. multiple primitive arguments) passed at the call site to avoid ambiguity.
*   **Thread Safety**: As the class consists entirely of static methods that operate solely on provided input parameters without maintaining internal mutable state, all members are thread-safe and can be called concurrently from multiple threads without external synchronization.
*   **Exception Behavior**: While `Validate` and `IsValid` suppress validation errors into return values, `EnsureValid` is aggressive by design. In high-frequency processing loops, prefer `IsValid` to avoid the performance overhead of exception throwing and stack trace generation for expected failure cases.
*   **Return Consistency**: The `Validate` method returns an `IReadOnlyList<string>`. Callers should treat this list as immutable; attempting to modify the returned collection will result in a runtime exception.
