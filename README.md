# GPU Image Processing

GPU-accelerated image processing in C# using OpenCL (Silk.NET) - filters, transforms,
batch operations, with a byte-exact CPU fallback that keeps everything usable on
machines without a GPU.

## Architecture

The solution is split into a reusable library core (`src/`) and a console application
shell (root folders: CLI, API facade, middleware, workers). How the projects fit
together, the data flow, key design decisions and extension points are documented in
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

The sections below are per-type reference notes with usage examples.

## GpuException

The `GpuException` is thrown when a GPU operation fails or when the requested GPU device is unavailable. It provides comprehensive diagnostic information, including the name of the affected device, a specific error code, and the timestamp when the failure occurred, making it easier to troubleshoot GPU-related issues in the processing pipeline.

### Usage Example

```csharp
using GpuImageProcessing.Core;
using System;

try
{
    // Simulate a GPU failure
    throw new GpuException("Failed to initialize GPU device.", "NVIDIA GeForce RTX 4090", 500);
}
catch (GpuException ex)
{
    Console.WriteLine($"Exception Message: {ex.Message}");
    Console.WriteLine($"Device: {ex.DeviceName ?? "Unknown"}");
    Console.WriteLine($"Error Code: {ex.ErrorCode?.ToString() ?? "N/A"}");
    Console.WriteLine($"Occurred At: {ex.OccurredAt}");

    // Using the overridden ToString() for detailed logging
    Console.WriteLine($"Full Exception Details:\n{ex}");
}
```

## SimdCapabilities

The `SimdCapabilities` class provides a runtime-detected snapshot of the CPU's SIMD instruction set capabilities. It exposes boolean flags for each supported SIMD level (SSE2, SSE4.1, AVX, AVX2, AVX-512F), the highest available level via `BestAvailableLevel`, and the native vector register width in bytes via `VectorWidthBytes`. Use `Detect()` to probe the current CPU and cache the result per-process; the returned instance is immutable and thread-safe.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;

// Detect available SIMD capabilities on the current CPU
var capabilities = SimdCapabilities.Detect();

Console.WriteLine($"SIMD Detection Results:");
Console.WriteLine($"  SSE2 support: {capabilities.SupportsSSE2}");
Console.WriteLine($"  SSE4.1 support: {capabilities.SupportsSse41}");
Console.WriteLine($"  AVX support: {capabilities.SupportsAvx}");
Console.WriteLine($"  AVX2 support: {capabilities.SupportsAvx2}");
Console.WriteLine($"  AVX-512F support: {capabilities.SupportsAvx512F}");
Console.WriteLine($"  Best available level: {capabilities.BestAvailableLevel}");
Console.WriteLine($"  Vector width: {capabilities.VectorWidthBytes} bytes");
Console.WriteLine($"  Any SIMD available: {capabilities.IsAnySimdAvailable}");

// Choose code paths based on detected capabilities
if (capabilities.SupportsAvx2)
{
    Console.WriteLine("Using AVX2-accelerated processing path");
    // Initialize AVX2-specific processing pipeline
}
else if (capabilities.SupportsSse41)
{
    Console.WriteLine("Using SSE4.1-accelerated processing path");
    // Initialize SSE4.1-specific processing pipeline
}
else
{
    Console.WriteLine("Using scalar processing path (no SIMD acceleration)");
    // Initialize fallback scalar processing pipeline
}

// Display human-readable summary
Console.WriteLine($"Full capabilities: {capabilities}");
```


## EnumerableExtensionsBenchmarks

The `EnumerableExtensionsBenchmarks` class provides performance benchmarks for common `IEnumerable<T>` operations used throughout the GPU batch processing pipeline. These include shuffling, batching, deduplication, and dictionary conversion methods that are critical for organizing and processing image batches efficiently.

### Usage Example

```csharp
using GpuImageProcessing.Benchmarks;
using GpuImageProcessing.Utilities;

// Create benchmark instance
var benchmarks = new EnumerableExtensionsBenchmarks();

// Setup test data (required before running benchmarks)
benchmarks.Setup();

// Benchmark shuffling operations
var shuffled32 = benchmarks.Shuffle_32Items();
var shuffled1024 = benchmarks.Shuffle_1024Items();

// Benchmark batching operations
int batch32Count = benchmarks.Batch_1000By32();
int batch8Count = benchmarks.Batch_1000By8();

// Benchmark deduplication
int distinctCount = benchmarks.DistinctBy_1000Strings();

// Benchmark dictionary conversion
var dictionary = benchmarks.SafeToDictionary_1000Items();

Console.WriteLine($"Batch 32 count: {batch32Count}");
Console.WriteLine($"Batch 8 count: {batch8Count}");
Console.WriteLine($"Distinct count: {distinctCount}");
Console.WriteLine($"Dictionary count: {dictionary.Count}");
```

## ImageRegisteredEvent

The `ImageRegisteredEvent` is a domain event that is published when an image is registered for processing. It contains metadata about the image, including its ID, path, width, height, and description. This event can be used to trigger subsequent processing steps or to update external systems.

### Usage Example

```csharp
using GpuImageProcessing.Events;

// Register an image
var imageRegisteredEvent = new ImageRegisteredEvent
{
    ImageId = Guid.NewGuid(),
    ImagePath = "/path/to/image.jpg",
    Width = 1920,
    Height = 1080,
    Description = "Example image"
};

// Publish the event (e.g., using an event aggregator)
var eventAggregator = new EventAggregator();
eventAggregator.Publish(imageRegisteredEvent);
```

## EventPublisher

`EventPublisher` provides a lightweight, in‑process publish‑subscribe mechanism for processing events. It lets you register asynchronous or synchronous handlers for a named event type, publish events to all registered handlers, and manage subscriptions (unsubscribe, clear, query counts, and list event types).

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GpuImageProcessing.Events;

class Program
{
    static async Task Main()
    {
        // Create a logger (console logger for the example)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<EventPublisher> logger = loggerFactory.CreateLogger<EventPublisher>();

        // Instantiate the publisher
        var publisher = new EventPublisher(logger);

        // Define an async handler for ImageRegisteredEvent
        async Task HandleImageRegisteredAsync(ImageRegisteredEvent ev)
        {
            Console.WriteLine($"[Async] Image registered: {ev.ImageId}");
            await Task.CompletedTask;
        }

        // Subscribe the handler to the "ImageRegistered" event type
        publisher.Subscribe<ImageRegisteredEvent>("ImageRegistered", HandleImageRegisteredAsync);

        // Create an event instance
        var imageEvent = new ImageRegisteredEvent
        {
            ImageId = Guid.NewGuid(),
            ImagePath = "/images/sample.jpg",
            Width = 1280,
            Height = 720,
            Description = "Sample image"
        };

        // Publish the event – all subscribed handlers will be invoked
        await publisher.PublishAsync(imageEvent);

        // Query subscription information
        int subscriberCount = publisher.GetSubscriberCount("ImageRegistered");
        Console.WriteLine($"Subscribers for 'ImageRegistered': {subscriberCount}");

        var eventTypes = publisher.GetEventTypes();
        Console.WriteLine("Registered event types: " + string.Join(", ", eventTypes));

        // Unsubscribe the handler when it is no longer needed
        publisher.Unsubscribe<ImageRegisteredEvent>("ImageRegistered", (Delegate)HandleImageRegisteredAsync);
    }
}
```

## EventAggregator

The `EventAggregator` is a centralized event bus that simplifies communication between components in a decoupled manner. It provides both synchronous and asynchronous event publishing capabilities, allowing you to subscribe to specific event types, track subscription statistics, and manage the lifecycle of subscriptions through disposable handles.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using GpuImageProcessing.Events;

