# ConfigurationUtilities

The `ConfigurationUtilities` class serves as a centralized, static accessor for retrieving application settings, environment variables, and runtime configuration parameters within the `gpu-image-processing` project. It abstracts the underlying configuration sources—such as appsettings files, environment variables, or system defaults—providing strongly-typed helper methods to ensure consistent data retrieval across the image processing pipeline, GPU device selection, logging infrastructure, and performance tuning modules.

## API

### GetConfigValue
Retrieves a configuration setting as a raw string.
*   **Parameters**: `string key` – The name of the configuration key.
*   **Returns**: `string` – The value associated with the key, or `null` if the key does not exist.
*   **Throws**: `ArgumentNullException` if the provided key is null or empty.

### GetConfigInteger
Retrieves a configuration setting and parses it as a 32-bit integer.
*   **Parameters**: `string key` – The name of the configuration key.
*   **Returns**: `int` – The parsed integer value.
*   **Throws**: `FormatException` if the value cannot be parsed as an integer; `KeyNotFoundException` if the key is missing.

### GetConfigBoolean
Retrieves a configuration setting and parses it as a boolean.
*   **Parameters**: `string key` – The name of the configuration key.
*   **Returns**: `bool` – The parsed boolean value (supports "true", "false", "1", "0").
*   **Throws**: `FormatException` if the value is not a valid boolean representation; `KeyNotFoundException` if the key is missing.

### GetConfigDouble
Retrieves a configuration setting and parses it as a double-precision floating-point number.
*   **Parameters**: `string key` – The name of the configuration key.
*   **Returns**: `double` – The parsed double value.
*   **Throws**: `FormatException` if the value cannot be parsed as a double; `KeyNotFoundException` if the key is missing.

### GetEnvironment
Retrieves the current hosting environment name.
*   **Parameters**: None.
*   **Returns**: `string` – The environment name (e.g., "Development", "Production", "Staging").
*   **Throws**: None.

### IsDevelopment
Determines if the application is running in the Development environment.
*   **Parameters**: None.
*   **Returns**: `bool` – `true` if the environment is "Development"; otherwise, `false`.
*   **Throws**: None.

### IsProduction
Determines if the application is running in the Production environment.
*   **Parameters**: None.
*   **Returns**: `bool` – `true` if the environment is "Production"; otherwise, `false`.
*   **Throws**: None.

### GetDataDirectory
Retrieves the absolute path to the application's data storage directory.
*   **Parameters**: None.
*   **Returns**: `string` – The full file system path.
*   **Throws**: `InvalidOperationException` if the data directory path is not configured or invalid.

### GetLogDirectory
Retrieves the absolute path to the directory designated for log files.
*   **Parameters**: None.
*   **Returns**: `string` – The full file system path.
*   **Throws**: `InvalidOperationException` if the log directory path is not configured.

### GetTempDirectory
Retrieves the absolute path to the directory used for temporary file storage during image processing.
*   **Parameters**: None.
*   **Returns**: `string` – The full file system path.
*   **Throws**: `InvalidOperationException` if the temp directory path is not configured.

### GetMaxConcurrentOperations
Retrieves the maximum number of image processing operations allowed to run simultaneously.
*   **Parameters**: None.
*   **Returns**: `int` – The concurrency limit.
*   **Throws**: None (returns a default value if not configured).

### GetOperationTimeoutSeconds
Retrieves the timeout duration for individual processing operations.
*   **Parameters**: None.
*   **Returns**: `int` – The timeout value in seconds.
*   **Throws**: None (returns a default value if not configured).

### GetPreferredDeviceId
Retrieves the identifier for the preferred GPU device to be used for acceleration.
*   **Parameters**: None.
*   **Returns**: `int` – The device ID (zero-based index).
*   **Throws**: None (returns a default value if not configured).

### GetLogLevel
Retrieves the configured logging verbosity level.
*   **Parameters**: None.
*   **Returns**: `string` – The log level (e.g., "Information", "Warning", "Error", "Debug").
*   **Throws**: None.

### GetEnablePerformanceLogging
Determines whether performance metrics and timing data should be logged.
*   **Parameters**: None.
*   **Returns**: `bool` – `true` if performance logging is enabled; otherwise, `false`.
*   **Throws**: None.

### GetEnableDebugLogging
Determines whether detailed debug messages should be included in the logs.
*   **Parameters**: None.
*   **Returns**: `bool` – `true` if debug logging is enabled; otherwise, `false`.
*   **Throws**: None.

### GetCacheSizeMb
Retrieves the maximum size allocated for the image processing cache.
*   **Parameters**: None.
*   **Returns**: `int` – The cache size in megabytes.
*   **Throws**: None (returns a default value if not configured).

