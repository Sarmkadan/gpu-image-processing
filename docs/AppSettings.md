# AppSettings
The `AppSettings` type in the `gpu-image-processing` project serves as a central configuration hub, providing a unified interface to manage various application settings. These settings encompass a broad range of functionalities, including application metadata, performance optimization, logging, caching, and compatibility configurations. By offering a structured approach to configuring the application, `AppSettings` facilitates customization, troubleshooting, and maintenance, ensuring that the application can be tailored to meet specific requirements and operational constraints.

## API
- `ApplicationName`: A string property that holds the name of the application. It does not take any parameters and does not throw exceptions.
- `ApplicationVersion`: A string property that stores the version of the application. Similar to `ApplicationName`, it does not accept parameters and does not throw exceptions.
- `EnableGpuAcceleration`: A boolean property indicating whether GPU acceleration is enabled. It does not take parameters and does not throw exceptions.
- `MaxConcurrentOperations`: An integer property specifying the maximum number of concurrent operations allowed. It does not take parameters and does not throw exceptions.
- `OperationTimeoutMs`: An integer property that sets the operation timeout in milliseconds. It does not take parameters and does not throw exceptions.
- `OutputDirectory`: A string property that defines the output directory for processed images. It does not take parameters and does not throw exceptions.
- `CacheDirectory`: A string property specifying the directory used for caching. It does not take parameters and does not throw exceptions.
- `EnableMetricsCollection`: A boolean property that determines whether metrics collection is enabled. It does not take parameters and does not throw exceptions.
- `MetricsCollectionIntervalMs`: An integer property setting the interval for metrics collection in milliseconds. It does not take parameters and does not throw exceptions.
- `EnablePerformanceLogging`: A boolean property indicating whether performance logging is enabled. It does not take parameters and does not throw exceptions.
- `MaxBatchSize`: An integer property that sets the maximum batch size for operations. It does not take parameters and does not throw exceptions.
- `MaxMemoryPerImage`: A long property specifying the maximum memory allocated per image. It does not take parameters and does not throw exceptions.
- `MaxTotalGpuMemory`: A long property that sets the maximum total GPU memory usage. It does not take parameters and does not throw exceptions.
- `EnableCaching`: A boolean property that controls whether caching is enabled. It does not take parameters and does not throw exceptions.
- `CacheExpirMinutes`: An integer property defining the cache expiration time in minutes. It does not take parameters and does not throw exceptions.
- `SupportedImageFormats`: A list of strings property listing the supported image formats. It does not take parameters and does not throw exceptions.
- `Validate`: A boolean property indicating whether validation is enabled. It does not take parameters and does not throw exceptions.
- `ToString()`: An overridden string method that returns a string representation of the object. It does not take parameters and does not throw exceptions.

## Usage
The following examples demonstrate how to utilize the `AppSettings` type in a C# application:
```csharp
// Example 1: Basic Configuration
AppSettings settings = new AppSettings();
settings.ApplicationName = "ImageProcessor";
settings.EnableGpuAcceleration = true;
settings.MaxConcurrentOperations = 4;
Console.WriteLine(settings.ToString());

// Example 2: Advanced Configuration with Caching and Metrics
AppSettings advancedSettings = new AppSettings();
advancedSettings.OutputDirectory = @"C:\ProcessedImages";
advancedSettings.CacheDirectory = @"C:\ImageCache";
advancedSettings.EnableCaching = true;
advancedSettings.CacheExpirMinutes = 30;
advancedSettings.EnableMetricsCollection = true;
advancedSettings.MetricsCollectionIntervalMs = 1000;
Console.WriteLine(advancedSettings.ToString());
```

## Notes
When working with `AppSettings`, consider the following edge cases and thread-safety remarks:
- The `MaxConcurrentOperations` and `MaxBatchSize` properties should be carefully tuned based on the available system resources to avoid overloading the GPU or CPU.
- Enabling `EnableGpuAcceleration` requires a compatible GPU; otherwise, it may lead to performance degradation or errors.
- The `CacheExpirMinutes` property should be set based on the application's requirements and available disk space to avoid cache overflow.
- Since `AppSettings` is not explicitly marked as thread-safe, access to its properties should be synchronized in multi-threaded environments to prevent data corruption or inconsistencies.
- The `Validate` property's impact on application behavior should be thoroughly tested, as it may introduce additional overhead or affect compatibility with certain image formats.
