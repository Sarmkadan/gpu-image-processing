# HealthCheckServiceValidation

Provides static validation methods for health check results and configurations used by the `gpu-image-processing` health check infrastructure. The class centralizes common validation logic, returning lists of error messages, boolean validity indicators, or throwing exceptions when validation fails. All members are thread-safe and designed for use in both synchronous and asynchronous health check pipelines.

## API

### Validate

Four overloads that validate a health check input and return a read‑only list of error messages. An empty list indicates no errors.

| Overload | Description |
|----------|-------------|
| `Validate(HealthCheckResult result)` | Validates a single health check result object. |
| `Validate(IEnumerable<HealthCheckResult> results)` | Validates a collection of health check results. |
| `Validate(string serviceName)` | Validates a service name against allowed patterns and length constraints. |
| `Validate(HealthCheckServiceOptions options)` | Validates the configuration options for a health check service. |

**Returns:** `IReadOnlyList<string>` – a list of validation error messages; empty if the input is valid.

**Throws:** `ArgumentNullException` if the input is `null` (where applicable). Other exceptions may be thrown by the specific overload (e.g., `ArgumentException` for invalid service names).

### IsValid

Three overloads that return a boolean indicating whether the input passes validation.

| Overload | Description |
|----------|-------------|
| `IsValid(HealthCheckResult result)` | Checks whether a single health check result is valid. |
| `IsValid(IEnumerable<HealthCheckResult> results)` | Checks whether all results in a collection are valid. |
| `IsValid(string serviceName)` | Checks whether a service name is valid. |

**Returns:** `bool` – `true` if the input is valid; otherwise `false`.

**Throws:** `ArgumentNullException` if the input is `null` (where applicable).

### EnsureValid

Four overloads that validate the input and throw an exception if validation fails.

| Overload | Description |
|----------|-------------|
| `EnsureValid(HealthCheckResult result)` | Throws if the health check result is invalid. |
| `EnsureValid(IEnumerable<HealthCheckResult> results)` | Throws if any result in the collection is invalid. |
| `EnsureValid(string serviceName)` | Throws if the service name is invalid. |
| `EnsureValid(HealthCheckServiceOptions options)` | Throws if the health check service options are invalid. |

**Throws:** `ValidationException` (or a derived exception) containing the aggregated error messages. `ArgumentNullException` if the input is `null` (where applicable).

## Usage

### Example 1: Validating a health check result and logging errors

```csharp
using System;
using System.Collections.Generic;
using GpuImageProcessing.HealthChecks;

public class HealthCheckReporter
{
    public void Report(HealthCheckResult result)
    {
        IReadOnlyList<string> errors = HealthCheckServiceValidation.Validate(result);
        if (errors.Count > 0)
        {
            Console.WriteLine("Health check validation failed:");
            foreach (string error in errors)
                Console.WriteLine($"  - {error}");
        }
        else
        {
            Console.WriteLine("Health check result is valid.");
        }
    }
}
```

### Example 2: Ensuring valid service options before registration

```csharp
using GpuImageProcessing.HealthChecks;

public class ServiceRegistration
{
    public void RegisterHealthCheck(HealthCheckServiceOptions options)
    {
        // Throws if options are invalid
        HealthCheckServiceValidation.EnsureValid(options);

        // Proceed with registration
        // ...
    }
}
```

## Notes

- All methods are static and thread‑safe. They do not modify any shared state and can be called concurrently from multiple threads.
- The `Validate` overloads that accept `IEnumerable<HealthCheckResult>` will enumerate the collection exactly once. Avoid passing deferred LINQ queries that may cause side effects.
- `EnsureValid` throws a `ValidationException` (from `System.ComponentModel.DataAnnotations` or a custom exception defined in the project) that contains all error messages. Catch this exception to present a unified validation failure.
- When the input is `null`, `ArgumentNullException` is thrown by all overloads that accept reference types. The `IsValid` overloads also throw on `null`; they do not return `false` for a `null` argument.
- The `Validate` and `IsValid` overloads that accept a `string` service name perform pattern matching and length checks. Service names must be non‑empty, contain only alphanumeric characters and hyphens, and be between 1 and 128 characters long.
- The `HealthCheckServiceOptions` overload validates that required properties (e.g., endpoint URI, timeout) are present and within acceptable ranges.
