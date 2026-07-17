# ConfigurationValidatorValidation

Provides centralized validation logic for configuration validators used throughout the GPU image processing pipeline. This type exposes static methods to inspect, assert, and enumerate validation rules, ensuring that configuration validators conform to expected contracts before they are consumed by processing components.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate { get; }
```

Returns a read-only list of all registered validation rule identifiers. Each string corresponds to a distinct validation check that can be executed against a configuration validator instance. The list is populated once during static initialization and remains immutable thereafter.

**Return Value**  
`IReadOnlyList<string>` — A collection of validation rule names in the order they were registered. Never null; returns an empty list if no rules are defined.

**Remarks**  
This property appears multiple times in the public surface due to explicit interface implementations or overloaded access patterns. All instances return the same underlying list.

---

### ValidateConfigurationValidator

```csharp
public static IReadOnlyList<string> ValidateConfigurationValidator { get; }
```

Provides the same read-only list of validation rule identifiers as `Validate`. This member exists as a more explicit, self-documenting accessor for contexts where clarity about the subject of validation is desired.

**Return Value**  
`IReadOnlyList<string>` — Identical to the list returned by `Validate`.

---

### IsValidConfigurationValidator

```csharp
public static bool IsValidConfigurationValidator { get; }
```

Evaluates whether the currently configured validator passes all registered validation rules. This property performs a full validation pass each time it is accessed.

**Return Value**  
`bool` — `true` if every validation rule succeeds; `false` if any rule fails or if validation cannot be completed.

**Exceptions**  
None. Failures are reported through the return value rather than by throwing.

---

### EnsureValidConfigurationValidator

```csharp
public static void EnsureValidConfigurationValidator()
```

Performs a full validation pass and throws an exception if any rule fails. Use this method at application startup or configuration load boundaries to fail fast when a misconfigured validator is detected.

**Exceptions**  
Throws an `InvalidOperationException` (or a more specific derived exception type) when one or more validation rules fail. The exception message typically includes the names of the failing rules.

**Remarks**  
This method calls the same underlying validation logic as `IsValidConfigurationValidator` but escalates failures into exceptions rather than returning a boolean.

---

## Usage

### Example 1: Checking validity before processing

```csharp
if (ConfigurationValidatorValidation.IsValidConfigurationValidator)
{
    var processor = new GpuImageProcessor();
    processor.ProcessBatch(imageStreams);
}
else
{
    logger.LogError("Configuration validator is invalid. Processing aborted.");
    return;
}
```

### Example 2: Failing fast during service initialization

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register validator components first
    services.AddConfigurationValidator();
    
    // Build a temporary provider to resolve and validate
    var provider = services.BuildServiceProvider();
    provider.GetRequiredService<IConfigurationValidator>();
    
    // Fail immediately if the validator is misconfigured
    ConfigurationValidatorValidation.EnsureValidConfigurationValidator();
    
    // Continue with remaining service registration
    services.AddGpuProcessingPipeline();
}
```

---

## Notes

- **Thread Safety**  
  All members are static and operate on shared state. The `Validate` and `ValidateConfigurationValidator` properties return an immutable list and are safe to read from any thread without synchronization. `IsValidConfigurationValidator` and `EnsureValidConfigurationValidator` may read mutable configuration state; concurrent modifications to the underlying validator during validation can produce inconsistent results. Callers should ensure that validator configuration is complete and stable before invoking these members.

- **Edge Cases**  
  - When no validation rules are registered, `IsValidConfigurationValidator` returns `true` (vacuously valid), and `EnsureValidConfigurationValidator` completes without throwing.  
  - The `Validate` list may be empty in minimal deployments that do not require configuration validation.  
  - Repeated access to `IsValidConfigurationValidator` may yield different results if the underlying validator state changes between calls.  
  - `EnsureValidConfigurationValidator` is designed for one-shot invocation at initialization boundaries; calling it repeatedly in hot paths is discouraged due to the cost of full validation passes.