class Program
{
    static async Task Main()
    {
        // Create an event aggregator instance
        var eventAggregator = new EventAggregator();

        // Subscribe to ImageRegisteredEvent synchronously
        using var syncSubscription = eventAggregator.Subscribe<ImageRegisteredEvent>(
            ev => Console.WriteLine($"[Sync] Image registered: {ev.ImageId}"));

        // Subscribe to ImageRegisteredEvent asynchronously
        using var asyncSubscription = await eventAggregator.SubscribeAsync<ImageRegisteredEvent>(
            async ev => 
            {
                Console.WriteLine($"[Async] Processing image: {ev.ImageId}");
                await Task.Delay(100); // Simulate async work
                Console.WriteLine($"[Async] Completed processing: {ev.ImageId}");
            }
        );

        // Create and publish an event
        var imageEvent = new ImageRegisteredEvent
        {
            ImageId = Guid.NewGuid(),
            ImagePath = "/images/sample.jpg",
            Width = 1920,
            Height = 1080,
            Description = "Sample image for processing"
        };

        // Publish synchronously - all synchronous subscribers will be invoked immediately
        eventAggregator.Publish(imageEvent);

        // Publish asynchronously - all subscribers (sync and async) will be invoked
        await eventAggregator.PublishAsync(imageEvent);

        // Get subscription statistics
        var stats = eventAggregator.GetStats();
        Console.WriteLine($"Total event types: {stats.TotalEventTypes}");
        Console.WriteLine($"Total subscriptions: {stats.TotalSubscriptions}");
        Console.WriteLine($"Event types: {string.Join(", ", stats.EventTypes)}");

        // Dispose the aggregator to clean up all subscriptions
        // Note: Disposing the aggregator also disposes all subscription handles
        eventAggregator.Dispose();
    }
}
```

## BatchProcessingBenchmarks

The `BatchProcessingBenchmarks` class provides performance benchmarks for core batch processing operations used in the GPU image processing pipeline. It measures critical hot-paths such as batch creation, validation, progress tracking, success rate calculation, and priority queue construction that are called repeatedly during batch execution.

### Usage Example

```csharp
using GpuImageProcessing.Benchmarks;
using GpuImageProcessing.Domain;

// Create benchmark instance
var benchmarks = new BatchProcessingBenchmarks();

// Configure the number of images to process
benchmarks.ImageCount = 100;

// Setup test data (required before running benchmarks)
benchmarks.Setup();

// Benchmark batch creation and population
var populatedBatch = benchmarks.CreateAndPopulateBatch();

// Benchmark batch validation
bool isValid = benchmarks.ValidateBatch();

// Benchmark progress tracking
double progress = benchmarks.GetProgressPercentage();

// Benchmark success rate calculation
double successRate = benchmarks.GetSuccessRate();

// Benchmark remaining time estimation
TimeSpan? remainingTime = benchmarks.GetEstimatedRemainingTime();

// Benchmark priority queue construction
var priorityQueue = benchmarks.BuildPriorityQueue();

// Benchmark image processing in the hot loop
benchmarks.MarkImageProcessed_TenSuccesses();

Console.WriteLine($"Progress: {progress:P0}");
Console.WriteLine($"Success rate: {successRate:P0}");
Console.WriteLine($"Remaining time: {remainingTime?.ToString() ?? "N/A"}");
```

## ImageUtilitiesBenchmarks

The `ImageUtilitiesBenchmarks` class provides performance benchmarks for the `ImageUtilities` hot paths that are called per-image during the ingestion pipeline. These include file extension validation, MIME type resolution, image format detection, file size formatting, and proportional size calculations that are critical for image processing workflows.

### Usage Example

```csharp
using GpuImageProcessing.Benchmarks;
using GpuImageProcessing.Utilities;

// Create benchmark instance
var benchmarks = new ImageUtilitiesBenchmarks();

// Setup test data (required before running benchmarks)
// Note: BenchmarkDotNet will handle setup automatically

// Benchmark image file extension validation
bool isJpegSupported = benchmarks.IsSupportedImageFile_Jpeg();
bool isWebPSupported = benchmarks.IsSupportedImageFile_WebP();
bool isUnsupported = benchmarks.IsSupportedImageFile_Unsupported();

// Benchmark file size formatting
string kbSize = benchmarks.FormatFileSize_Kilobytes();
string mbSize = benchmarks.FormatFileSize_Megabytes();
string gbSize = benchmarks.FormatFileSize_Gigabytes();

// Benchmark MIME type resolution
string? jpegMime = benchmarks.GetMimeType_Jpeg();
string? pngMime = benchmarks.GetMimeType_Png();
string? tiffFormat = benchmarks.GetImageFormat_Tiff();

// Benchmark proportional size calculations
(int width, int height) = benchmarks.CalculateProportionalSize_2x();

Console.WriteLine($"JPEG supported: {isJpegSupported}");
Console.WriteLine($"WebP supported: {isWebPSupported}");
Console.WriteLine($"Unsupported file detected: {isUnsupported}");
Console.WriteLine($"MIME types - JPEG: {jpegMime}, PNG: {pngMime}");
Console.WriteLine($"TIFF format detected: {tiffFormat}");
Console.WriteLine($"Proportional size: {width}x{height}");
Console.WriteLine($"File sizes - KB: {kbSize}, MB: {mbSize}, GB: {gbSize}");
```

## GpuPerformanceBenchmarks

The `GpuPerformanceBenchmarks` class provides performance benchmarks for GPU-accelerated image processing operations. It measures throughput and latency of critical GPU operations including single filter applications, multiple filter chains, batch processing throughput, memory allocation patterns, and device initialization overhead. These benchmarks help identify performance bottlenecks and optimize GPU utilization in image processing workflows.

### Usage Example

```csharp
using GpuImageProcessing.Benchmarks;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.DependencyInjection;

// Create benchmark instance and configure image dimensions
var benchmarks = new GpuPerformanceBenchmarks
{
    ImageWidth = 1920,  // 1080p
    ImageHeight = 1080
};

// Setup test data and services (required before running benchmarks)
benchmarks.Setup();

// Benchmark single filter applications
var gaussianBlurResult = await benchmarks.ApplyGaussianBlurFilter();
var edgeDetectionResult = await benchmarks.ApplyEdgeDetectionFilter();
var sharpenResult = await benchmarks.ApplySharpenFilter();
var grayscaleResult = await benchmarks.ApplyGrayscaleFilter();
var customConvolutionResult = await benchmarks.ApplyCustomConvolutionFilter();

// Benchmark realistic filter chains
var threeFilterChainResult = await benchmarks.ApplyThreeFilterChain();
var fiveFilterChainResult = await benchmarks.ApplyFiveFilterChain();

// Benchmark memory allocation patterns
long pixelDataSize = benchmarks.CalculatePixelDataSize();
long memoryFootprint1080p = benchmarks.MemoryFootprint1080p();
long memoryFootprint4K = benchmarks.MemoryFootprint4K();

// Benchmark throughput
await benchmarks.ProcessTenImages();

// Benchmark device capabilities
var bestDevice = await benchmarks.GetBestDevice();

// Benchmark service provider creation
var serviceProvider = GpuPerformanceBenchmarks.CreateServiceProvider();

Console.WriteLine($"Gaussian blur result: {gaussianBlurResult.Status}");
Console.WriteLine($"Edge detection result: {edgeDetectionResult.Status}");
Console.WriteLine($"Memory footprint 1080p: {memoryFootprint1080p:N0} bytes");
Console.WriteLine($"Memory footprint 4K: {memoryFootprint4K:N0} bytes");
Console.WriteLine($"Best device: {bestDevice?.Name ?? "None found"}");
```

## GpuDevice

The `GpuDevice` class represents a physical GPU available for processing tasks, encapsulating hardware specifications and operational capabilities. It provides detailed metrics such as memory capacity, compute unit counts, and supported precision types, allowing the application to intelligently select devices based on workload requirements.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;

// Initialize a new GPU device representation
var device = new GpuDevice
{
    Id = Guid.NewGuid(),
    Name = "NVIDIA GeForce RTX 4090",
    DeviceType = GpuDeviceType.Discrete,
    GlobalMemoryBytes = 24L * 1024 * 1024 * 1024, // 24 GB
    IsAvailable = true
};

// Check if the device meets workload requirements
if (device.IsAvailable && device.GlobalMemoryBytes > 8 * 1024 * 1024 * 1024)
{
    Console.WriteLine($"Selected GPU: {device.Name}");
    Console.WriteLine($"Vendor: {device.Vendor}");
    Console.WriteLine($"Max Compute Units: {device.MaxComputeUnits}");
}
```

## ValidationException

The `ValidationException` is thrown when input validation fails during GPU image processing operations. It provides detailed information about the validation failure including the name of the validated entity and a dictionary of validation errors with field names as keys and error messages as values. This exception is particularly useful for batch processing pipelines where multiple images or entities need to be validated before processing begins.

### Usage Example

