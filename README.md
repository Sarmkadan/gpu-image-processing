
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

    Console.WriteLine($"Succeeded: {result.SucceededCount}");
    Console.WriteLine($"Failed: {result.FailedCount}");
    Console.WriteLine($"Total Duration: {result.TotalDuration}");

    foreach (var outcome in result.Outcomes)
    {
    Console.WriteLine($"Image {outcome.ImageId}: {outcome.Stage} (Attempts: {outcome.Attempts})");
    }
    ```