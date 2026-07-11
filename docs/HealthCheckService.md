# HealthCheckService

Central service for performing health checks on system components and external dependencies. It aggregates results from multiple health checks, tracks overall system health, and provides detailed diagnostics for troubleshooting.

## API

### `HealthCheckService` (constructor)
Initializes a new instance of the `HealthCheckService` with default health check registrations. The service starts with no registered checks and an `Unknown` overall status until the first health check is performed.

### `RegisterHealthCheck`
Registers a health check component with the service. The provided `IHealthCheck` instance will be invoked during health check execution.
- **Parameters**:
  - `healthCheck`: The health check implementation to register.
- **Throws**: `ArgumentNullException` if `healthCheck` is `null`.

### `CheckHealthAsync`
Performs an aggregated health check across all registered components and returns a composite result.
- **Returns**: `Task<HealthCheckResult>` containing the overall health status, message, details, and timestamp.
- **Throws**: `InvalidOperationException` if no health checks have been registered.

### `CheckComponentAsync`
Executes the health check for a specific registered component by name.
- **Parameters**:
  - `componentName`: The name of the component to check.
- **Returns**: `Task<ComponentHealth>` containing the component's health status, message, details, and timestamp.
- **Throws**:
  - `ArgumentNullException` if `componentName` is `null`.
  - `KeyNotFoundException` if no component with the specified name is registered.

### `GetRegisteredChecks`
Returns the names of all registered health check components.
- **Returns**: `List<string>` of registered component names.

### `Status` (property)
Gets the overall health status of the system (`HealthStatus`).
- **Access**: Read-only.

### `Message` (property)
Gets the descriptive message associated with the overall health status.
- **Access**: Read-only.

### `Details` (property)
Gets a dictionary of additional diagnostic details related to the overall health status.
- **Access**: Read-only.

### `CheckedAt` (property)
Gets the timestamp when the last health check was performed.
- **Access**: Read-only.

### `Timestamp` (property)
Gets the timestamp when the health check result was generated.
- **Access**: Read-only.

### `Components` (property)
Gets a dictionary mapping component names to their individual health check results (`ComponentHealth`).
- **Access**: Read-only.

### `Summary` (property)
Gets a human-readable summary of the overall health status and key component states.
- **Access**: Read-only.

### `MemoryHealthCheck` (property)
Gets the registered `MemoryHealthCheck` instance, if one exists.
- **Access**: Read-only.
- **Throws**: `InvalidOperationException` if no memory health check is registered.

### `ResponseTimeHealthCheck` (property)
Gets the registered `ResponseTimeHealthCheck` instance, if one exists.
- **Access**: Read-only.
- **Throws**: `InvalidOperationException` if no response time health check is registered.

## Usage

### Example 1: Basic health check execution
```csharp
var healthCheckService = new HealthCheckService();
healthCheckService.RegisterHealthCheck(new MemoryHealthCheck());
healthCheckService.RegisterHealthCheck(new ResponseTimeHealthCheck());

var result = await healthCheckService.CheckHealthAsync();

Console.WriteLine($"Overall Status: {result.Status}");
Console.WriteLine($"Message: {result.Message}");
foreach (var component in result.Components)
{
    Console.WriteLine($"{component.Key}: {component.Value.Status}");
}
```

### Example 2: Checking a specific component
```csharp
var healthCheckService = new HealthCheckService();
healthCheckService.RegisterHealthCheck(new MemoryHealthCheck());
healthCheckService.RegisterHealthCheck(new ResponseTimeHealthCheck());

var componentHealth = await healthCheckService.CheckComponentAsync("MemoryHealthCheck");

Console.WriteLine($"Component Status: {componentHealth.Status}");
Console.WriteLine($"Details: {string.Join(", ", componentHealth.Details.Select(d => $"{d.Key}={d.Value}"))}");
```

## Notes

- The service is **thread-safe** for concurrent calls to `RegisterHealthCheck`, `CheckHealthAsync`, and `CheckComponentAsync`. Internally, a `ConcurrentDictionary` is used for registered checks, and results are computed atomically per check.
- Health check execution is **not atomic** across components; if one check throws, others may still complete and be included in the result. Exceptions from individual checks are captured in the `Details` dictionary under the `"Exceptions"` key.
- The `CheckedAt` timestamp reflects the start of the health check execution, while `Timestamp` reflects when the aggregated result was finalized.
- Calling `CheckHealthAsync` or `CheckComponentAsync` before any checks are registered throws `InvalidOperationException`; ensure at least one check is registered via `RegisterHealthCheck` first.