```csharp
using GpuImageProcessing.Exceptions;
using System;
using System.Collections.Generic;

// Example: Validating image metadata before processing
try
{
    var imageMetadata = new Dictionary<string, object>
    {
        { "Width", 0 },
        { "Height", -100 },
        { "Format", "invalid_format" }
    };

    ValidateImageMetadata(imageMetadata);
}
catch (ValidationException ex) when (ex.ValidationErrors != null)
{
    Console.WriteLine($"Validation failed for entity: {ex.EntityName}");
    Console.WriteLine("Validation errors:");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"  - {error.Key}: {error.Value}");
    }
    
    // Re-throw or handle the exception appropriately
    throw;
}

static void ValidateImageMetadata(Dictionary<string, object> metadata)
{
    var validationErrors = new Dictionary<string, string>();
    
    if (!metadata.TryGetValue("Width", out var widthObj) || 
        (widthObj is int width && width <= 0))
    {
        validationErrors[nameof(width)] = "Width must be a positive integer";
    }
    
    if (!metadata.TryGetValue("Height", out var heightObj) || 
        (heightObj is int height && height <= 0))
    {
        validationErrors[nameof(height)] = "Height must be a positive integer";
    }
    
    if (!metadata.TryGetValue("Format", out var formatObj) || 
        formatObj is not string format || 
        !IsValidImageFormat(format))
    {
        validationErrors[nameof(format)] = "Format must be a valid image format (JPEG, PNG, etc.)";
    }
    
    if (validationErrors.Count > 0)
    {
        throw new ValidationException(
            "Image metadata validation failed",
            entityName: "ImageMetadata",
            validationErrors: validationErrors,
            errorCode: 400
        );
    }
}

static bool IsValidImageFormat(string format)
{
    return format.Equals("JPEG", StringComparison.OrdinalIgnoreCase) ||
           format.Equals("PNG", StringComparison.OrdinalIgnoreCase) ||
           format.Equals("WEBP", StringComparison.OrdinalIgnoreCase);
}
```

## FilterChainBenchmarks

The `FilterChainBenchmarks` class provides performance benchmarks for core `FilterChain` operations that are critical hot paths during GPU filter pipeline setup and execution. It measures realistic in-process operations including step management, validation, querying, and cloning that are called repeatedly during batch processing workflows.


### Usage Example

```csharp
using GpuImageProcessing.Benchmarks;
using GpuImageProcessing.Domain;

// Create benchmark instance
var benchmarks = new FilterChainBenchmarks();

// Setup test data (required before running benchmarks)
benchmarks.Setup();

// Benchmark building a 10-step chain from scratch
var newChain = benchmarks.AddStep_TenFilters();

// Benchmark retrieving enabled steps from a 10-step chain
var enabledSteps = benchmarks.GetEnabledSteps_TenSteps();

// Benchmark full chain validation (called before every batch job dispatch)
bool isValid = benchmarks.Validate_TenSteps();

// Benchmark counting enabled filters without allocating a list
int enabledCount = benchmarks.GetEnabledFilterCount();

// Benchmark cloning a 10-step chain (used when duplicating profiles)
var clonedChain = benchmarks.Clone_TenStepChain();

Console.WriteLine($"New chain created: {newChain.Name}");
Console.WriteLine($"Enabled steps: {enabledSteps.Count}");
Console.WriteLine($"Chain is valid: {isValid}");
Console.WriteLine($"Enabled filter count: {enabledCount}");
Console.WriteLine($"Cloned chain has {clonedChain.GetEnabledSteps().Count} steps");
```

## ConfigurationException

The `ConfigurationException` is thrown when there is an issue with application configuration during GPU image processing operations. It provides detailed information about the configuration key and value that caused the exception, allowing for easier debugging of configuration-related issues in batch processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Exceptions;
using System;

// Example: Validating configuration before starting image processing
try
{
  var maxBatchSize = GetConfigurationValue("MaxBatchSize");
  if (maxBatchSize <= 0)
  {
    throw new ConfigurationException(
      "MaxBatchSize must be a positive integer",
      "MaxBatchSize",
      maxBatchSize.ToString()
    );
  }
}
catch (ConfigurationException ex)
{
  Console.WriteLine($"Configuration error occurred: {ex.Message}");
  Console.WriteLine($"Configuration key: {ex.ConfigurationKey}");
  Console.WriteLine($"Configuration value: {ex.ConfigurationValue}");
  Console.WriteLine($"Exception details: {ex}");

  // Log the error and use a default value or exit gracefully
  Logger.LogError(ex, "Invalid configuration detected");
  Environment.Exit(1);
}

static int GetConfigurationValue(string key)
{
  // Simulate getting configuration value from app settings
  return key switch
  {
    "MaxBatchSize" => -1, // Invalid configuration
    _ => 10
  };
}
```

## ProcessingException

The `ProcessingException` is thrown when an image processing operation fails during the execution of a filter chain. It encapsulates diagnostic information about the failed operation, including the path of the image being processed, the name of the filter that was being applied, and the attempt number, which facilitates better error handling and logging in batch processing workflows.

### Usage Example

```csharp
using GpuImageProcessing.Core;
using System;

try
{
    // Simulate a processing failure
    throw new ProcessingException(
        "Failed to apply GaussianBlur filter.",
        imagePath: "/images/input.jpg",
        filterName: "GaussianBlur",
        attemptNumber: 1
    );
}
catch (ProcessingException ex)
{
    Console.WriteLine($"Exception Message: {ex.Message}");
    Console.WriteLine($"Image Path: {ex.ImagePath ?? "N/A"}");
    Console.WriteLine($"Filter Name: {ex.FilterName ?? "N/A"}");
    Console.WriteLine($"Attempt Number: {ex.AttemptNumber?.ToString() ?? "N/A"}");
    }
    ```

## ImageBatch

The `ImageBatch` class represents a collection of images to be processed together as a single unit. It tracks processing status, manages image and filter collections, and provides progress tracking and performance metrics. Batches can be validated before processing and support adding/removing images and filters dynamically.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Create a new image batch
var batch = new ImageBatch
{
    Name = "SummerPhotos-2024",
    Description = "Batch processing summer vacation photos with enhancement filters",
    OutputDirectory = "/output/summer-2024-enchanced"
};

// Add images to the batch
batch.AddImage(Guid.NewGuid()); // First image
batch.AddImage(Guid.NewGuid()); // Second image
batch.AddImage(Guid.NewGuid()); // Third image

// Add filters to apply to all images in the batch
batch.AddFilter(Guid.NewGuid()); // Enhancement filter
batch.AddFilter(Guid.NewGuid()); // Color correction filter

// Set custom batch options
batch.BatchOptions["QualityThreshold"] = 0.95;
batch.BatchOptions["MaxMemoryUsage"] = 2L * 1024 * 1024 * 1024; // 2GB

// Validate the batch before processing
if (batch.Validate())
{
    Console.WriteLine($"Batch {batch.Name} is valid and ready for processing.");
    Console.WriteLine($"Total images: {batch.TotalImages}");
    Console.WriteLine($"Total filters: {batch.FilterIds.Count}");
    
    // Start processing
    batch.Start();
    
    // Simulate processing progress
    batch.MarkImageProcessed(success: true);
    batch.MarkImageProcessed(success: true);
    batch.MarkImageProcessed(success: false); // One failed
    
    // Check progress
    double progress = batch.GetProgressPercentage();
    double successRate = batch.GetSuccessRate();
    
    Console.WriteLine($"Progress: {progress:P0}");
    Console.WriteLine($"Success rate: {successRate:P0}");
    
    // Complete processing
    batch.Complete();
    Console.WriteLine($"Processing completed at: {batch.CompletedAt}");
}
```

    ## BatchProcessingPipeline

    The `BatchProcessingPipeline` provides a robust, stage-aware mechanism for executing batch image processing jobs with integrated retry policies and progress reporting. It orchestrates image ingestion through pre-processing, GPU filtering, and post-processing stages, offering fine-grained control over concurrency and fault tolerance.

    ### Usage Example

    ```csharp
    using GpuImageProcessing.Pipeline;
    using GpuImageProcessing.Services;
    using GpuImageProcessing.Domain;
    using Microsoft.Extensions.Logging;

    // Assuming services are initialized (e.g., via dependency injection)
    var options = new BatchPipelineOptions
    {
    MaxConcurrency = 8,
    MaxRetries = 3,
    RetryBaseDelayMs = 200
    };
    var pipeline = new BatchProcessingPipeline(processingService, perfMonitor, options, logger);

    // Prepare the batch
    var batch = new ImageBatch
    {
    Name = "Batch-001",
    OutputDirectory = "./output"
    };
    batch.AddImage(Guid.NewGuid());
    batch.AddFilter(Guid.NewGuid());

    // Run the pipeline
    BatchPipelineResult result = await pipeline.RunAsync(batch);


## ComputeShaderPipelineOptions

The `ComputeShaderPipelineOptions` class provides runtime configuration for the compute shader pipeline and the automatic workgroup optimizer. It controls optimization strategies, workgroup dimensions, benchmarking behavior, profiling output, pipeline depth limits, memory allocation settings, and validation thresholds that govern how compute shader operations are executed and optimized across GPU devices.

### Usage Example

```csharp
using GpuImageProcessing.Configuration;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

