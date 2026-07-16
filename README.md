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


## ComputeShaderPass

`ComputeShaderPass` represents a single dispatchable unit within a `ComputeShaderPipeline`. It encapsulates the OpenCL kernel source (or reference), input/output images, workgroup configuration, and execution parameters necessary for the pipeline to dispatch the compute workload on a GPU.

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