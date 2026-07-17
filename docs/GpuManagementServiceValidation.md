# GpuManagementServiceValidation

The `GpuManagementServiceValidation` class provides a static utility API for verifying the state of GPU resources within the `gpu-image-processing` pipeline. It offers methods to validate general service configurations, check device allocation feasibility, and enforce these constraints by throwing descriptive exceptions when validation fails, ensuring that GPU operations are only attempted when the underlying hardware and driver state are suitable.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate()
```
Performs a comprehensive validation check on the current GPU management service configuration. It returns a read-only list of error messages describing any detected issues. If the configuration is valid, the returned list is empty. This method does not throw exceptions; it aggregates all validation failures for inspection.

### Validate
*Note: The signature indicates an overloaded or specialized validation method also named `Validate`. Without explicit parameter definitions in the source provided, this member serves as an alternative entry point for validation logic, potentially targeting specific subsystems or contexts distinct from the parameterless overload.*
```csharp
public static IReadOnlyList<string> Validate(...)
```
Executes a validation routine and returns a read-only list of strings representing error messages. Like the parameterless overload, it aggregates failures without throwing, allowing the caller to decide how to handle the invalid state.

### IsValid
```csharp
public static bool IsValid()
```
A convenience method that determines if the GPU management service is currently in a valid state. It returns `true` if no validation errors are present, and `false` otherwise. This is equivalent to checking if `Validate().Count == 0` but optimized for boolean evaluation.

### EnsureValid
```csharp
public static void EnsureValid()
```
Enforces the validity of the GPU management service. If the service configuration is invalid, this method throws a validation exception (typically `ValidationException` or a domain-specific derivative) containing the aggregated error messages from the internal validation logic. If the service is valid, the method completes silently.

### ValidateDeviceAllocation
```csharp
public static IReadOnlyList<string> ValidateDeviceAllocation()
```
Specifically validates the feasibility of allocating GPU devices for the current workload. It returns a read-only list of strings detailing any reasons why device allocation would fail (e.g., insufficient VRAM, device busy, or driver limitations). It does not throw exceptions.

### CanAllocate
```csharp
public static bool CanAllocate()
```
Checks whether GPU devices can be successfully allocated at this time. Returns `true` if allocation is possible, and `false` if `ValidateDeviceAllocation` would return errors. This provides a quick boolean check before attempting expensive allocation operations.

### EnsureCanAllocate
```csharp
public static void EnsureCanAllocate()
```
Enforces the ability to allocate GPU devices. If allocation is not possible, this method throws an exception detailing the allocation failure reasons. If allocation is feasible, the method returns normally. This is used to fail fast before initiating resource-intensive GPU tasks.

## Usage

### Example 1: Pre-flight Check Before Processing
This example demonstrates using `IsValid` and `EnsureValid` to guard a high-level image processing operation, ensuring the service is configured correctly before starting.

```csharp
using GpuImageProcessing;

public void ProcessImagePipeline(ImageData input)
{
    // Quick boolean check to skip processing if GPU is misconfigured
    if (!GpuManagementServiceValidation.IsValid())
    {
        Logger.Warn("Skipping GPU pipeline due to invalid service configuration.");
        FallbackToCpuProcessing(input);
        return;
    }

    try
    {
        // Enforce validity explicitly before critical section
        GpuManagementServiceValidation.EnsureValid();
        
        // Proceed with GPU operations
        var result = GpuProcessor.Transform(input);
        SaveResult(result);
    }
    catch (ValidationException ex)
    {
        Logger.Error($"GPU Validation failed: {string.Join("; ", ex.Messages)}");
        throw;
    }
}
```

### Example 2: Dynamic Device Allocation Check
This example illustrates checking specific device allocation constraints using `CanAllocate` and `ValidateDeviceAllocation` before requesting heavy VRAM resources.

```csharp
using GpuImageProcessing;
using System.Linq;

public void AllocateHighMemoryContext(int requiredMemoryMb)
{
    var allocationErrors = GpuManagementServiceValidation.ValidateDeviceAllocation();
    
    if (allocationErrors.Any())
    {
        Console.WriteLine("Cannot allocate GPU context:");
        foreach (var error in allocationErrors)
        {
            Console.WriteLine($"- {error}");
        }
        return;
    }

    if (GpuManagementServiceValidation.CanAllocate())
    {
        // Safe to proceed with allocation logic
        var context = GpuDeviceManager.CreateContext(requiredMemoryMb);
        InitializeKernels(context);
    }
    else
    {
        // Fallback or retry logic
        ScheduleForLaterExecution();
    }
}
```

## Notes

*   **Thread Safety**: As a static utility class containing only stateless validation logic (inferred from the lack of instance state and standard validation patterns), `GpuManagementServiceValidation` is expected to be thread-safe. Multiple threads can call `Validate`, `IsValid`, or `EnsureValid` concurrently without external synchronization, provided the underlying GPU driver state accessed by these methods is handled safely by the runtime.
*   **Exception Behavior**: The `Ensure...` methods (`EnsureValid`, `EnsureCanAllocate`) are designed to fail fast. They should be treated as guard clauses. Callers must be prepared to catch validation-specific exceptions when invoking these methods.
*   **Return Value Mutability**: The methods returning `IReadOnlyList<string>` guarantee that the caller cannot modify the returned collection. However, the list contents reflect the state of the system at the exact moment of the call; subsequent calls may yield different results if the GPU state changes (e.g., if another process releases VRAM).
*   **Redundancy**: Calling `EnsureValid` immediately after checking `IsValid` is redundant. Use `IsValid` for flow control (if/else) and `EnsureValid` for assertion-style enforcement where an exception is the desired failure mode.