// Configure pipeline options inline
var services = new ServiceCollection();

services.AddComputeShaderPipeline(options =>
{
    options.DefaultStrategy = WorkgroupOptimizationStrategy.Balanced;
    options.MaxWorkgroupDimension = 32;
    options.BenchmarkGuidedOptimization = true;
    options.EnableProfiling = true;
    options.MaxPipelineDepth = 64;
    options.DefaultLocalMemoryPerThreadBytes = 16;
    options.OccupancyWarningThreshold = 0.3;
});

// Or bind from appsettings.json configuration
// services.AddComputeShaderPipeline(configuration);

// Build service provider and log settings for diagnostics
var provider = services.BuildServiceProvider();
provider.LogComputeShaderPipelineSettings();

// Use the configured pipeline in your application
var pipeline = provider.GetRequiredService<IComputeShaderPipeline>();
```

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Assuming sourceImage and targetImage are initialized Image instances
var pass = new ComputeShaderPass(
    "ApplyGrayscale",
    "__kernel void ApplyGrayscale(...)",
    ShaderPassType.ColorTransform,
    priority: 1
);

// Configure pass inputs and parameters
pass.InputImages.Add(sourceImage);
pass.OutputImage = targetImage;
pass.Parameters["Intensity"] = 0.5f;

// Check readiness before dispatch
if (pass.IsReady())
{
    Console.WriteLine($"Pass '{pass.KernelName}' (ID: {pass.Id}) is ready for execution.");
}
```

## ComputeShaderPipeline

The `ComputeShaderPipeline` class orchestrates compute shader execution on GPU devices, handling workgroup optimization, per-pass profiling, and GPU memory lifecycle management. It dispatches sequential compute shader passes based on their priority and automatically optimizes workgroup configurations to ensure efficient GPU utilization.

### Usage Example

```csharp
using GpuImageProcessing.Pipeline;
using GpuImageProcessing.Domain;
using System;
using System.Threading.Tasks;

// Assuming dependencies are initialized (e.g., via dependency injection)
var pipeline = new ComputeShaderPipeline(optimizer, gpuService, perfMonitor, options, logger);

// Run the pipeline for a set of passes
var result = await pipeline.ExecuteAsync(passes, deviceId);

// Optimize workgroup configuration for a specific pass
var workgroupConfig = await pipeline.OptimizeWorkgroupAsync(pass, deviceId);

// Retrieve and reset pipeline performance statistics
var stats = await pipeline.GetStatisticsAsync();
await pipeline.ResetStatisticsAsync();



## FilterConfigurationRepository

The `FilterConfigurationRepository` class provides data access operations for `FilterConfiguration` entities, implementing a repository pattern for managing filter configurations in memory. It offers comprehensive CRUD operations along with specialized query methods for filtering configurations by various criteria such as type, name, parameters, and kernel availability. This repository is particularly useful for managing filter presets and configurations used throughout the GPU image processing pipeline.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create repository instance
        var repository = new FilterConfigurationRepository();

        // Create and add a filter configuration
        var blurFilter = new FilterConfiguration
        {
            Name = "GaussianBlur",
            FilterType = FilterType.Blur,
            Description = "Gaussian blur filter for image smoothing",
            IsActive = true,
            Priority = 1,
            KernelCode = "__kernel void GaussianBlur(__global float4* input, __global float4* output, int radius)"
        };
        blurFilter.SetParameter("Radius", 5);
        blurFilter.ParameterTypes["Radius"] = "System.Int32";

        var createdFilter = await repository.CreateAsync(blurFilter);
        Console.WriteLine($"Created filter: {createdFilter.Name} with ID: {createdFilter.Id}");

        // Create and add another filter configuration
        var grayscaleFilter = new FilterConfiguration
        {
            Name = "Grayscale",
            FilterType = FilterType.ColorTransform,
            Description = "Convert image to grayscale",
            IsActive = true,
            Priority = 2
        };
        grayscaleFilter.SetParameter("Intensity", 1.0f);
        grayscaleFilter.ParameterTypes["Intensity"] = "System.Single";

        await repository.CreateAsync(grayscaleFilter);

        // Get all filters
        var allFilters = await repository.GetAllAsync();
        Console.WriteLine($"Total filters: {allFilters.Count()}");

        // Get filter by ID
        var retrievedFilter = await repository.GetByIdAsync(createdFilter.Id);
        if (retrievedFilter != null)
        {
            Console.WriteLine($"Retrieved filter: {retrievedFilter.Name}");
        }

        // Get filters by type
        var blurFilters = await repository.GetByTypeAsync(FilterType.Blur);
        Console.WriteLine($"Blur filters count: {blurFilters.Count()}");

        // Get active filters sorted by priority
        var activeFilters = await repository.GetActiveFiltersAsync();
        Console.WriteLine($"Active filters count: {activeFilters.Count()}");

        // Get filter by name
        var grayscaleByName = await repository.GetByNameAsync("Grayscale");
        if (grayscaleByName != null)
        {
            Console.WriteLine($"Found filter by name: {grayscaleByName.Name}");
        }

        // Get filters with specific parameter
        var filtersWithIntensity = await repository.GetByParameterAsync("Intensity");
        Console.WriteLine($"Filters with Intensity parameter: {filtersWithIntensity.Count()}");

        // Get filters with kernel code
        var filtersWithKernel = await repository.GetFiltersWithKernelAsync();
        Console.WriteLine($"Filters with kernel code: {filtersWithKernel.Count()}");

        // Update a filter
        createdFilter.Description = "Updated: Gaussian blur filter for image smoothing";
        var updatedFilter = await repository.UpdateAsync(createdFilter);
        Console.WriteLine($"Updated filter: {updatedFilter.Description}");

        // Check if filter exists
        var exists = await repository.ExistsAsync(createdFilter.Id);
        Console.WriteLine($"Filter exists: {exists}");

        // Count filters
        var filterCount = await repository.CountAsync();
        Console.WriteLine($"Total filter count: {filterCount}");

        // Get paged results
        var page1 = await repository.GetPagedAsync(1, 1);
        Console.WriteLine($"Page 1 has {page1.Count()} filters");

        // Delete a filter
        var deleteSuccess = await repository.DeleteAsync(createdFilter.Id);
        Console.WriteLine($"Filter deleted: {deleteSuccess}");

        // Verify deletion
        var filterAfterDelete = await repository.GetByIdAsync(createdFilter.Id);
        Console.WriteLine($"Filter still exists after delete: {filterAfterDelete != null}");
    }
}
```

## ImageRepository