### GetUseGpuAcceleration
Determines whether GPU acceleration is enabled for image processing tasks.
*   **Parameters**: None.
*   **Returns**: `bool` – `true` if GPU acceleration is active; otherwise, `false`.
*   **Throws**: None.

### GetDefaultProfile
Retrieves the name of the default processing profile to apply when none is specified.
*   **Parameters**: None.
*   **Returns**: `string` – The profile name.
*   **Throws**: None (returns a default value if not configured).

### BuildConfigurationDictionary
Aggregates all relevant configuration settings into a single dictionary for serialization or diagnostic output.
*   **Parameters**: None.
*   **Returns**: `Dictionary<string, string>` – A collection of key-value pairs representing the current configuration state. Sensitive values may be masked.
*   **Throws**: None.

## Usage

### Example 1: Initializing a Processing Pipeline
The following example demonstrates how to configure a processing pipeline by retrieving GPU settings, concurrency limits, and directory paths before starting operations.

```csharp
using System;
using System.IO;

public class PipelineInitializer
{
    public void Initialize()
    {
        if (!ConfigurationUtilities.GetUseGpuAcceleration())
        {
            Console.WriteLine("GPU acceleration is disabled. Falling back to CPU.");
        }
        else
        {
            int deviceId = ConfigurationUtilities.GetPreferredDeviceId();
            Console.WriteLine($"Initializing pipeline on GPU Device ID: {deviceId}");
        }

        int maxThreads = ConfigurationUtilities.GetMaxConcurrentOperations();
        int timeout = ConfigurationUtilities.GetOperationTimeoutSeconds();
        
        string tempPath = ConfigurationUtilities.GetTempDirectory();
        if (!Directory.Exists(tempPath))
        {
            Directory.CreateDirectory(tempPath);
        }

        Console.WriteLine($"Pipeline configured: MaxConcurrency={maxThreads}, Timeout={timeout}s");
    }
}
```

### Example 2: Conditional Logging and Diagnostics
This example shows how to adjust logging behavior and gather diagnostic information based on the current environment and specific boolean flags.

```csharp
using System;
using System.Collections.Generic;

public class DiagnosticService
{
    public void RunDiagnostics()
    {
        if (ConfigurationUtilities.IsDevelopment())
        {
            Console.WriteLine("Running in Development mode. Debug logging enabled: " + 
                ConfigurationUtilities.GetEnableDebugLogging());
        }
        else if (ConfigurationUtilities.IsProduction())
        {
            // Ensure performance logging is off in production unless explicitly requested
            if (!ConfigurationUtilities.GetEnablePerformanceLogging())
            {
                Console.WriteLine("Production mode: Performance logging is disabled by default.");
            }
        }

        // Gather full config for health check endpoint
        Dictionary<string, string> configSnapshot = ConfigurationUtilities.BuildConfigurationDictionary();
        
        Console.WriteLine($"Current Log Level: {ConfigurationUtilities.GetLogLevel()}");
        Console.WriteLine($"Cache Limit: {ConfigurationUtilities.GetCacheSizeMb()} MB");
    }
}
```

## Notes

*   **Thread Safety**: All methods in `ConfigurationUtilities` are static and designed to be thread-safe. They read from immutable or concurrently safe underlying configuration sources, making them suitable for use in high-concurrency image processing scenarios without external locking.
*   **Missing Keys**: Methods that return primitive types (`GetConfigInteger`, `GetConfigBoolean`, `GetConfigDouble`) will throw exceptions if a required key is missing or malformed. In contrast, specialized getters (e.g., `GetMaxConcurrentOperations`, `GetPreferredDeviceId`) return sensible default values if the configuration key is absent, preventing runtime crashes during startup.
*   **Environment Sensitivity**: The `IsDevelopment` and `IsProduction` methods rely on the standard environment variable conventions. If the environment variable is unset, `IsDevelopment` returns `false`, effectively treating unknown environments as Production for safety-critical checks.
*   **Path Validation**: Directory retrieval methods (`GetDataDirectory`, `GetLogDirectory`, `GetTempDirectory`) verify the existence and validity of the path configuration. They throw `InvalidOperationException` rather than returning invalid paths to prevent silent failures in file I/O operations later in the execution flow.
*   **Dictionary Content**: The `BuildConfigurationDictionary` method returns a snapshot of the configuration. While useful for diagnostics, callers should be aware that certain sensitive keys (like connection strings, if present in the broader config) might be masked or excluded from this specific dictionary depending on internal security policies.
