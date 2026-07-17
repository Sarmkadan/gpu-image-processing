# BatchProcessingUtilitiesValidation

The `BatchProcessingUtilitiesValidation` class provides a centralized set of static utility methods for validating data collections intended for batch processing operations within the GPU image processing pipeline. It offers a triad of validation strategies per data type: returning a list of error messages, returning a boolean status, or throwing an exception immediately upon failure. This design allows callers to choose the appropriate level of strictness for their specific context, whether gathering multiple validation errors for user feedback or enforcing preconditions in critical execution paths.

## API

### Validate<T>
```csharp
public static IReadOnlyList<string> Validate<T>(T data)
```
Evaluates the provided `data` object against defined batch processing rules for type `T`.
*   **Purpose**: To collect all validation failures without halting execution.
*   **Parameters**: `data` – The instance to validate.
*   **Return Value**: An `IReadOnlyList<string>` containing error descriptions. If the data is valid, the list is empty.
*   **Exceptions**: Throws if `data` is null and nulls are not permitted for type `T`, or if the internal validation logic encounters an unexpected state.

### Validate
```csharp
public static IReadOnlyList<string> Validate(object data)
```
Evaluates the provided `data` object against defined batch processing rules using runtime type inspection.
*   **Purpose**: To validate objects when the generic type is not known at compile time.
*   **Parameters**: `data` – The object instance to validate.
*   **Return Value**: An `IReadOnlyList<string>` containing error descriptions. If the data is valid, the list is empty.
*   **Exceptions**: Throws `ArgumentNullException` if `data` is null, or `ArgumentException` if the runtime type is not supported by the validation engine.

### Validate
```csharp
public static IReadOnlyList<string> Validate(IEnumerable<object> dataSet)
```
Iterates through a collection of objects and aggregates validation errors for the entire set.
*   **Purpose**: To validate a batch of heterogeneous or homogeneous items in a single pass.
*   **Parameters**: `dataSet` – The enumerable collection of objects to validate.
*   **Return Value**: An `IReadOnlyList<string>` containing aggregated error messages for all invalid items in the set.
*   **Exceptions**: Throws `ArgumentNullException` if `dataSet` is null.

### IsValid<T>
```csharp
public static bool IsValid<T>(T data)
```
Determines whether the provided `data` object meets all batch processing criteria for type `T`.
*   **Purpose**: To perform a quick boolean check without generating error message strings.
*   **Parameters**: `data` – The instance to validate.
*   **Return Value**: `true` if the data is valid; otherwise, `false`.
*   **Exceptions**: Throws if `data` is null and nulls are not permitted for type `T`.

### IsValid
```csharp
public static bool IsValid(object data)
```
Determines whether the provided `data` object meets all batch processing criteria using runtime type inspection.
*   **Purpose**: To perform a quick boolean check on non-generic objects.
*   **Parameters**: `data` – The object instance to validate.
*   **Return Value**: `true` if the data is valid; otherwise, `false`.
*   **Exceptions**: Throws `ArgumentNullException` if `data` is null, or `ArgumentException` if the runtime type is unsupported.

### IsValid
```csharp
public static bool IsValid(IEnumerable<object> dataSet)
```
Determines whether every item in the provided collection meets validation criteria.
*   **Purpose**: To verify the integrity of an entire batch before processing begins.
*   **Parameters**: `dataSet` – The enumerable collection of objects to validate.
*   **Return Value**: `true` if all items are valid; `false` if any item fails validation.
*   **Exceptions**: Throws `ArgumentNullException` if `dataSet` is null.

### EnsureValid<T>
```csharp
public static void EnsureValid<T>(T data)
```
Validates the `data` object and throws an exception immediately if validation fails.
*   **Purpose**: To enforce preconditions in methods where invalid data constitutes a critical failure.
*   **Parameters**: `data` – The instance to validate.
*   **Return Value**: None. Returns silently if valid.
*   **Exceptions**: Throws `ValidationException` (or derived type) containing the specific error messages if validation fails. Throws if `data` is null where not allowed.

### EnsureValid
```csharp
public static void EnsureValid(object data)
```
Validates the `data` object via runtime inspection and throws an exception immediately if validation fails.
*   **Purpose**: To enforce preconditions for non-generic objects.
*   **Parameters**: `data` – The object instance to validate.
*   **Return Value**: None. Returns silently if valid.
*   **Exceptions**: Throws `ArgumentNullException` if `data` is null, `ArgumentException` if the type is unsupported, or `ValidationException` if the data fails business rules.

### EnsureValid
```csharp
public static void EnsureValid(IEnumerable<object> dataSet)
```
Validates every item in the `dataSet` and throws an exception if any item is invalid.
*   **Purpose**: To guarantee the integrity of a batch prior to GPU submission.
*   **Parameters**: `dataSet` – The enumerable collection of objects to validate.
*   **Return Value**: None. Returns silently if all items are valid.
*   **Exceptions**: Throws `ArgumentNullException` if `dataSet` is null, or `ValidationException` containing aggregated errors if any item in the collection fails validation.

## Usage

### Example 1: Pre-flight Batch Check
Use `EnsureValid` to halt execution immediately if a batch of image configurations contains any invalid entries before submitting to the GPU queue.

```csharp
using System.Collections.Generic;
using GpuImageProcessing.Utilities;

public void SubmitBatch(List<ImageJobConfig> jobs)
{
    // Cast to IEnumerable<object> to utilize the collection overload
    // This will throw a ValidationException detailing every invalid job if any exist.
    BatchProcessingUtilitiesValidation.EnsureValid((IEnumerable<object>)jobs);

    // Proceed only if no exception was thrown
    GPUQueue.EnqueueBatch(jobs);
}
```

### Example 2: Aggregating Errors for Reporting
Use `Validate` to gather all validation issues across a dataset to return a comprehensive error report to the user interface rather than failing on the first error.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using GpuImageProcessing.Utilities;

public ValidationResult AnalyzeBatch(IEnumerable<FilterParameterSet> parameters)
{
    var errors = BatchProcessingUtilitiesValidation.Validate((IEnumerable<object>)parameters);
    
    if (errors.Count == 0)
    {
        return ValidationResult.Success();
    }

    return ValidationResult.Failure(errors.ToList());
}
```

## Notes

*   **Thread Safety**: All methods in `BatchProcessingUtilitiesValidation` are static and stateless. They are safe to call concurrently from multiple threads provided the input objects (`data` or `dataSet`) are not modified by other threads during the execution of the validation call.
*   **Null Handling**: The non-generic `Validate(object)` and `IsValid(object)` overloads explicitly throw `ArgumentNullException` if the input is null. The generic overloads (`<T>`) may allow nulls depending on the specific validation rules defined for type `T`, but typically treat null as invalid unless `T` is a reference type explicitly configured to accept null states.
*   **Collection Validation**: When validating an `IEnumerable<object>`, the utility enumerates the entire collection. If the collection is extremely large, this may incur a performance cost as every element is inspected. The `EnsureValid(IEnumerable<object>)` method aggregates all errors before throwing, ensuring the exception message contains a complete report of all failures rather than just the first encountered.
*   **Type Support**: The non-generic methods rely on runtime type resolution. Passing an object of a type not registered with the internal validation engine will result in an `ArgumentException`.