The `ImageRepository` class provides data access operations for `Image` entities, implementing a repository pattern for managing images in memory. It offers comprehensive CRUD operations along with specialized query methods for filtering images by status, format, size, date ranges, and other criteria. This repository is particularly useful for managing the collection of images being processed in the GPU image processing pipeline.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
static async Task Main()
{
// Create repository instance
var repository = new ImageRepository();

// Create and add an image
var image = new Image(1920, 1080, 3)
{
FileName = "sample_image.jpg",
FilePath = "/data/sample_image.jpg",
Format = ImageFormat.Jpeg,
ColorSpace = ColorSpace.Rgb,
BitsPerPixel = 24,
Status = ProcessingStatus.Pending
};

var createdImage = await repository.CreateAsync(image);
Console.WriteLine($"Created image: {createdImage.FileName} with ID: {createdImage.Id}");

// Create and add another image
var image2 = new Image(3840, 2160, 3)
{
FileName = "high_res_image.png",
FilePath = "/data/high_res_image.png",
Format = ImageFormat.Png,
ColorSpace = ColorSpace.Rgb,
BitsPerPixel = 24,
Status = ProcessingStatus.Pending
};

await repository.CreateAsync(image2);

// Get all images
var allImages = await repository.GetAllAsync();
Console.WriteLine($"Total images: {allImages.Count()}");

// Get image by ID
var retrievedImage = await repository.GetByIdAsync(createdImage.Id);
if (retrievedImage != null)
{
Console.WriteLine($"Retrieved image: {retrievedImage.FileName}");
}

// Get images by status
var pendingImages = await repository.GetByStatusAsync(ProcessingStatus.Pending);
Console.WriteLine($"Pending images count: {pendingImages.Count()}");

var failedImages = await repository.GetFailedImagesAsync();
Console.WriteLine($"Failed images count: {failedImages.Count()}");

// Get images by format
var jpegImages = await repository.GetByFormatAsync(ImageFormat.Jpeg);
Console.WriteLine($"JPEG images count: {jpegImages.Count()}");

var pngImages = await repository.GetByFormatAsync(ImageFormat.Png);
Console.WriteLine($"PNG images count: {pngImages.Count()}");

// Get images within size range
var smallImages = await repository.GetBySizeRangeAsync(0, 1920, 0, 1080);
Console.WriteLine($"Small images count: {smallImages.Count()}");

var largeImages = await repository.GetBySizeRangeAsync(1921, 9999, 1081, 9999);
Console.WriteLine($"Large images count: {largeImages.Count()}");

// Get images by date range
var recentImages = await repository.GetByDateRangeAsync(
DateTime.UtcNow.AddDays(-7),
DateTime.UtcNow
);
Console.WriteLine($"Recent images count: {recentImages.Count()}");

// Update an image
retrievedImage.Status = ProcessingStatus.Processed;
var updatedImage = await repository.UpdateAsync(retrievedImage);
Console.WriteLine($"Updated image status: {updatedImage.Status}");

// Check if image exists
var exists = await repository.ExistsAsync(createdImage.Id);
Console.WriteLine($"Image exists: {exists}");

// Count images
var imageCount = await repository.CountAsync();
Console.WriteLine($"Total image count: {imageCount}");

// Get paged results
var page1 = await repository.GetPagedAsync(1, 10);
Console.WriteLine($"Page 1 has {page1.Count()} images");

// Delete an image
var deleteSuccess = await repository.DeleteAsync(createdImage.Id);
Console.WriteLine($"Image deleted: {deleteSuccess}");

// Verify deletion
var imageAfterDelete = await repository.GetByIdAsync(createdImage.Id);
Console.WriteLine($"Image still exists after delete: {imageAfterDelete != null}");
}
}
```

## ProcessingResultRepository

The `ProcessingResultRepository` class provides data access operations for `ProcessingResult` entities, implementing a repository pattern for managing processing results in memory. It offers comprehensive CRUD operations along with specialized query methods for filtering results by image ID, processing status, success/failure state, time ranges, and performance metrics. This repository is particularly useful for tracking and analyzing image processing operations throughout the GPU pipeline.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using GpuImageProcessing.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
static async Task Main()
{
// Create repository instance
var repository = new ProcessingResultRepository();

// Create and add processing results for different images
var result1 = new ProcessingResult
{
ImageId = Guid.NewGuid(),
OutputPath = "/output/processed_image1.png",
Status = ProcessingStatus.Processed,
IsSuccessful = true,
ProcessingTimeMilliseconds = 45.2,
CompletedAt = DateTime.UtcNow.AddMinutes(-10)
};
result1.AddFilterApplied("GaussianBlur", FilterType.Blur, 12.5);
result1.AddFilterApplied("Sharpen", FilterType.Enhancement, 8.3);

var result2 = new ProcessingResult
{
ImageId = Guid.NewGuid(),
OutputPath = "/output/processed_image2.jpg",
Status = ProcessingStatus.Processed,
IsSuccessful = true,
ProcessingTimeMilliseconds = 32.8,
CompletedAt = DateTime.UtcNow.AddMinutes(-5)
};
result2.AddFilterApplied("Grayscale", FilterType.ColorTransform, 5.1);

var result3 = new ProcessingResult
{
ImageId = Guid.NewGuid(),
OutputPath = "/output/failed_image.png",
Status = ProcessingStatus.Failed,
IsSuccessful = false,
ErrorMessage = "GPU memory allocation failed",
ProcessingTimeMilliseconds = 2.1,
CompletedAt = DateTime.UtcNow.AddMinutes(-2)
};

// Add results to repository
await repository.CreateAsync(result1);
await repository.CreateAsync(result2);
await repository.CreateAsync(result3);

Console.WriteLine("Created 3 processing results");

// Get all results
var allResults = await repository.GetAllAsync();
Console.WriteLine($"Total results: {allResults.Count()}");

// Get result by ID
var retrievedResult = await repository.GetByIdAsync(result1.Id);
if (retrievedResult != null)
{
Console.WriteLine($"Retrieved result for image ID: {retrievedResult.ImageId}");
}

// Get results by image ID
var resultsForImage = await repository.GetByImageIdAsync(result1.ImageId);
Console.WriteLine($"Results for image {result1.ImageId}: {resultsForImage.Count()}");

// Get successful results
var successfulResults = await repository.GetSuccessfulResultsAsync();
Console.WriteLine($"Successful results: {successfulResults.Count()}");

// Get failed results
var failedResults = await repository.GetFailedResultsAsync();
Console.WriteLine($"Failed results: {failedResults.Count()}");

// Get results by status
var processedResults = await repository.GetByStatusAsync(ProcessingStatus.Processed);
Console.WriteLine($"Processed results: {processedResults.Count()}");

// Get results completed between specific dates
var recentResults = await repository.GetCompletedBetweenAsync(
DateTime.UtcNow.AddHours(-1),
DateTime.UtcNow
);
Console.WriteLine($"Results completed in last hour: {recentResults.Count()}");

// Get slowest results (top 10)
var slowestResults = await repository.GetSlowestResultsAsync(10);
Console.WriteLine($"Slowest results count: {slowestResults.Count()}");

// Get average processing time for successful operations
var avgProcessingTime = await repository.GetAverageProcessingTimeAsync();
Console.WriteLine($"Average processing time: {avgProcessingTime:F2}ms");

// Update a result
retrievedResult.Status = ProcessingStatus.Processed;
var updatedResult = await repository.UpdateAsync(retrievedResult);
Console.WriteLine($"Updated result status: {updatedResult.Status}");

// Check if result exists
var exists = await repository.ExistsAsync(result1.Id);
Console.WriteLine($"Result exists: {exists}");

// Count results
var resultCount = await repository.CountAsync();
Console.WriteLine($"Total result count: {resultCount}");

// Get paged results
var page1 = await repository.GetPagedAsync(1, 2);
Console.WriteLine($"Page 1 has {page1.Count()} results");

// Delete a result
var deleteSuccess = await repository.DeleteAsync(result1.Id);
Console.WriteLine($"Result deleted: {deleteSuccess}");

// Verify deletion
var resultAfterDelete = await repository.GetByIdAsync(result1.Id);
Console.WriteLine($"Result still exists after delete: {resultAfterDelete != null}");
}
}
```

## ProcessingResult

The `ProcessingResult` class encapsulates the outcome of an image processing operation, providing detailed status tracking, performance metrics, and information about applied filters. It supports automatic state management through completion and failure methods, allowing for consistent error handling and diagnostic reporting across processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;

// Initialize a new result object for an image
var result = new ProcessingResult
{
    ImageId = Guid.NewGuid(),
    OutputPath = "/output/processed_image.png"
};

// Apply a filter and record its execution metrics
// Assuming FilterType enum is available in GpuImageProcessing.Domain
result.AddFilterApplied("GaussianBlur", FilterType.Blur, 15.5);

// Complete the processing operation
result.Complete(result.OutputPath);

