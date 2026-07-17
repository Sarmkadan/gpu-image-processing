# PerformanceMonitoringServiceExtensionsValidation

Provides static validation utilities for performance monitoring service extensions. The class exposes a set of validation errors, a quick validity check, and a method that throws an exception when the current configuration is invalid. It is designed to be used before enabling or modifying performance monitoring features.

## API

### `Validate`
```csharp
public static IReadOnlyList<string> Validate { get; }
```
Gets a read-only list of validation error messages describing why the current performance monitoring service extensions configuration is invalid.  
- **Returns**: `IReadOnlyList<string>` – an empty list if the configuration is valid; otherwise, a list of human-readable error messages.  
- **Throws**: Never.

### `IsValid`
```csharp
public static bool IsValid { get; }
```
Gets a value indicating whether the current performance monitoring service extensions configuration is valid.  
- **Returns**: `true` if the configuration is valid; otherwise, `false`.  
- **Throws**: Never.

### `EnsureValid`
```csharp
public static void EnsureValid()
```
Validates the current configuration and throws a `ValidationException` if it is invalid.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: `ValidationException` – when the configuration is invalid. The exception message contains the aggregated validation errors.

## Usage

### Example 1: Check validity and log errors
```csharp
if (!PerformanceMonitoringServiceExtensionsValidation.IsValid)
{
    var errors = PerformanceMonitoringServiceExtensionsValidation.Validate;
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
else
{
    // Proceed with performance monitoring setup
}
```

### Example 2: Ensure validity before critical operation
```csharp
try
{
    PerformanceMonitoringServiceExtensionsValidation.EnsureValid();
    // Configuration is valid – start monitoring
    StartPerformanceMonitoring();
}
catch (ValidationException ex)
{
    Logger.LogError("Performance monitoring configuration is invalid: {Message}", ex.Message);
    throw;
}
```

## Notes

- **Edge cases**:  
  - When `Validate` returns an empty list, `IsValid` is `true` and `EnsureValid` completes without throwing.  
  - The `Validate` property always returns a non-null instance; an empty list indicates a valid state.  
  - If the underlying configuration is never set or is null, the validation may produce errors; the exact behavior depends on the validation rules implemented by the class.

- **Thread safety**:  
  All static members are thread-safe. The validation state is either immutable or accessed under synchronization, so concurrent calls to `Validate`, `IsValid`, and `EnsureValid` are safe.