// Verify the result
if (result.IsSuccessful)
{
    Console.WriteLine($"Image processed successfully in {result.ProcessingTimeMilliseconds}ms.");
    Console.WriteLine($"Total filter execution time: {result.GetTotalFilterExecutionTime()}ms");
}
else
{



## FilterConfiguration

`FilterConfiguration` defines the settings and parameters for a specific image processing filter, including its name, priority, and any custom kernel code or parameter settings. It provides robust validation and cloning capabilities to ensure filter configurations are correctly set up and can be safely reused or modified within processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Initialize a new filter configuration
var config = new FilterConfiguration
{
    Id = Guid.NewGuid(),
    Name = "CustomBlur",
    FilterType = (FilterType)0, // Replace with appropriate enum value
    Description = "Custom Blur Filter",
    IsActive = true,
    Priority = 1
};

// Set parameters and their types
config.SetParameter("BlurRadius", 5);
config.ParameterTypes["BlurRadius"] = "System.Int32";

// Validate the configuration before use
if (config.Validate())
{
    Console.WriteLine($"Filter '{config.Name}' is valid and ready for processing.");
}

// Clone the configuration for modification
var clonedConfig = config.Clone();
clonedConfig.Name = "CustomBlur_Copy";
```

## BatchProcessingService

The `BatchProcessingService` class manages batch image processing operations, enabling efficient processing of multiple images with configurable filters. It handles batch creation, status tracking, progress monitoring, and concurrent execution management. The service maintains a registry of active batches and provides methods for batch lifecycle management, including cancellation and progress reporting.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Setup logging (typically via dependency injection in real applications)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<BatchProcessingService> logger = loggerFactory.CreateLogger<BatchProcessingService>();

        // Initialize required services (in real usage these would be injected)
        var processingService = new ImageProcessingService(...);
        var imageRepository = new ImageRepository();
        var batchService = new BatchProcessingService(processingService, imageRepository, logger);

        // Create a batch with multiple images and filters
        var imageIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var filterIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        var batch = await batchService.CreateBatchAsync(
            imageIds,
            filterIds,
            "SummerPhotos-2024",
            @"./output/summer-2024-enhanced"
        );

        Console.WriteLine($"Created batch '{batch.Name}' with {batch.TotalImages} images");

        // Process the batch asynchronously
        var processingTask = batchService.ProcessBatchAsync(batch);

        // Monitor progress
        while (!processingTask.IsCompleted)
        {
            var progress = batchService.GetBatchProgress(batch.Id);
            Console.WriteLine($"Progress: {progress["ProgressPercent"]:P0} - " +
                           $"Processed: {progress["ProcessedImages"]}/{progress["TotalImages"]}");
            
            await Task.Delay(1000);
        }

        // Get final result
        var completedBatch = await processingTask;
        Console.WriteLine($"Batch completed: {completedBatch.ProcessedImages} processed, " +
                       $"{completedBatch.FailedImages} failed");

        // Check active batches
        int activeCount = batchService.GetActiveBatchCount();
        Console.WriteLine($"Active batches: {activeCount}");

        // Get all active batches
        var activeBatches = batchService.GetActiveBatches();
        foreach (var activeBatch in activeBatches)
        {
            Console.WriteLine($" - Batch {activeBatch.Id}: {activeBatch.Name}");
        }

        // Cancel a batch if needed
        bool cancelled = batchService.CancelBatch(batch.Id);
        Console.WriteLine($"Batch cancellation {(cancelled ? "succeeded" : "failed")}");

        // Get batch status
        var batchStatus = batchService.GetBatchStatus(batch.Id);
        if (batchStatus != null)
        {
            Console.WriteLine($"Batch status: {batchStatus.Status}");
        }
    }
}
```

## FilterService

The `FilterService` class provides centralized management and application of image filters within the GPU image processing pipeline. It offers comprehensive CRUD operations for filter configurations, including creating, retrieving, updating, and deleting filters, as well as specialized methods for applying filters to images and managing active filter presets. The service integrates with the `FilterConfigurationRepository` to persist filter configurations and uses specialized handlers for different filter types to ensure proper processing.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Setup logging (typically via dependency injection in real applications)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<FilterService> logger = loggerFactory.CreateLogger<FilterService>();

        // Initialize the filter service with repository and logger
        var repository = new FilterConfigurationRepository();
        var filterService = new FilterService(repository, logger);

        // Create a new grayscale filter configuration
        var grayscaleFilter = new FilterConfiguration
        {
            Name = "PhotoGrayscale",
            FilterType = FilterType.Grayscale,
            Description = "Convert photos to grayscale for artistic effect",
            IsActive = true,
            Priority = 1
        };
        grayscaleFilter.SetParameter("Intensity", 1.0f);
        grayscaleFilter.ParameterTypes["Intensity"] = "System.Single";

        // Create the filter in the repository
        var createdFilter = await filterService.CreateFilterAsync(grayscaleFilter);
        Console.WriteLine($"Created filter: {createdFilter.Name} with ID: {createdFilter.Id}");

        // Create a blur filter configuration
        var blurFilter = new FilterConfiguration
        {
            Name = "SoftBlur",
            FilterType = FilterType.Blur,
            Description = "Soft blur for portrait smoothing",
            IsActive = true,
            Priority = 2
        };
        blurFilter.SetParameter("Radius", 5);
        blurFilter.ParameterTypes["Radius"] = "System.Int32";

        await filterService.CreateFilterAsync(blurFilter);

        // Get all active filters sorted by priority
        var activeFilters = await filterService.GetActiveFiltersAsync();
        Console.WriteLine($"Active filters count: {activeFilters.Count()}");

        // Get filters by type
        var blurFilters = await filterService.GetFiltersByTypeAsync(FilterType.Blur);
        Console.WriteLine($"Blur filters count: {blurFilters.Count()}");

        // Update a filter configuration
        createdFilter.Description = "Updated: Convert photos to grayscale for artistic effect";
        var updatedFilter = await filterService.UpdateFilterAsync(createdFilter);
        Console.WriteLine($"Updated filter: {updatedFilter.Description}");

        // Apply a filter to an image (simulated - would process actual image data in real usage)
        var image = new Image(1920, 1080, 3)
        {
            Id = Guid.NewGuid(),
            FileName = "sample.jpg",
            FilePath = "/images/sample.jpg",
            Format = ImageFormat.Jpeg,
            ColorSpace = ColorSpace.Rgb,
            BitsPerPixel = 24
        };

        await filterService.ApplyFilterAsync(image, createdFilter.Id);
        Console.WriteLine($"Applied filter {createdFilter.Name} to image {image.Id}");

        // Get a specific filter by ID
        var retrievedFilter = await filterService.GetFilterAsync(createdFilter.Id);
        if (retrievedFilter != null)
        {
            Console.WriteLine($"Retrieved filter: {retrievedFilter.Name}");
        }

        // Delete a filter
        var deleteSuccess = await filterService.DeleteFilterAsync(createdFilter.Id);
        Console.WriteLine($"Filter deleted: {deleteSuccess}");
    }
}
```

## FilterChainBuilder

The `FilterChainBuilder` class provides a fluent interface for constructing and configuring `FilterChain` instances. It allows you to build filter chains programmatically with a clean, readable API, supporting all filter types and configuration options available in the domain. The builder pattern simplifies chain creation and ensures proper validation before the chain is used in processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Create a filter chain builder
var builder = FilterChainBuilder.Create()
    .WithDescription("Photo enhancement chain with noise reduction and sharpening")
    .WithExecutionOrder(1)
    .AllowParallelExecution()
    .CacheIntermediates();

// Add various filter types to the chain
builder.AddGrayscale()
    .WithParameter("Intensity", 0.8f);

builder.AddBlur()
    .WithParameter("Radius", 3);

builder.AddSharpen()
    .WithParameter("Amount", 1.2f);

builder.AddEdgeDetection()
    .WithParameter("Threshold", 0.3f);

builder.AddColorCorrection()
    .WithParameter("Brightness", 1.1f)
    .WithParameter("Contrast", 1.05f);

builder.AddThreshold()
    .WithParameter("ThresholdValue", 0.7f);

builder.AddRotation()
    .WithParameter("Degrees", 90);

builder.AddScaling()
    .WithParameter("ScaleFactor", 1.5f);

// Build the final filter chain
var chain = builder.Build();

// Validate the chain before use
if (chain.Validate())
{
    Console.WriteLine($"Filter chain '{chain.Name}' created successfully with {chain.GetEnabledSteps().Count} enabled steps.");
    Console.WriteLine($"Execution order: {chain.ExecutionOrder}");
    Console.WriteLine($"Parallel execution: {chain.AllowParallelExecution}");
    Console.WriteLine($"Cache intermediates: {chain.CacheIntermediateResults}");
}
```

## PerformanceMetrics

The `PerformanceMetrics` class tracks performance metrics for GPU operations, including CPU and GPU utilization, memory usage, execution times, and throughput. It provides methods for recording operations, calculating success rates, checking memory warnings, and resetting metrics for new measurement periods. This class is essential for monitoring and optimizing GPU-accelerated image processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Create performance metrics instance
var metrics = new PerformanceMetrics
{
    CpuUsagePercent = 45.2,
    MemoryUsedBytes = 8589934592, // 8 GB
    GpuMemoryUsedBytes = 6442450944, // 6 GB
    GpuUtilizationPercent = 87.5,
    ImagePixelsProcessedPerSecond = 1920000000, // 1.92 billion pixels/s
    ThroughputMegabytesPerSecond = 1250.75
};

// Record individual operation execution times
metrics.RecordExecution(12.5); // 12.5ms
metrics.RecordExecution(14.2); // 14.2ms
metrics.RecordExecution(9.8);  // 9.8ms

// Calculate derived metrics
Console.WriteLine($"Average execution time: {metrics.AverageExecutionTimeMs:F2}ms");
Console.WriteLine($"Max execution time: {metrics.MaxExecutionTimeMs:F2}ms");
Console.WriteLine($"Min execution time: {metrics.MinExecutionTimeMs:F2}ms");
Console.WriteLine($"Total operations: {metrics.TotalOperationsCount}");
Console.WriteLine($"Success rate: {metrics.GetSuccessRate():F2}%");
Console.WriteLine($"Memory usage percent: {metrics.GetMemoryUsagePercent():F2}%");

// Check if memory warning is required
if (metrics.IsMemoryWarningRequired())
{
    Console.WriteLine("Warning: GPU memory usage is above threshold!");
}

// Update metrics with additional operations
metrics.FailedOperationsCount = 2;
Console.WriteLine($"Updated success rate: {metrics.GetSuccessRate():F2}%");

// Reset metrics for a new measurement period
metrics.Reset();
metrics.RecordExecution(15.3);

// Display the complete metrics summary
Console.WriteLine($"Performance summary: {metrics}");
```

## FilterChain

The `FilterChain` class represents a sequence of image processing filters that are applied in order to transform an image. It manages filter steps, execution order, parallel processing options, and caching behavior, making it the central component for defining image processing workflows. Filter chains can be validated, cloned, and configured with various options to optimize performance and resource usage.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Create a new filter chain
var chain = new FilterChain
{
    Name = "Photo Enhancement Chain",
    Description = "A chain for enhancing digital photographs",
    ExecutionOrder = 1,
    AllowParallelExecution = true,
    MaxParallelSteps = 4,
    CacheIntermediateResults = true,
    ChainOptions = new Dictionary<string, object>
    {
        { "QualityThreshold", 0.95 },
        { "MaxMemoryUsage", 2L * 1024 * 1024 * 1024 }
    }
};

// Add filter steps to the chain
chain.AddStep(Guid.NewGuid()); // Add first filter
chain.AddStep(Guid.NewGuid()); // Add second filter
chain.AddStep(Guid.NewGuid()); // Add third filter

// Reorder steps if needed
chain.ReorderSteps(new List<Guid> {
    chain.Steps[2].FilterId,
    chain.Steps[0].FilterId,
    chain.Steps[1].FilterId
});

// Disable a specific step
var stepToDisable = chain.Steps.First(s => s.Order == 1);
stepToDisable.IsEnabled = false;

// Get enabled steps for processing
var enabledSteps = chain.GetEnabledSteps();
Console.WriteLine($"Chain '{chain.Name}' has {enabledSteps.Count} enabled steps.");

// Validate the chain before processing
if (chain.Validate())
{
    Console.WriteLine($"Filter chain '{chain.Name}' is valid and ready for execution.");
    double estimatedTime = chain.EstimateTotalProcessingTime();
    Console.WriteLine($"Estimated processing time: {estimatedTime}ms");
}

// Clone the chain for reuse with different parameters
var clonedChain = chain.Clone();
clonedChain.Name = "Photo Enhancement Chain - High Quality";
```


## GpuManagementService

The `GpuManagementService` class provides centralized management of GPU devices and resources within the GPU image processing pipeline. It handles device discovery, memory allocation, validation, and performance monitoring, ensuring efficient utilization of available GPU resources. The service automatically falls back to CPU-based processing when no GPU devices are available, maintaining system operability across different hardware configurations.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging;
using System;

// Initialize the service with logging (typically via dependency injection)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<GpuManagementService> logger = loggerFactory.CreateLogger<GpuManagementService>();

var gpuService = new GpuManagementService(logger);

// Check if fallback mode is active (no GPU devices available)
if (gpuService.UseFallback)
{
    Console.WriteLine("Running in CPU fallback mode - no GPU devices detected.");
}

// Get all available GPU devices
var availableDevices = gpuService.GetAvailableDevices();
Console.WriteLine($"Found {availableDevices.Count()} available GPU device(s):");
foreach (var device in availableDevices)
{
    Console.WriteLine($" - {device.Name} ({device.Vendor}) - {device.GlobalMemoryBytes / (1024 * 1024 * 1024)} GB");
}

// Get the best performing device for processing
var bestDevice = gpuService.GetBestDevice();
if (bestDevice != null)
{
    Console.WriteLine($"Selected best device: {bestDevice.Name}");
    
    // Allocate memory for image processing (e.g., 100MB for a batch of images)
    long requiredMemory = 100 * 1024 * 1024; // 100 MB
    bool allocationSuccess = gpuService.AllocateMemory(requiredMemory, bestDevice.Id);
    
    if (allocationSuccess)
    {
        Console.WriteLine($"Successfully allocated {requiredMemory / (1024 * 1024)} MB on {bestDevice.Name}");
        
        // Process images...
        
        // Deallocate memory when done
        gpuService.DeallocateMemory(requiredMemory, bestDevice.Id);
        Console.WriteLine("Memory deallocated.");
    }
}

// Get device with most memory available
var memoryRichDevice = gpuService.GetDeviceWithMostMemory();
if (memoryRichDevice != null)
{
    Console.WriteLine($"Device with most memory: {memoryRichDevice.Name} - {memoryRichDevice.GetAvailableMemory() / (1024 * 1024 * 1024)} GB available");
}

// Validate device capabilities before intensive operations
var deviceId = bestDevice?.Id ?? Guid.Empty;
bool isValid = gpuService.ValidateDevice(deviceId, requiredMemory: 50 * 1024 * 1024); // 50 MB minimum
Console.WriteLine($"Device validation for intensive processing: {(isValid ? "PASSED" : "FAILED")}");

// Get comprehensive memory statistics
var memoryStats = gpuService.GetMemoryStatistics();
Console.WriteLine("Memory Statistics:");
foreach (var stat in memoryStats)
{
    Console.WriteLine($"  {stat.Key}: {stat.Value}");
}

// Get total allocated memory across all devices
long totalAllocated = gpuService.GetTotalAllocatedMemory();
Console.WriteLine($"Total GPU memory allocated: {totalAllocated / (1024 * 1024)} MB");
```

## PerformanceMonitoringService

The `PerformanceMonitoringService` class provides centralized monitoring and tracking of performance metrics for GPU-accelerated image processing operations. It records execution times, system resource usage (CPU, memory, GPU), throughput metrics, and maintains a history of performance snapshots. This service is essential for performance analysis, optimization, and real-time monitoring of image processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Setup logging (typically via dependency injection in real applications)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<PerformanceMonitoringService> logger = loggerFactory.CreateLogger<PerformanceMonitoringService>();

        // Initialize the performance monitoring service
        var performanceMonitor = new PerformanceMonitoringService(logger);

        // Record individual operation execution times
        performanceMonitor.RecordOperation(12.5); // 12.5ms - successful operation
        performanceMonitor.RecordOperation(8.3);  // 8.3ms - successful operation
        performanceMonitor.RecordOperation(15.7, success: true); // Explicit success
        performanceMonitor.RecordOperation(2.1, success: false); // Failed operation

        // Update system metrics (CPU, memory, GPU utilization)
        performanceMonitor.UpdateSystemMetrics(
            cpuPercent: 45.2,
            memoryBytes: 8589934592, // 8 GB
            gpuMemoryBytes: 6442450944, // 6 GB
            gpuUtilization: 87.5
        );

        // Update throughput metrics (pixels per second and MB/s)
        performanceMonitor.UpdateThroughput(
            pixelsPerSecond: 1920000000, // 1.92 billion pixels/s
            megabytesPerSecond: 1250.75
        );

        // Get current performance metrics
        var currentMetrics = performanceMonitor.GetCurrentMetrics();
        Console.WriteLine($"Current CPU Usage: {currentMetrics.CpuUsagePercent:F2}%");
        Console.WriteLine($"Current GPU Utilization: {currentMetrics.GpuUtilizationPercent:F2}%");
        Console.WriteLine($"Average Execution Time: {currentMetrics.AverageExecutionTimeMs:F2}ms");
        Console.WriteLine($"Throughput: {currentMetrics.ThroughputMegabytesPerSecond:F2} MB/s");
        Console.WriteLine($"Success Rate: {currentMetrics.GetSuccessRate():F2}%");

        // Snapshot current metrics and start new measurement period
        var snapshot = performanceMonitor.SnapshotAndReset();
        Console.WriteLine($"\nSnapshot recorded at: {snapshot.RecordedAt:O}");
        Console.WriteLine($"Total operations: {snapshot.TotalOperationsCount}");
        Console.WriteLine($"Failed operations: {snapshot.FailedOperationsCount}");

        // Get historical metrics (last 10 snapshots)
        var recentHistory = performanceMonitor.GetMetricsHistory(limit: 10);
        Console.WriteLine($"\nRetrieved {recentHistory.Count()} historical metrics");

        // Get average metrics over the last 60 minutes
        var averageMetrics = performanceMonitor.GetAverageMetrics(lastMinutes: 60);
        Console.WriteLine("\nAverage Metrics (last 60 minutes):");
        foreach (var metric in averageMetrics)
        {
            Console.WriteLine($" - {metric.Key}: {metric.Value:F2}");
        }

        // Generate comprehensive performance report
        string report = performanceMonitor.GetPerformanceReport();
        Console.WriteLine("\nPerformance Report:");
        Console.WriteLine(report);
    }
}
```
## AppSettings

The `AppSettings` class provides centralized configuration management for the GPU image processing application. It exposes application metadata, performance tuning parameters, directory paths, and feature flags that control GPU acceleration, batch processing behavior, caching strategies, and monitoring capabilities throughout the image processing pipeline.

### Usage Example

```csharp
using GpuImageProcessing.Configuration;
using System;
using System.Collections.Generic;

// Create application settings instance
var settings = new AppSettings
{
    ApplicationName = "GPU Image Processing Pipeline",
    ApplicationVersion = "1.0.0",
    EnableGpuAcceleration = true,
    MaxConcurrentOperations = 8,
    OperationTimeoutMs = 30000,
    OutputDirectory = @"./output",
    CacheDirectory = @"./cache",
    EnableMetricsCollection = true,
    MetricsCollectionIntervalMs = 5000,
    EnablePerformanceLogging = true,
    MaxBatchSize = 16,
    MaxMemoryPerImage = 256L * 1024 * 1024, // 256 MB
    MaxTotalGpuMemory = 4L * 1024 * 1024 * 1024, // 4 GB
    EnableCaching = true,
    CacheExpirMinutes = 60,
    SupportedImageFormats = new List<string> { "JPEG", "PNG", "WEBP", "BMP", "TIFF" },
    Validate = true
};

// Display application configuration
Console.WriteLine($"Application: {settings.ApplicationName} v{settings.ApplicationVersion}");
Console.WriteLine($"GPU Acceleration: {(settings.EnableGpuAcceleration ? "Enabled" : "Disabled")}");
Console.WriteLine($"Max Concurrent Operations: {settings.MaxConcurrentOperations}");
Console.WriteLine($"Output Directory: {settings.OutputDirectory}");
Console.WriteLine($"Cache Directory: {settings.CacheDirectory}");
Console.WriteLine($"Metrics Collection: {(settings.EnableMetricsCollection ? "Enabled" : "Disabled")}");
Console.WriteLine($"Performance Logging: {(settings.EnablePerformanceLogging ? "Enabled" : "Disabled")}");
Console.WriteLine($"Max Batch Size: {settings.MaxBatchSize}");
Console.WriteLine($"Max Memory Per Image: {settings.MaxMemoryPerImage / (1024 * 1024)} MB");
Console.WriteLine($"Max Total GPU Memory: {settings.MaxTotalGpuMemory / (1024 * 1024 * 1024)} GB");
Console.WriteLine($"Caching: {(settings.EnableCaching ? "Enabled" : "Disabled")}");
Console.WriteLine($"Cache Expiration: {settings.CacheExpirMinutes} minutes");
Console.WriteLine($"Supported Formats: {string.Join(", ", settings.SupportedImageFormats)}");
Console.WriteLine($"Validation: {(settings.Validate ? "Enabled" : "Disabled")}");
Console.WriteLine($"Full configuration: {settings}");
```

## CpuImageProcessor

The `CpuImageProcessor` class provides CPU-based image processing functionality as a fallback when no OpenCL-capable GPU devices are available. It implements essential image operations including resizing, grayscale conversion, blurring, and various filter applications using raw pixel manipulation to ensure the library remains functional across different hardware configurations.

This processor is automatically used when GPU acceleration is unavailable, providing identical functionality to the GPU-accelerated processors but with CPU-based implementations of the filter algorithms.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using GpuImageProcessing.Fallback;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Setup logging (typically via dependency injection in real applications)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<CpuImageProcessor> logger = loggerFactory.CreateLogger<CpuImageProcessor>();

        // Create CPU image processor instance
        var cpuProcessor = new CpuImageProcessor(logger);

        // Create a sample image (1920x1080 RGB)
        var image = new Image(1920, 1080, 3)
        {
            Id = Guid.NewGuid(),
            FileName = "sample.jpg",
            FilePath = @"/images/sample.jpg",
            Format = ImageFormat.Jpeg,
            ColorSpace = ColorSpace.Rgb,
            BitsPerPixel = 24
        };

        // Initialize pixel data (simulated - in real usage this would contain actual image data)
        image.PixelData = new byte[image.Width * image.Height * 3];
        new Random().NextBytes(image.PixelData);

        // Check if the processor can handle the requested filter
        bool canProcessGrayscale = cpuProcessor.CanProcess(FilterType.Grayscale);
        Console.WriteLine($"Can process grayscale: {canProcessGrayscale}");

        // Apply grayscale filter
        var grayscaleImage = cpuProcessor.ToGrayscale(image.Clone());
        Console.WriteLine($"Image converted to grayscale: {grayscaleImage.Width}x{grayscaleImage.Height}");

        // Apply blur filter with radius 3
        var blurredImage = cpuProcessor.Blur(image.Clone(), radius: 3);
        Console.WriteLine($"Image blurred with radius 3: {blurredImage.Width}x{blurredImage.Height}");

        // Resize image to 1280x720
        var resizedImage = cpuProcessor.Resize(image.Clone(), 1280, 720);
        Console.WriteLine($"Image resized to: {resizedImage.Width}x{resizedImage.Height}");

        // Apply a filter configuration asynchronously
        var filterConfig = new FilterConfiguration
        {
            Name = "Edge Detection",
            FilterType = FilterType.EdgeDetection,
            IsActive = true,
            Priority = 1
        };

        await cpuProcessor.ApplyFilterAsync(image.Clone(), filterConfig);
        Console.WriteLine("Edge detection filter applied successfully");
    }
}
```

## BenchmarkSuiteConfiguration

The `BenchmarkSuiteConfiguration` class configures which benchmark categories are active and how they are executed during performance testing. It allows fine-grained control over which benchmark suites to include (filter chains, batch processing, chain builders, utilities, etc.), accuracy levels, output directories, and hardware counter collection for comprehensive performance analysis.

### Usage Example

```csharp
using GpuImageProcessing.Benchmarking;
using System;

// Create a custom benchmark configuration for development
var devConfig = new BenchmarkSuiteConfiguration
{
    RunName = "Development-Benchmark-Run-2024",
    IncludeFilterChainBenchmarks = true,
    IncludeBatchProcessingBenchmarks = true,
    IncludeFilterChainBuilderBenchmarks = true,
    IncludeImageUtilitiesBenchmarks = false,
    IncludeEnumerableExtensionsBenchmarks = false,
    AccuracyLevel = BenchmarkAccuracyLevel.Quick,
    OutputDirectory = @"./benchmarks/dev",
    EnableHardwareCounters = false
};

// Validate the configuration before running benchmarks
var validationErrors = devConfig.Validate();
if (validationErrors.Count == 0)
{
    Console.WriteLine("Configuration is valid!");
    Console.WriteLine($"Enabled categories: {string.Join(", ", devConfig.GetEnabledCategories())}");
}
else
{
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Use convenience factory methods for common scenarios
var ciConfig = BenchmarkSuiteConfiguration.ForCi("CI-Regression-Tests");
Console.WriteLine($"CI config accuracy: {ciConfig.AccuracyLevel}");
Console.WriteLine($"CI config categories: {string.Join(", ", ciConfig.GetEnabledCategories())}");

var releaseConfig = BenchmarkSuiteConfiguration.ForRelease("Formal-Performance-Report");
Console.WriteLine($"Release config accuracy: {releaseConfig.AccuracyLevel}");
Console.WriteLine($"Release config hardware counters: {releaseConfig.EnableHardwareCounters}");
```

## Image

The `Image` class is a core domain model that encapsulates raw image pixel data along with its metadata, such as format, color space, dimensions, and processing status. It provides essential validation and size calculation methods, making it the primary structure for representing images throughout the processing pipeline.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Create a new image instance with specified dimensions
var image = new Image(1920, 1080, 3) 
{
    Id = Guid.NewGuid(),
    FileName = "input_image.png",
    FilePath = "/data/input_image.png",
    Format = ImageFormat.Png, // Assuming ImageFormat enum exists
    ColorSpace = ColorSpace.Rgb, // Assuming ColorSpace enum exists
    BitsPerPixel = 24,
    CreatedAt = DateTime.UtcNow
};

// Add custom metadata
image.Metadata["CameraModel"] = "Generic-Camera-X";

// Validate the image configuration
if (image.Validate())
{
    long requiredBytes = image.CalculatePixelDataSize();
    Console.WriteLine($"Image '{image.FileName}' validated.");
    Console.WriteLine($"Required buffer size: {requiredBytes} bytes.");
    Console.WriteLine($"Status: {image.Status}"); // Assuming ProcessingStatus enum exists
}
```


```