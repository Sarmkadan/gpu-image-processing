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

## OpenCLException

The `OpenCLException` is thrown when an OpenCL operation fails during GPU-accelerated image processing. It provides detailed diagnostic information including the OpenCL error code, the name of the device where the error occurred, and comprehensive error messages from the OpenCL runtime. This exception is particularly useful for debugging OpenCL-specific issues such as kernel compilation failures, memory allocation errors, or invalid device contexts.

### Usage Example

```csharp
using GpuImageProcessing.Core.Exceptions;
using System;

try
{
// Simulate an OpenCL operation failure
throw new OpenCLException("Failed to compile OpenCL kernel: invalid work group size", -48);
}
catch (OpenCLException ex)
{
Console.WriteLine($"Exception Message: {ex.Message}");
Console.WriteLine($"Device Name: {ex.DeviceName ?? "Unknown"}");
Console.WriteLine($"OpenCL Error Code: {ex.OpenCLErrorCode?.ToString() ?? "N/A"}");
Console.WriteLine($"Occurred At: {ex.OccurredAt:O}");

// Using the overridden ToString() for detailed logging
Console.WriteLine($"Full Exception Details:\n{ex}");
}

// Example with device name
var deviceException = new OpenCLException(
    "Memory object allocation failed on device",
    "AMD Radeon RX 6800 XT",
    -4);
Console.WriteLine(deviceException);
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

## BatchProcessingServiceTests

The `BatchProcessingServiceTests` class provides comprehensive unit tests for the `BatchProcessingService` class, validating batch image processing functionality including batch creation, processing, status tracking, cancellation, and progress reporting. These tests ensure proper error handling, edge cases, and correct behavior across different scenarios including null inputs, invalid batches, partial failures, and cancellation requests.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Initialize required services (typically via dependency injection in real applications)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<BatchProcessingService> logger = loggerFactory.CreateLogger<BatchProcessingService>();

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

## InteractiveShell

The `InteractiveShell` class provides a REPL-style interactive command shell for the GPU image processing CLI application. It supports command registration, execution, history tracking, and auto-completion suggestions, enabling users to interactively run commands and receive contextual feedback.

### Usage Example

```csharp
using GpuImageProcessing.Cli;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create CLI parser and interactive shell
        var parser = new CliParser();
        var shell = new InteractiveShell(parser);
        
        // Register command handlers
        shell.RegisterHandler("process", async cmd => 
        {
            Console.WriteLine($"Processing command with {cmd.Arguments.Count} arguments");
            await Task.CompletedTask;
        });
        
        shell.RegisterHandler("filter", async cmd => 
        {
            Console.WriteLine($"Applying filter: {cmd.Arguments[0]}");
            await Task.CompletedTask;
        });
        
        // Register commands for auto-completion
        var completionProvider = new CompletionProvider(parser);
        completionProvider.RegisterCommand("process");
        completionProvider.RegisterCommand("filter");
        completionProvider.RegisterCommand("exit");
        completionProvider.RegisterCommand("help");
        
        // Start the interactive shell
        Console.WriteLine("Starting GPU Image Processing Interactive Shell...");
        Console.WriteLine("Type 'help' for available commands or 'exit' to quit");
        await shell.RunAsync();
    }
}
```

## ProcessingProfile

The `ProcessingProfile` class represents a configuration profile for optimized image processing operations. It encapsulates processing parameters such as GPU acceleration settings, parallel operation limits, memory constraints, precision formats, and tiling options. Processing profiles enable consistent configuration across batch operations and can be customized for different workload requirements including speed-optimized, quality-optimized, or balanced processing scenarios.

### Usage Example

```csharp
using GpuImageProcessing.Core.Models;
using System;
using System.Collections.Generic;

// Create a custom processing profile for high-performance GPU processing
var performanceProfile = new ProcessingProfile
{
    Name = "High Performance",
    Description = "Optimized for maximum throughput with GPU acceleration",
    UseGPUAcceleration = true,
    MaxParallelOperations = 8,
    BatchSize = 25,
    MaxMemoryUsageBytes = 4_000_000_000, // 4GB
    EnablePrecisionReduction = true,
    PrecisionFormat = "float16",
    EnableTiling = true,
    TileSizePixels = 1024,
    OptimizationSettings = new Dictionary<string, float>
    {
        { "QualityBoost", 1.2f },
        { "MemoryEfficiency", 0.9f }
    }
};

// Use predefined profiles for common scenarios
var speedProfile = ProcessingProfile.CreateSpeedOptimized();
var qualityProfile = ProcessingProfile.CreateQualityOptimized();
var balancedProfile = ProcessingProfile.CreateBalanced();

// Access profile properties for configuration
Console.WriteLine($"Profile: {balancedProfile.Name}");
Console.WriteLine($"Description: {balancedProfile.Description}");
Console.WriteLine($"GPU Acceleration: {balancedProfile.UseGPUAcceleration}");
Console.WriteLine($"Max Parallel Operations: {balancedProfile.MaxParallelOperations}");
Console.WriteLine($"Batch Size: {balancedProfile.BatchSize}");
Console.WriteLine($"Precision: {(balancedProfile.EnablePrecisionReduction ? balancedProfile.PrecisionFormat : "full")}");
Console.WriteLine($"Max Memory: {balancedProfile.MaxMemoryUsageBytes / (1024.0 * 1024.0):F2} MB");

// Validate the profile configuration
if (balancedProfile.IsValid())
{
    Console.WriteLine("Profile configuration is valid");
}

// Clone a profile for customization
var customProfile = balancedProfile.Clone();
customProfile.Name = "Custom Balanced";
customProfile.MaxParallelOperations = 6;

// Get effective batch size based on memory constraints
int effectiveBatchSize = balancedProfile.GetEffectiveBatchSize(150_000_000); // ~150MB per image
Console.WriteLine($"Effective batch size: {effectiveBatchSize}");

// Get profile summary for logging
string profileSummary = balancedProfile.GetProfileSummary();
Console.WriteLine(profileSummary);
```

## CommandDispatcher

The `CommandDispatcher` class routes CLI commands to appropriate handlers, managing command registration, discovery, and execution. It supports dynamic command registration at runtime, provides introspection capabilities to list available commands, and handles command instantiation with dependency injection through an `IServiceProvider`. The dispatcher validates command types and provides error handling for command execution failures.

### Usage Example

```csharp
using GpuImageProcessing.Cli;
using System;
using System.Threading.Tasks;

class Program
{
static async Task Main()
{
// Setup dependency injection container (simplified for example)
var serviceProvider = new ServiceCollection()
    .AddSingleton<IServiceProvider>(sp => sp)
    .BuildServiceProvider();

// Create command dispatcher
var dispatcher = new CommandDispatcher(serviceProvider);

// Register custom command
// dispatcher.RegisterCommand("custom", typeof(CustomCommandHandler));

// List available commands
Console.WriteLine("Available commands:");
foreach (var command in dispatcher.GetAvailableCommands())
{
    Console.WriteLine($" - {command}");
}

// Get command description
var description = dispatcher.GetCommandDescription("help");
if (description != null)
{
    Console.WriteLine($"\nHelp command description: {description}");
}

// Dispatch a command (simulating command line: "help --detailed")
string[] args = new[] { "help", "--detailed" };
int exitCode = await dispatcher.DispatchAsync(args);

Console.WriteLine($"\nCommand executed with exit code: {exitCode}");
}
}
```

## CliParser

The `CliParser` class provides robust command-line argument parsing with support for subcommands, options, flags, and positional arguments. It enables registering commands and global options, parsing command-line input, generating comprehensive help text, and validating required arguments. The parser supports both long-form (`--option`) and short-form (`-o`) options, required option validation, and automatic help text generation for both individual commands and the entire CLI application.

### Usage Example

```csharp
using GpuImageProcessing.Cli;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create CLI parser instance
        var parser = new CliParser();

        // Register a global option available to all commands
        parser.RegisterGlobalOption(
            name: "verbose",
            shortForm: "v",
            description: "Enable verbose output",
            requiresValue: false
        );

        // Register a global option that requires a value
        parser.RegisterGlobalOption(
            name: "output",
            shortForm: "o",
            description: "Output directory path",
            requiresValue: true
        );

        // Register a command with its options
        parser.RegisterCommand(
            name: "process",
            description: "Process an image with GPU acceleration",
            builder: cmd => cmd
                .AddOption(
                    longForm: "input",
                    shortForm: "i",
                    description: "Input image file path",
                    requiresValue: true,
                    isRequired: true
                )
                .AddOption(
                    longForm: "filter",
                    shortForm: "f",
                    description: "Filter to apply (blur, grayscale, sharpen)",
                    requiresValue: true
                )
                .AddOption(
                    longForm: "quality",
                    shortForm: "q",
                    description: "Quality level (0-100)",
                    requiresValue: true
                )
        );

        // Parse command-line arguments (simulating: process --input image.jpg --filter blur -q 90)
        string[] args = new[] { "process", "--input", "image.jpg", "--filter", "blur", "-q", "90" };
        var parsedCommand = parser.Parse(args);

        Console.WriteLine($"Command: {parsedCommand.CommandName}");
        Console.WriteLine($"Input file: {parsedCommand.GetOption("input")}");
        Console.WriteLine($"Filter: {parsedCommand.GetOption("filter")}");
        Console.WriteLine($"Quality: {parsedCommand.GetOption("quality")}");
        Console.WriteLine($"Verbose: {parsedCommand.HasOption("verbose")}");

        // Generate help text for a specific command
        string helpText = parser.GenerateCommandHelp("process");
        Console.WriteLine($"\nHelp for 'process' command:\n{helpText}");

        // Generate full help text
        string fullHelp = parser.GenerateHelpText();
        Console.WriteLine($"\nFull CLI help:\n{fullHelp}");
    }
}
```

## CommandHandler

The `CommandHandler` class serves as the base class for all CLI commands in the GPU image processing application. It implements the command pattern with execution logic, argument parsing, validation, and dependency injection support. Command handlers provide descriptive information about their purpose through `GetDescription()` and `GetUsage()` methods, and execute operations via the `ExecuteAsync()` method. The class includes utility methods for accessing arguments (`GetArgument()`), checking flags (`HasFlag()`), and setting arguments from command-line input (`SetArguments()`).

### Usage Example

```csharp
using GpuImageProcessing.Cli;
using System;
using System.Threading.Tasks;

class Program
{
  static async Task Main()
  {
    // Setup dependency injection container
    var serviceProvider = new ServiceCollection()
      .AddSingleton<IServiceProvider>(sp => sp)
      .BuildServiceProvider();

    // Create a custom command handler by inheriting from CommandHandler
    var commandHandler = new ProcessImageCommandHandler(serviceProvider, "process-image");

    // Set command arguments (simulating: process-image --input /path/to/image.jpg --output /path/to/output.png --blur 5)
    string[] args = new[] { "process-image", "--input", "/path/to/image.jpg", "--output", "/path/to/output.png", "--blur", "5" };
    commandHandler.SetArguments(args);

    // Check if a flag is set
    bool hasBlurFlag = commandHandler.HasFlag("blur");
    Console.WriteLine($"Has blur flag: {hasBlurFlag}");

    // Get argument value with default fallback
    string inputPath = commandHandler.GetArgument("input", "/default/input.jpg");
    string outputPath = commandHandler.GetArgument("output", "/default/output.png");
    int blurRadius = int.Parse(commandHandler.GetArgument("blur", "3"));

    Console.WriteLine($"Processing image:");
    Console.WriteLine($"  Input: {inputPath}");
    Console.WriteLine($"  Output: {outputPath}");
    Console.WriteLine($"  Blur radius: {blurRadius}");

    // Get command description and usage
    Console.WriteLine($"\nDescription: {commandHandler.GetDescription()}");
    Console.WriteLine($"Usage: {commandHandler.GetUsage()}");

    // Execute the command
    int exitCode = await commandHandler.ExecuteAsync();
    Console.WriteLine($"\nCommand executed with exit code: {exitCode}");
  }
}

// Example custom command handler implementation
public class ProcessImageCommandHandler : CommandHandler
{
  public ProcessImageCommandHandler(IServiceProvider serviceProvider, string commandName)
    : base(serviceProvider, commandName) { }

  public override string GetDescription() => "Process an image with GPU acceleration using specified filters";

  public override string GetUsage() => "process-image --input <path> --output <path> [--blur <radius>] [--sharpen <amount>]";

  public override async Task<int> ExecuteAsync()
  {
    // Implementation would process the image using the parsed arguments
    PrintInfo($"Processing image from {GetArgument("input")} to {GetArgument("output")}");
    return 0; // Success
  }
}
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

## ImageProcessingException

The `ImageProcessingException` serves as the base exception for all image processing-related errors in the GPU image processing pipeline. It provides standardized error handling with properties for tracking error codes, numeric error codes, and timestamps. This exception is designed to be extended by more specific image processing exceptions while maintaining consistent error reporting across the system.

### Usage Example

```csharp
using GpuImageProcessing.Core.Exceptions;
using System;

try
{
    // Simulate an image processing error
    throw new ImageProcessingException(
        "Failed to process image due to GPU memory constraints",
        "GPU_MEMORY_LIMIT_EXCEEDED",
        new OutOfMemoryException("GPU memory allocation failed")
    );
}
catch (ImageProcessingException ex)
{
    Console.WriteLine($"Exception Message: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode ?? "N/A"}");
    Console.WriteLine($"Numeric Error Code: {ex.ErrorCode_Numeric?.ToString() ?? "N/A"}");
    Console.WriteLine($"Occurred At: {ex.OccurredAt:O}");
    Console.WriteLine($"Full Exception Details:\n{ex}");

    // Re-throw or handle the exception appropriately
    throw;
}

// Example with custom error code
var processingException = new ImageProcessingException(
    "Image processing pipeline failed",
    "PROCESSING_PIPELINE_ERROR"
);

// Example with numeric error code
var numericException = new ImageProcessingException(
    "Invalid image dimensions detected",
    "INVALID_DIMENSIONS"
);
numericException.ErrorCode_Numeric = 4001;
```

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

## FilterParameter

The `FilterParameter` class represents a configurable parameter for image filters with validation, normalization, and clamping capabilities. It defines the range, type, and metadata for filter parameters like blur radius, intensity values, or threshold levels. Parameters can be validated to ensure values are within bounds, normalized for UI controls, and cloned for safe reuse across filter instances.

### Usage Example

```csharp
using GpuImageProcessing.Core.Models;
using System;

// Create a blur radius parameter with range validation
var blurRadius = new FilterParameter
{
    Name = "BlurRadius",
    Value = 5.0f,
    Min = 1.0f,
    Max = 25.0f,
    Unit = "pixels",
    Description = "Radius for Gaussian blur effect"
};

// Validate the parameter value
bool isValid = blurRadius.IsValid();
Console.WriteLine($"Parameter is valid: {isValid}"); // True (5.0 is within 1.0-25.0 range)

// Clamp an out-of-range value
blurRadius.Value = 30.0f;
blurRadius.ClampValue();
Console.WriteLine($"Clamped value: {blurRadius.Value}"); // 25.0

// Create a normalized slider value (0-1 range) for UI
float normalized = blurRadius.GetNormalizedValue();
Console.WriteLine($"Normalized value: {normalized:F3}"); // ~0.923

// Set value from normalized input
blurRadius.SetFromNormalizedValue(0.5f);
Console.WriteLine($"Value from normalized 0.5: {blurRadius.Value}"); // ~13.0

// Clone for reuse in multiple filters
var clonedParameter = blurRadius.Clone();
Console.WriteLine($"Cloned parameter ID: {clonedParameter.Id}"); // Different Guid

// Create an intensity parameter with percentage unit
var intensity = new FilterParameter
{
    Name = "Intensity",
    Value = 0.8f,
    Min = 0.0f,
    Max = 1.0f,
    Unit = "%",
    Description = "Effect strength as percentage"
};

// Validate required parameter
var requiredParam = new FilterParameter
{
    Name = "Threshold",
    Value = 0.5f,
    Min = 0.0f,
    Max = 1.0f,
    IsRequired = true
};

Console.WriteLine($"Required parameter: {requiredParam.IsRequired}, Valid: {requiredParam.IsValid()}");
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

    ## MetricsPublisher

The `MetricsPublisher` class provides a centralized mechanism for publishing performance metrics to external monitoring systems. It supports multiple formats including JSON, Prometheus, and InfluxDB, allowing metrics to be sent to various observability platforms. The publisher buffers metrics in memory and automatically flushes them to registered endpoints, ensuring minimal impact on application performance while maintaining reliable metrics delivery.

### Usage Example

```csharp
using GpuImageProcessing.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create a metrics publisher with default buffer size (100 metrics)
        var publisher = new MetricsPublisher();
        
        // Register endpoints for different monitoring systems
        publisher.RegisterEndpoint(
            url: "https://prometheus.example.com/api/v1/write",
            format: MetricsFormat.Prometheus,
            apiKey: "prometheus-secret-key"
        );
        
        publisher.RegisterEndpoint(
            url: "https://influxdb.example.com/api/v2/write",
            format: MetricsFormat.InfluxDb,
            apiKey: "influxdb-secret-token"
        );
        
        // Record basic metrics
        publisher.RecordMetric("gpu_processing_time_ms", 45.2, 
            new Dictionary<string, string> { { "gpu_type", "NVIDIA RTX 4090" } });
        
        publisher.RecordMetric("batch_size", 150, 
            new Dictionary<string, string> { { "batch_name", "summer_photos_2024" } });
        
        // Record timing metrics
        publisher.RecordTiming("image_filtering", TimeSpan.FromMilliseconds(125.7), 
            new Dictionary<string, string> { { "filter_type", "gaussian_blur" } });
        
        // Manually flush metrics (automatically happens when buffer is full)
        await publisher.FlushAsync();
        
        // Dispose to ensure all metrics are flushed
        await publisher.DisposeAsync();
    }
}
```

## DatabaseConnectionPool

The `DatabaseConnectionPool` class manages a pool of database connections for efficient database operations within the GPU image processing pipeline. It provides connection lifecycle management, automatic connection creation and cleanup, and comprehensive statistics tracking. The pool maintains a minimum number of connections ready for use and scales up to a maximum limit based on demand, with automatic timeout handling for connection acquisition requests.

### Usage Example

```csharp
using GpuImageProcessing.Integration;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create a connection pool with default settings (min 5, max 20 connections, 30s timeout)
        var pool = new DatabaseConnectionPool(
            "Server=localhost;Database=gpu_images;User Id=app_user;Password=secure_password;Port=5432"
        );

        // Initialize the pool - creates minimum connections
        await pool.InitializeAsync();
        Console.WriteLine("Database connection pool initialized successfully.");

        // Acquire a connection from the pool
        var connection = await pool.AcquireConnectionAsync();
        Console.WriteLine($"Acquired connection: {connection.Id}");
        Console.WriteLine($"Connection state: {connection.State}");
        Console.WriteLine($"Total connections created: {pool.GetStatistics().TotalConnections}");

        // Use the connection (simulated - in real usage this would execute database operations)
        // connection.ExecuteQuery(...);

        // Release the connection back to the pool when done
        await pool.ReleaseConnectionAsync(connection);
        Console.WriteLine("Connection released back to pool.");

        // Get pool statistics
        var stats = pool.GetStatistics();
        Console.WriteLine($"Pool Statistics:");
        Console.WriteLine($"  Available: {stats.AvailableCount}");
        Console.WriteLine($"  In Use: {stats.InUseCount}");
        Console.WriteLine($"  Idle: {stats.IdleCount}");
        Console.WriteLine($"  Total: {stats.TotalConnections}");
        Console.WriteLine($"  Total Requests: {stats.TotalRequests}");
        Console.WriteLine($"  Min Pool Size: {stats.MinPoolSize}");
        Console.WriteLine($"  Max Pool Size: {stats.MaxPoolSize}");

        // Close a connection (removes from pool)
        await pool.CloseConnectionAsync(connection);
        Console.WriteLine("Connection closed and removed from pool.");

        // Clean up idle connections older than 5 minutes
        await pool.CleanupIdleConnectionsAsync(TimeSpan.FromMinutes(5));
        Console.WriteLine("Idle connections cleanup completed.");
    }
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



## RemoteImageService

The `RemoteImageService` class provides a robust service for fetching images from remote URLs with support for authentication, retry logic, and bandwidth throttling. It handles downloading images from various sources, validates image integrity, and manages trusted sources with automatic authorization. This service is essential for integrating the GPU image processing pipeline with external image repositories and APIs.

### Usage Example

```csharp
using GpuImageProcessing.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create remote image service with default settings (3 retries, 30s timeout)
        var remoteImageService = new RemoteImageService(maxRetries: 3, timeoutSeconds: 30);

        // Register a trusted source for automatic authorization
        remoteImageService.RegisterTrustedSource(
            url: "https://api.example.com/images/",
            apiKey: "your-api-key-here"
        );

        // Download a single image
        var imageUrl = "https://api.example.com/images/sample.jpg";
        var result = await remoteImageService.DownloadImageAsync(imageUrl);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Successfully downloaded image from {result.Data.SourceUrl}");
            Console.WriteLine($"Content type: {result.Data.ContentType}");
            Console.WriteLine($"Size: {result.Data.SizeBytes:N0} bytes");
            Console.WriteLine($"Downloaded at: {result.Data.DownloadedAt}");
        }
        else
        {
            Console.WriteLine($"Failed to download image: {result.Error}");
        }

        // Download multiple images concurrently with rate limiting
        var imageUrls = new List<string>
        {
            "https://api.example.com/images/photo1.jpg",
            "https://api.example.com/images/photo2.jpg",
            "https://api.example.com/images/photo3.jpg"
        };

        var batchResults = await remoteImageService.DownloadImagesAsync(
            imageUrls,
            maxConcurrentDownloads: 5
        );

        Console.WriteLine($"Downloaded {batchResults.Count(r => r.IsSuccess)} out of {batchResults.Count} images");

        // Validate downloaded image data
        bool isValidImage = remoteImageService.ValidateImageData(
            result.Data.ImageData,
            result.Data.ContentType
        );
        Console.WriteLine($"Image validation: {(isValidImage ? "PASSED" : "FAILED")}");
    }
}
```

## HttpImageClient

The `HttpImageClient` class provides an HTTP client wrapper for downloading and uploading images from remote sources. It includes retry logic with exponential backoff, timeout handling, content validation, and comprehensive error reporting. This client is essential for integrating the GPU image processing pipeline with external image sources and destinations.

### Usage Example

```csharp
using GpuImageProcessing.Integration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Setup logging (typically via dependency injection in real applications)
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        ILogger<HttpImageClient> logger = loggerFactory.CreateLogger<HttpImageClient>();

        // Create HTTP image client with default settings (3 retries, 30s timeout)
        using var httpClient = new HttpImageClient(logger);

        // Download an image from a URL
        string imageUrl = "https://example.com/images/sample.jpg";
        string outputPath = "/data/images/downloaded/sample.jpg";
        
        bool downloadSuccess = await httpClient.DownloadImageAsync(imageUrl, outputPath);
        Console.WriteLine($"Download {(downloadSuccess ? "succeeded" : "failed")}");

        // Verify an image URL is accessible before processing
        bool isAccessible = await httpClient.VerifyImageUrlAsync(imageUrl);
        Console.WriteLine($"URL accessible: {isAccessible}");

        // Upload a processed image to a remote endpoint
        string processedImagePath = "/output/processed/sample-enhanced.jpg";
        string uploadUrl = "https://api.example.com/upload/image";
        
        bool uploadSuccess = await httpClient.UploadImageAsync(processedImagePath, uploadUrl);
        Console.WriteLine($"Upload {(uploadSuccess ? "succeeded" : "failed")}");

        // Access client properties for debugging
        Console.WriteLine($"Last URL: {httpClient.Url}");
        Console.WriteLine($"Last HTTP Status: {httpClient.HttpStatusCode}");
    }
}
```

## WebhookHandler

The `WebhookHandler` class manages webhook subscriptions and event dispatching for external integrations. It allows registering and unregistering webhook endpoints, dispatching events to subscribers, and tracking subscription status including retry policies and failure counts. This component is essential for integrating the GPU image processing pipeline with external systems and services.

### Usage Example

```csharp
using GpuImageProcessing.Integration;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create a webhook handler instance
        var webhookHandler = new WebhookHandler("image-processing-events");

        // Register a webhook endpoint for image processing events
        string webhookUrl = "https://api.example.com/webhooks/image-processing";
        bool registrationSuccess = webhookHandler.RegisterWebhook(
            webhookUrl: webhookUrl,
            eventType: "ImageProcessed",
            isActive: true
        );

        Console.WriteLine($"Webhook registration {(registrationSuccess ? "succeeded" : "failed")}");

        // Get all registered webhooks
        var subscriptions = webhookHandler.GetWebhooks();
        Console.WriteLine($"Total webhook subscriptions: {subscriptions.Count}");

        foreach (var subscription in subscriptions)
        {
            Console.WriteLine($" - ID: {subscription.Id}, URL: {subscription.WebhookUrl}, " +
                           $"Event: {subscription.EventType}, Active: {subscription.IsActive}");
        }

        // Dispatch an event to all registered webhooks
        var imageProcessedEvent = new ImageProcessedEvent
        {
            ImageId = Guid.NewGuid(),
            ProcessingTimeMs = 45.2,
            OutputPath = "/output/processed_image.jpg",
            Success = true
        };

        await webhookHandler.DispatchEventAsync("ImageProcessed", imageProcessedEvent);

        // Unregister a webhook when it's no longer needed
        bool unregistrationSuccess = webhookHandler.UnregisterWebhook(webhookUrl);
        Console.WriteLine($"Webhook unregistration {(unregistrationSuccess ? "succeeded" : "failed")}");

        // Configure retry policy for webhook delivery
        webhookHandler.RetryPolicy.MaxRetries = 3;
        webhookHandler.RetryPolicy.InitialDelayMs = 200;
        webhookHandler.RetryPolicy.MaxDelayMs = 5000;
        webhookHandler.RetryPolicy.BackoffMultiplier = 2.0;

        Console.WriteLine($"Retry policy configured: MaxRetries={webhookHandler.RetryPolicy.MaxRetries}, " +
                       $"InitialDelay={webhookHandler.RetryPolicy.InitialDelayMs}ms, " +
                       $"MaxDelay={webhookHandler.RetryPolicy.MaxDelayMs}ms");

        // Dispose the handler to clean up resources
        webhookHandler.Dispose();
    }
}
```

## Filter

The `Filter` class represents an image filter with configurable parameters for GPU-accelerated image processing. It encapsulates filter metadata including name, type, description, and processing order, along with runtime settings and parameter collections. Filters can be created predefined for common operations like Gaussian blur, bilateral filtering, median filtering, and thresholding, or customized with custom kernel code for specialized processing needs.

### Usage Example

```csharp
using GpuImageProcessing.Core.Models;
using GpuImageProcessing.Core.Constants;
using System;
using System.Collections.Generic;

// Create a custom filter with specific parameters
var customFilter = new Filter
{
    Name = "CustomEdgeDetection",
    Type = FilterType.EdgeDetection,
    Description = "Sobel edge detection filter with configurable threshold",
    IsActive = true,
    ProcessingOrder = 1,
    KernelCode = @"
    __kernel void EdgeDetection(__global const float4* input, __global float4* output, 
                             int width, int height, float threshold)
    {
        // Edge detection kernel implementation
    }
    "
};

// Add custom parameters
customFilter.AddParameter(new FilterParameter
{
    Name = "Threshold",
    Value = 0.5f,
    Min = 0.0f,
    Max = 1.0f
});

customFilter.AddParameter(new FilterParameter
{
    Name = "EdgeStrength",
    Value = 1.5f,
    Min = 0.5f,
    Max = 3.0f
});

// Create a predefined Gaussian blur filter
var gaussianBlur = Filter.CreatePredefined(FilterType.Gaussian);
Console.WriteLine($"Predefined filter: {gaussianBlur.Name} ({gaussianBlur.Type})");
Console.WriteLine($"Parameters: {gaussianBlur.Parameters.Count}");

// Get and update a parameter value
if (gaussianBlur.UpdateParameterValue("Sigma", 2.5f))
{
    Console.WriteLine($"Updated Sigma to: {gaussianBlur.GetParameter("Sigma")?.Value}");
}

// Validate all parameters
bool isValid = gaussianBlur.ValidateParameters();
Console.WriteLine($"Parameter validation: {(isValid ? "PASSED" : "FAILED")}");

// Get filter configuration as string
string config = gaussianBlur.GetConfiguration();
Console.WriteLine($"Filter configuration: {config}");

// Access applied settings
var settings = new Dictionary<string, object> 
{
    { "Quality", 0.95 },
    { "PreserveAlpha", true }
};
gaussianBlur.AppliedSettings = settings;

Console.WriteLine($"Filter created at: {gaussianBlur.CreatedAt}");
Console.WriteLine($"Filter ID: {gaussianBlur.Id}");
```

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

## BenchmarkSuiteConfigurationTests

The `BenchmarkSuiteConfigurationTests` class contains unit tests for the `BenchmarkSuiteConfiguration` class, which validates benchmark suite configurations including run names, categories, and accuracy levels.

These tests ensure that benchmark configurations are properly validated and that presets for different environments (CI vs Release) produce the expected results.

Example usage:

```csharp
using GpuImageProcessing.Benchmarks;
using GpuImageProcessing.Tests.Benchmarking;
using System;

public class BenchmarkSuiteConfigurationTests
{
    public void Validate_ValidDefaultConfig_ReturnsNoErrors()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.Default;
        
        // Act
        var errors = config.Validate();
        
        // Assert
        Assert.Empty(errors);
    }

    public void Validate_BlankRunName_ReturnsNameError()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.Default with
        {
            RunName = string.Empty
        };
        
        // Act
        var errors = config.Validate();
        
        // Assert
        Assert.Contains(errors, e => e.Contains("RunName"));
    }

    public void Validate_AllCategoriesDisabled_ReturnsAtLeastOneError()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.Default with
        {
            Categories = new[] { false, false, false, false, false }
        };
        
        // Act
        var errors = config.Validate();
        
        // Assert
        Assert.Contains(errors, e => e.Contains("at least one category"));
    }

    public void GetEnabledCategories_DefaultConfig_ContainsFourCategories()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.Default;
        
        // Act
        var enabledCategories = config.GetEnabledCategories();
        
        // Assert
        Assert.Equal(4, enabledCategories.Count);
    }

    public void GetEnabledCategories_AllEnabled_ContainsFiveCategories()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.Default with
        {
            Categories = new[] { true, true, true, true, true }
        };
        
        // Act
        var enabledCategories = config.GetEnabledCategories();
        
        // Assert
        Assert.Equal(5, enabledCategories.Count);
    }

    public void ForCi_Preset_HasQuickAccuracyAndThreeCategories()
    {
        // Arrange & Act
        var ciConfig = BenchmarkSuiteConfiguration.ForCi;
        var enabledCategories = ciConfig.GetEnabledCategories();
        
        // Assert
        Assert.Equal("Quick", ciConfig.AccuracyLevel);
        Assert.Equal(3, enabledCategories.Count);
    }

    public void ForRelease_Preset_HasThoroughAccuracyAndAllCategories()
    {
        // Arrange & Act
        var releaseConfig = BenchmarkSuiteConfiguration.ForRelease;
        var enabledCategories = releaseConfig.GetEnabledCategories();
        
        // Assert
        Assert.Equal("Thorough", releaseConfig.AccuracyLevel);
        Assert.Equal(5, enabledCategories.Count);
    }

    public void ForRelease_Preset_IsValid()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.ForRelease;
        
        // Act
        var errors = config.Validate();
        
        // Assert
        Assert.Empty(errors);
    }

    public void ForCi_Preset_IsValid()
    {
        // Arrange
        var config = BenchmarkSuiteConfiguration.ForCi;
        
        // Act
        var errors = config.Validate();
        
        // Assert
        Assert.Empty(errors);
    }
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

## FilterServiceTests

The `FilterServiceTests` class provides comprehensive unit tests for the `FilterService` class, focusing on filter application, creation, retrieval, and error handling. These tests validate the service's ability to properly apply filters to images, handle invalid configurations, manage active/inactive filters, and correctly filter configurations by type. The test suite ensures robust error handling and proper state management when working with image filters.

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
        ILogger<FilterService> logger = loggerFactory.CreateLogger<FilterService>();

        // Initialize the filter service with repository and logger
        var repository = new FilterConfigurationRepository();
        var filterService = new FilterService(repository, logger);

        // Create a grayscale filter configuration
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

        // Create the filter
        var createdFilter = await filterService.CreateFilterAsync(grayscaleFilter);
        Console.WriteLine($"Created filter: {createdFilter.Name} with ID: {createdFilter.Id}");

        // Create a test image
        var image = new Image(1920, 1080, 3)
        {
            Id = Guid.NewGuid(),
            FileName = "sample.jpg",
            FilePath = @"/images/sample.jpg",
            Format = ImageFormat.Jpeg,
            ColorSpace = ColorSpace.Rgb,
            BitsPerPixel = 24
        };

        // Apply the filter to the image
        var result = await filterService.ApplyFilterAsync(image, createdFilter.Id);
        Console.WriteLine($"Applied filter {createdFilter.Name} to image {image.Id}");
        Console.WriteLine($"Image color space after filter: {result.ColorSpace}");

        // Get filters by type
        var blurFilters = await filterService.GetFiltersByTypeAsync(FilterType.Grayscale);
        Console.WriteLine($"Found {blurFilters.Count} grayscale filter(s)");
    }
}
```

## GpuManagementServiceTests

The `GpuManagementServiceTests` class provides comprehensive unit tests for the `GpuManagementService` class, validating GPU device management functionality including device discovery, memory allocation, validation, and performance monitoring. These tests ensure the service correctly handles device operations, memory management, error conditions, and edge cases across different hardware configurations.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Tests.Services;
using Microsoft.Extensions.Logging;
using System;

// Initialize the service with logging (typically via dependency injection)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<GpuManagementService> logger = loggerFactory.CreateLogger<GpuManagementService>();

// Create the service instance
var gpuService = new GpuManagementService(logger);

// Get available devices - should always return at least one device
var availableDevices = gpuService.GetAvailableDevices();
Console.WriteLine($"Available devices: {availableDevices.Count}");

// Get the best performing device for processing
var bestDevice = gpuService.GetBestDevice();
if (bestDevice != null)
{
    Console.WriteLine($"Best device: {bestDevice.Name}");
    
    // Test memory allocation with valid parameters
    long requiredMemory = 100 * 1024 * 1024; // 100 MB
    bool allocationSuccess = gpuService.AllocateMemory(requiredMemory, bestDevice.Id);
    Console.WriteLine($"Memory allocation {(allocationSuccess ? "succeeded" : "failed")} for {requiredMemory / (1024 * 1024)} MB");
    
    // Test device validation
    bool isValid = gpuService.ValidateDevice(bestDevice.Id, requiredMemory, 1);
    Console.WriteLine($"Device validation: {(isValid ? "PASSED" : "FAILED")}");
    
    // Test memory statistics
    var memoryStats = gpuService.GetMemoryStatistics();
    Console.WriteLine($"Total allocated memory: {memoryStats["TotalAllocatedMemory"]} bytes");
    Console.WriteLine($"Memory usage: {memoryStats["MemoryUsagePercent"]}%");
    
    // Test deallocation
    gpuService.DeallocateMemory(requiredMemory, bestDevice.Id);
    Console.WriteLine("Memory deallocated successfully.");
}

// Test constructor validation
try
{
    var invalidService = new GpuManagementService(null!);
    Console.WriteLine("ERROR: Constructor should throw ArgumentNullException");
}
catch (ArgumentNullException)
{
    Console.WriteLine("Constructor validation: PASSED (throws ArgumentNullException for null logger)");
}

// Get device with most memory
var memoryRichDevice = gpuService.GetDeviceWithMostMemory();
if (memoryRichDevice != null)
{
    Console.WriteLine($"Device with most memory: {memoryRichDevice.Name}");
}

// Test edge cases
if (bestDevice != null)
{
    // Test zero bytes allocation (should return false)
    bool zeroBytesResult = gpuService.AllocateMemory(0, bestDevice.Id);
    Console.WriteLine($"Zero bytes allocation: {(zeroBytesResult == false ? "PASSED" : "FAILED")}");
    
    // Test negative bytes allocation (should return false)
    bool negativeBytesResult = gpuService.AllocateMemory(-100, bestDevice.Id);
    Console.WriteLine($"Negative bytes allocation: {(negativeBytesResult == false ? "PASSED" : "FAILED")}");
    
    // Test invalid device ID (should throw GpuException)
    try
    {
        gpuService.AllocateMemory(1024, Guid.NewGuid());
        Console.WriteLine("Invalid device test: FAILED (should throw exception)");
    }
    catch (GpuException)
    {
        Console.WriteLine("Invalid device test: PASSED (throws GpuException)");
    }
}
```

## ImageProcessingServiceTests

The `ImageProcessingServiceTests` class provides comprehensive unit tests for the `ImageProcessingService` class, focusing on image processing pipelines, error handling, and statistics calculation. These tests validate the service's ability to handle various scenarios including image validation, GPU device availability, filter application, and result tracking across different hardware configurations and edge cases.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Tests.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Initialize required services with logging (typically via dependency injection)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<ImageProcessingService> logger = loggerFactory.CreateLogger<ImageProcessingService>();

// Create mock repositories and services (in real usage these would be actual implementations)
var imageRepository = new ImageRepository();
var filterRepository = new FilterConfigurationRepository();
var resultRepository = new ProcessingResultRepository();
var filterService = new FilterService(filterRepository, logger);
var gpuService = new GpuManagementService(logger);
var performanceService = new PerformanceMonitoringService(logger);

// Create the image processing service
var processingService = new ImageProcessingService(
    imageRepository,
    filterRepository,
    resultRepository,
    filterService,
    gpuService,
    performanceService,
    logger
);

// Create a test image
var testImage = new Image(1920, 1080, 3)
{
    Id = Guid.NewGuid(),
    FileName = "test_image.png",
    FilePath = @"/test/test_image.png",
    Format = ImageFormat.Png,
    ColorSpace = ColorSpace.Rgb,
    BitsPerPixel = 24
};

// Test 1: Process image with a single filter
var filterId = Guid.NewGuid();
var filterConfig = new FilterConfiguration
{
    Id = filterId,
    Name = "Test Blur",
    FilterType = FilterType.Blur,
    IsActive = true
};

// Process the image (this would fail if image doesn't exist or GPU is unavailable)
try
{
    var result = await processingService.ProcessImageAsync(testImage.Id, [filterId]);
    Console.WriteLine($"Image processing result: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
}
catch (InvalidImageException)
{
    Console.WriteLine("Image not found or invalid");
}
catch (GpuException)
{
    Console.WriteLine("No GPU device available");
}

// Test 2: Process image with multiple filters
var filterId2 = Guid.NewGuid();
try
{
    var result = await processingService.ProcessImageAsync(testImage.Id, [filterId, filterId2]);
    Console.WriteLine($"Multiple filter processing result: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
}
catch (Exception ex)
{
    Console.WriteLine($"Multiple filter processing failed: {ex.Message}");
}

// Test 3: Get processing statistics
var stats = await processingService.GetStatisticsAsync();
Console.WriteLine($"Total images: {stats["TotalImages"]}");
Console.WriteLine($"Processed images: {stats["ProcessedImages"]}");
Console.WriteLine($"Success rate: {stats["SuccessRate"]:P0}");
Console.WriteLine($"Average processing time: {stats["AverageProcessingTime"]}ms");

// Test 4: Get processing result for an image
var processingResult = await processingService.GetProcessingResultAsync(testImage.Id);
if (processingResult != null)
{
    Console.WriteLine($"Found processing result for image {processingResult.ImageId}");
    Console.WriteLine($"Status: {(processingResult.IsSuccessful ? "SUCCESS" : "FAILED")}");
}
else
{
    Console.WriteLine("No processing result found for image");
}
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

## ImageProcessingExtensions

The `ImageProcessingExtensions` class provides a comprehensive set of extension methods for common image processing operations including color space conversion, format detection, resolution validation, memory calculation, and performance estimation. These methods extend both `ImageFormat` and `Image` types, offering utility functions that are essential for image processing workflows and batch operations.

### Key Features
- Color space determination from image format
- File extension and format conversion utilities
- Resolution validation against application limits
- Memory requirement calculations for processing
- Performance estimation for filter operations
- Aspect ratio analysis and description
- Filter applicability checking

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using GpuImageProcessing.Utilities;
using System;

// Example 1: Get color space for a specific image format
var jpegColorSpace = ImageFormat.Jpeg.GetColorSpaceForFormat();
Console.WriteLine($"JPEG color space: {jpegColorSpace}");

var pngColorSpace = ImageFormat.Png.GetColorSpaceForFormat();
Console.WriteLine($"PNG color space: {pngColorSpace}");

// Example 2: Get file extension from format
var jpegExtension = ImageFormat.Jpeg.GetFileExtension();
Console.WriteLine($"JPEG extension: {jpegExtension}");

var pngExtension = ImageFormat.Png.GetFileExtension();
Console.WriteLine($"PNG extension: {pngExtension}");

// Example 3: Parse format from file extension
var formatFromExtension = ".jpg".GetFormatFromExtension();
Console.WriteLine($"Format from '.jpg': {formatFromExtension}");

// Example 4: Create and validate an image
var image = new Image(1920, 1080, 3)
{
    Format = ImageFormat.Jpeg,
    ColorSpace = ColorSpace.Rgb
};

// Check if resolution is valid
bool isValid = image.IsResolutionValid();
Console.WriteLine($"Resolution valid: {isValid}");

// Get aspect ratio description
string aspectRatio = image.GetAspectRatioDescription();
Console.WriteLine($"Aspect ratio: {aspectRatio}");

// Calculate memory requirements for processing
long memoryRequired = image.GetMemoryRequirement();
Console.WriteLine($"Memory required: {memoryRequired:N0} bytes");

// Estimate processing time for a specific filter
var filterConfig = new FilterConfiguration
{
    FilterType = FilterType.GaussianBlur,
    Name = "Gaussian Blur"
};

double processingTime = image.EstimateProcessingTime(filterConfig);
Console.WriteLine($"Estimated processing time: {processingTime:F2}ms");

// Check if a filter can be applied
bool canApplyBlur = image.CanApplyFilter(FilterType.Blur);
Console.WriteLine($"Can apply blur filter: {canApplyBlur}");

// Calculate total bytes for image data
long totalBytes = image.CalculateTotalBytes();
Console.WriteLine($"Total image data size: {totalBytes:N0} bytes");
```

## ConcurrencyAndConfigurationTests

The `ConcurrencyAndConfigurationTests` class provides comprehensive integration tests for validating concurrent image processing operations and configuration handling in the GPU image processing pipeline. It tests thread safety, performance under load, proper device selection, filter configuration validation, memory management, and error handling across multiple concurrent operations.

### Usage Example

```csharp
using GpuImageProcessing.Tests.Integration;
using GpuImageProcessing.Domain;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create an instance of the concurrency and configuration tests
        var concurrencyTests = new ConcurrencyAndConfigurationTests();

        // Test 1: Concurrent image processing with multiple threads
        await concurrencyTests.ConcurrentImageProcessing_MultipleThreads_CompletesSuccessfully();
        Console.WriteLine("Concurrent processing test passed!");

        // Test 2: Performance metrics under concurrent load
        var metrics = await concurrencyTests.PerformanceMetrics_UnderConcurrentLoad_CalculatedCorrectly();
        Console.WriteLine($"Performance metrics: {metrics.TotalOperationsCount} operations, " +
                         $"Success rate: {metrics.GetSuccessRate():F2}%");

        // Test 3: Image dimensions with various configurations
        await concurrencyTests.ImageDimensions_VariousConfigurations_AllProcessCorrectly();
        Console.WriteLine("Image dimensions test passed!");

        // Test 4: Complex filter chain execution order
        await concurrencyTests.FilterChain_ComplexPipeline_ExecutesInOrder();
        Console.WriteLine("Filter chain execution test passed!");

        // Test 5: Large-scale image batch processing
        var largeBatch = new ImageBatch { Name = "LargeScaleBatch" };
        for (int i = 0; i < 100; i++)
        {
            largeBatch.AddImage(Guid.NewGuid());
        }
        await concurrencyTests.ImageBatch_LargeScale_HandlesMultipleImages(largeBatch);
        Console.WriteLine("Large-scale batch test passed!");

        // Test 6: GPU memory stress test
        await concurrencyTests.GpuMemory_StressTest_AllocateAndDeallocateMultipleTimes();
        Console.WriteLine("GPU memory stress test passed!");

        // Test 7: Multiple device selection and best device selection
        var bestDevice = await concurrencyTests.GpuDeviceSelection_MultipleDevices_SelectsBestOne();
        Console.WriteLine($"Best device selected: {bestDevice?.Name ?? "None found"}");

        // Test 8: Filter configuration validation and rejection
        await concurrencyTests.FilterConfiguration_InactiveFilter_RejectedDuringApplication();
        Console.WriteLine("Filter configuration validation test passed!");

        // Test 9: Result tracking for multiple operations
        var resultTracking = await concurrencyTests.ResultTracking_MultipleOperations_AllRecorded();
        Console.WriteLine($"Result tracking: {resultTracking.TotalOperationsCount} operations tracked");

        // Test 10: Image validation with boundary values
        await concurrencyTests.ImageValidation_BoundaryValues_AcceptedCorrectly();
        Console.WriteLine("Image validation test passed!");

        // Test 11: Batch progress calculation edge cases
        var progressCalculation = await concurrencyTests.ImageBatch_ProgressCalculation_HandlesEdgeCases();
        Console.WriteLine($"Progress calculation: {progressCalculation:P0}");

        // Test 12: Multiple filters of same type creation
        await concurrencyTests.FilterService_MultipleFiltersOfSameType_AllCreatedSuccessfully();
        Console.WriteLine("Multiple filters creation test passed!");

        // Test 13: Performance monitoring snapshot and reset
        var monitoringResult = await concurrencyTests.PerformanceMonitoring_SnapshotAndReset_ManagesHistoryCorrectly();
        Console.WriteLine("Performance monitoring test passed!");

        // Test 14: Filter chain step reordering maintains integrity
        await concurrencyTests.FilterChain_ReorderSteps_MaintainsIntegrity();
        Console.WriteLine("Filter chain reordering test passed!");
    }
}
```

## ImageDomainTests

The `ImageDomainTests` class provides comprehensive unit tests for the `Image` domain type and related batch operations. It validates image validation logic, pixel data calculations, status tracking, progress calculation, and processing result metrics to ensure the core domain types behave correctly under various scenarios.

### Usage Example

```csharp
using GpuImageProcessing.Tests.Domain;
using GpuImageProcessing.Domain;
using System;

// Create a test instance using the same helper method from the test class
var image = new Image
{
    FilePath = "/images/sample.png",
    FileName = "sample.png",
    Format = ImageFormat.Png,
    Width = 1920,
    Height = 1080,
    BitsPerPixel = 24,
    FileSizeBytes = 1024 * 1024
};

// Validate the image (should return true for valid images)
bool isValid = image.Validate();
Console.WriteLine($"Image validation result: {isValid}");

// Calculate required pixel data size
long pixelDataSize = image.CalculatePixelDataSize();
Console.WriteLine($"Pixel data size: {pixelDataSize:N0} bytes");

// Mark image as processing and then completed
image.MarkAsProcessing();
image.MarkAsCompleted("/output/processed_sample.png");
Console.WriteLine($"Image status: {image.Status}");
Console.WriteLine($"Output path: {image.ProcessedOutputPath}");

// Create a batch and track progress
var batch = new ImageBatch { TotalImages = 100 };
batch.MarkImageProcessed(success: true);  // 1 processed
batch.MarkImageProcessed(success: true);  // 2 processed
batch.MarkImageProcessed(success: false); // 1 failed

// Get progress percentage
double progress = batch.GetProgressPercentage();
Console.WriteLine($"Batch progress: {progress:P0}");

// Create a processing result with multiple filters
var result = new ProcessingResult { ImageId = Guid.NewGuid() };
result.AddFilterApplied("GaussianBlur", FilterType.Blur, 15.5);
result.AddFilterApplied("Sharpen", FilterType.Sharpen, 8.0);
result.AddFilterApplied("Grayscale", FilterType.Grayscale, 3.5);

// Get total filter execution time
double totalFilterTime = result.GetTotalFilterExecutionTime();
Console.WriteLine($"Total filter execution time: {totalFilterTime}ms");
```

## ImageProcessingServiceTests

The `ImageProcessingServiceTests` class provides comprehensive unit tests for the `ImageProcessingService` class, focusing on image processing pipelines, error handling, and statistics calculation. These tests validate the service's ability to handle various scenarios including image validation, GPU device availability, filter application, and result tracking across different hardware configurations and edge cases.

### Usage Example

```csharp
using GpuImageProcessing.Services;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Tests.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Initialize required services with logging (typically via dependency injection)
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger<ImageProcessingService> logger = loggerFactory.CreateLogger<ImageProcessingService>();

// Create mock repositories and services (in real usage these would be actual implementations)
var imageRepository = new ImageRepository();
var filterRepository = new FilterConfigurationRepository();
var resultRepository = new ProcessingResultRepository();
var filterService = new FilterService(filterRepository, logger);
var gpuService = new GpuManagementService(logger);
var performanceService = new PerformanceMonitoringService(logger);

// Create the image processing service
var processingService = new ImageProcessingService(
    imageRepository,
    filterRepository,
    resultRepository,
    filterService,
    gpuService,
    performanceService,
    logger
);

// Create a test image
var testImage = new Image(1920, 1080, 3)
{
    Id = Guid.NewGuid(),
    FileName = "test_image.png",
    FilePath = @"/test/test_image.png",
    Format = ImageFormat.Png,
    ColorSpace = ColorSpace.Rgb,
    BitsPerPixel = 24
};

// Test 1: Process image with a single filter
var filterId = Guid.NewGuid();
var filterConfig = new FilterConfiguration
{
    Id = filterId,
    Name = "Test Blur",
    FilterType = FilterType.Blur,
    IsActive = true
};

// Process the image (this would fail if image doesn't exist or GPU is unavailable)
try
{
    var result = await processingService.ProcessImageAsync(testImage.Id, [filterId]);
    Console.WriteLine($"Image processing result: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
}
catch (InvalidImageException)
{
    Console.WriteLine("Image not found or invalid");
}
catch (GpuException)
{
    Console.WriteLine("No GPU device available");
}

// Test 2: Process image with multiple filters
var filterId2 = Guid.NewGuid();
try
{
    var result = await processingService.ProcessImageAsync(testImage.Id, [filterId, filterId2]);
    Console.WriteLine($"Multiple filter processing result: {(result.IsSuccessful ? "SUCCESS" : "FAILED")}");
}
catch (Exception ex)
{
    Console.WriteLine($"Multiple filter processing failed: {ex.Message}");
}

// Test 3: Get processing statistics
var stats = await processingService.GetStatisticsAsync();
Console.WriteLine($"Total images: {stats["TotalImages"]}");
Console.WriteLine($"Processed images: {stats["ProcessedImages"]}");
Console.WriteLine($"Success rate: {stats["SuccessRate"]:P0}");
Console.WriteLine($"Average processing time: {stats["AverageProcessingTime"]}ms");

// Test 4: Get processing result for an image
var processingResult = await processingService.GetProcessingResultAsync(testImage.Id);
if (processingResult != null)
{
    Console.WriteLine($"Found processing result for image {processingResult.ImageId}");
    Console.WriteLine($"Status: {(processingResult.IsSuccessful ? "SUCCESS" : "FAILED")}");
}
else
{
    Console.WriteLine("No processing result found for image");
}
```

## PerformanceMonitoringServiceTests

The `PerformanceMonitoringServiceTests` class provides comprehensive unit tests for the `PerformanceMonitoringService` that validate performance monitoring functionality. These tests cover constructor validation, metric recording, system metric updates, throughput tracking, snapshot and reset operations, metrics history management, and performance reporting to ensure the performance monitoring service behaves correctly under various scenarios.

### Usage Example

```csharp
using GpuImageProcessing.Tests.Services;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

class Program
{
    static void Main()
    {
        // Setup mock logger (typically via dependency injection in real applications)
        var loggerMock = new Mock<ILogger<PerformanceMonitoringService>>();
        
        // Create the service under test
        var performanceMonitoringService = new PerformanceMonitoringService(loggerMock.Object);
        
        // Test 1: Constructor validation - should throw when null logger provided
        try
        {
            var invalidService = new PerformanceMonitoringService(null!);
            Console.WriteLine("ERROR: Constructor should throw ArgumentNullException for null logger");
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine("✓ Constructor correctly throws ArgumentNullException for null logger");
        }
        
        // Test 2: Record successful operation metrics
        performanceMonitoringService.RecordOperation(100, success: true);
        var metrics = performanceMonitoringService.GetCurrentMetrics();
        Console.WriteLine($"✓ Recorded successful operation: Total={metrics.TotalOperationsCount}, Failed={metrics.FailedOperationsCount}, AvgTime={metrics.AverageExecutionTimeMs:F2}ms");
        
        // Test 3: Record failed operation metrics
        performanceMonitoringService.RecordOperation(50, success: false);
        metrics = performanceMonitoringService.GetCurrentMetrics();
        Console.WriteLine($"✓ Recorded failed operation: Total={metrics.TotalOperationsCount}, Failed={metrics.FailedOperationsCount}");
        
        // Test 4: Update system metrics (CPU, memory, GPU)
        performanceMonitoringService.UpdateSystemMetrics(
            cpuPercent: 75.5,
            memoryBytes: 2_000_000_000,
            gpuMemoryBytes: 1_000_000_000,
            gpuUtilization: 85.0
        );
        metrics = performanceMonitoringService.GetCurrentMetrics();
        Console.WriteLine($"✓ Updated system metrics: CPU={metrics.CpuUsagePercent:F1}%, Memory={metrics.MemoryUsedBytes:N0} bytes, GPU={metrics.GpuUtilizationPercent:F1}%");
        
        // Test 5: Update throughput metrics
        performanceMonitoringService.UpdateThroughput(
            pixelsPerSecond: 1_000_000,
            megabytesPerSecond: 500.5
        );
        metrics = performanceMonitoringService.GetCurrentMetrics();
        Console.WriteLine($"✓ Updated throughput: {metrics.ImagePixelsProcessedPerSecond:N0} pixels/s, {metrics.ThroughputMegabytesPerSecond:F1} MB/s");
        
        // Test 6: Snapshot and reset metrics
        var snapshot = performanceMonitoringService.SnapshotAndReset();
        Console.WriteLine($"✓ Snapshot created: TotalOperations={snapshot.TotalOperationsCount}, Failed={snapshot.FailedOperationsCount}");
        
        var resetMetrics = performanceMonitoringService.GetCurrentMetrics();
        Console.WriteLine($"✓ Metrics reset: TotalOperations={resetMetrics.TotalOperationsCount}, Failed={resetMetrics.FailedOperationsCount}");
        
        // Test 7: Get metrics history
        performanceMonitoringService.RecordOperation(100, true);
        performanceMonitoringService.SnapshotAndReset();
        performanceMonitoringService.RecordOperation(200, true);
        var history = performanceMonitoringService.GetMetricsHistory();
        Console.WriteLine($"✓ Metrics history retrieved: {history.Count} snapshots");
        
        // Test 8: Get average metrics over time window
        var averages = performanceMonitoringService.GetAverageMetrics(lastMinutes: 60);
        Console.WriteLine($"✓ Average metrics calculated: {(averages.Count > 0 ? averages.Count + " metrics" : "no metrics")}");
        
        // Test 9: Generate performance report
        performanceMonitoringService.RecordOperation(100, true);
        performanceMonitoringService.RecordOperation(150, false);
        performanceMonitoringService.UpdateSystemMetrics(75, 2_000_000_000, 1_000_000_000, 85.0);
        performanceMonitoringService.UpdateThroughput(1_000_000, 500);
        
        string report = performanceMonitoringService.GetPerformanceReport();
        Console.WriteLine("✓ Performance report generated:");
        Console.WriteLine(report);
        
        // Test 10: Concurrent operations safety
        var concurrentTasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            concurrentTasks.Add(Task.Run(() => 
                performanceMonitoringService.RecordOperation(10 + Random.Shared.Next(100), Random.Shared.Next(2) == 0)
            ));
        }
        Task.WaitAll(concurrentTasks.ToArray());
        metrics = performanceMonitoringService.GetCurrentMetrics();
        Console.WriteLine($"✓ Concurrent operations handled safely: {metrics.TotalOperationsCount} operations recorded");
    }
}
```

## CoreImageProcessingServiceTests

The `CoreImageProcessingServiceTests` class provides unit tests for the `ImageProcessingService` class, validating core image processing operations including image registration, retrieval, and processing capability checks. These tests ensure that the service correctly handles valid and invalid inputs, maintains proper state management, and integrates correctly with the repository layer.

### Usage Example

```csharp
using GpuImageProcessing.Core.Services;
using GpuImageProcessing.Core.Models;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Initialize required services (in real usage these would be injected via DI)
        var imageRepository = new ImageRepository();
        var filterRepository = new GenericRepository<Filter>();
        var transformRepository = new GenericRepository<Transform>();
        var profileRepository = new GenericRepository<ProcessingProfile>();
        var deviceService = new DeviceService();
        var computeShaderPipeline = new ComputeShaderPipeline(...);
        var logger = new Mock<ILogger<ImageProcessingService>>().Object;
        var filterService = new FilterService(filterRepository, logger);
        var transformService = new TransformService(transformRepository, logger);
        
        var processingService = new ImageProcessingService(
            imageRepository,
            filterRepository,
            transformRepository,
            profileRepository,
            deviceService,
            computeShaderPipeline,
            logger,
            filterService,
            transformService
        );

        // Test 1: Register a valid image
        var validImage = await processingService.RegisterImageAsync("test_image.jpg", "Test Image");
        Console.WriteLine($"Registered image: {validImage.Name} with ID: {validImage.Id}");

        // Test 2: Attempt to register an image with empty path (should throw)
        try
        {
            await processingService.RegisterImageAsync("", "Invalid Image");
            Console.WriteLine("ERROR: Should have thrown exception for empty path");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Correctly threw ArgumentException for empty path");
        }

        // Test 3: Retrieve an existing image
        var retrievedImage = await processingService.GetImageAsync(validImage.Id);
        if (retrievedImage != null)
        {
            Console.WriteLine($"Retrieved image: {retrievedImage.Name}");
        }

        // Test 4: Attempt to retrieve non-existing image (should return null)
        var nonExistingImage = await processingService.GetImageAsync(Guid.NewGuid());
        if (nonExistingImage == null)
        {
            Console.WriteLine("Correctly returned null for non-existing image ID");
        }

        // Test 5: Check processing capability with no device selected
        var canProcess = await processingService.CanProcessAsync(new List<Guid>(), Guid.NewGuid());
        Console.WriteLine($"Can process without device: {canProcess}");
    }
}
```

## FilterChainTests

The `FilterChainTests` class provides comprehensive unit tests for the `FilterChain` class, validating core filter chain operations including step management, reordering, validation, querying, cloning, and configuration handling. These tests ensure that filter chains can be safely constructed, modified, and validated for use in GPU image processing pipelines.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;
using System.Collections.Generic;

// Create a filter chain for image processing
var chain = new FilterChain
{
    Name = "Photo Enhancement Pipeline",
    Description = "Standard enhancement pipeline for digital photographs",
    ExecutionOrder = 1,
    AllowParallelExecution = true,
    MaxParallelSteps = 4,
    CacheIntermediateResults = true
};

// Add filter steps to the chain
var blurFilterId = Guid.NewGuid();
var grayscaleFilterId = Guid.NewGuid();
var sharpenFilterId = Guid.NewGuid();

chain.AddStep(blurFilterId);
chain.AddStep(grayscaleFilterId);
chain.AddStep(sharpenFilterId);

Console.WriteLine($"Created chain '{chain.Name}' with {chain.Steps.Count} steps");

// Remove a step if needed
bool removed = chain.RemoveStep(grayscaleFilterId);
Console.WriteLine($"Removed grayscale step: {removed}");

// Reorder remaining steps
chain.ReorderSteps(new List<Guid> { sharpenFilterId, blurFilterId });

// Get only enabled steps
var enabledSteps = chain.GetEnabledSteps();
Console.WriteLine($"Chain has {enabledSteps.Count} enabled steps");

// Clone the chain for reuse with different parameters
var clonedChain = chain.Clone();
clonedChain.Name = "Photo Enhancement Pipeline - High Quality";
Console.WriteLine($"Cloned chain: {clonedChain.Name}");

// Validate filter configurations
var filterConfig = new FilterConfiguration
{
    Name = "Blur",
    FilterType = FilterType.Blur,
    MaxThreadsPerBlock = 256
};
filterConfig.SetParameter("radius", 5.0f);
filterConfig.ParameterTypes["radius"] = "System.Single";

bool isValid = filterConfig.Validate();
Console.WriteLine($"Filter configuration valid: {isValid}");
```

## FilterChainBuilderTests

The `FilterChainBuilderTests` class provides comprehensive unit tests for the `FilterChainBuilder` class, validating its fluent API for constructing filter chains. These tests cover validation scenarios (blank names, missing steps), basic functionality (single and multiple steps), configuration options (descriptions, parallel execution, caching), parameter validation for various filter types (blur radius, sharpen strength, rotation angles, scaling factors, color correction brightness, custom filter IDs), performance estimation, fluent chaining behavior, and CI preset configurations.

### Usage Example

```csharp
using GpuImageProcessing.Domain;
using System;

// Create a simple filter chain with a single grayscale step
var singleStepChain = FilterChainBuilder
    .Create("Grayscale Chain")
    .AddGrayscale()
    .Build();

Console.WriteLine($"Created chain: {singleStepChain.Name}");
Console.WriteLine($"Steps count: {singleStepChain.Steps.Count}");

// Create a complex filter chain with multiple steps and configuration
var complexChain = FilterChainBuilder
    .Create("Photo Enhancement Chain")
    .WithDescription("A comprehensive chain for enhancing digital photographs")
    .WithExecutionOrder(1)
    .AllowParallelExecution(maxParallelSteps: 4)
    .CacheIntermediates()
    .AddGrayscale()
    .AddBlur(radius: 2.5f)
    .AddSharpen(strength: 0.8f)
    .AddEdgeDetection()
    .AddColorCorrection(brightness: 1.1f, contrast: 1.05f)
    .AddThreshold(thresholdValue: 0.7f)
    .AddRotation(angleDegrees: 90)
    .AddScaling(scaleX: 1.5f, scaleY: 1.5f)
    .Build();

Console.WriteLine($"Complex chain: {complexChain.Name}");
Console.WriteLine($"Description: {complexChain.Description}");
Console.WriteLine($"Execution order: {complexChain.ExecutionOrder}");
Console.WriteLine($"Parallel execution: {complexChain.AllowParallelExecution}");
Console.WriteLine($"Max parallel steps: {complexChain.MaxParallelSteps}");
Console.WriteLine($"Cache intermediates: {complexChain.CacheIntermediateResults}");
Console.WriteLine($"Total steps: {complexChain.Steps.Count}");
Console.WriteLine($"Estimated processing time: {complexChain.EstimateTotalProcessingTime()}ms");

// Add a custom filter step
var customFilterId = Guid.NewGuid();
var customChain = FilterChainBuilder
    .Create("Custom Processing Chain")
    .AddCustomFilter(customFilterId, estimatedExecutionMs: 30.0)
    .Build();

Console.WriteLine($"Custom chain steps: {customChain.Steps.Count}");
Console.WriteLine($"Custom filter ID: {customChain.Steps[0].FilterId}");
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

## EndToEndProcessingTests

The `EndToEndProcessingTests` class provides comprehensive integration tests for the complete GPU image processing pipeline. It validates end-to-end workflows including filter creation, application to images, batch processing, configuration validation, performance monitoring, GPU memory management, and error handling across the entire processing chain.

### Usage Example

```csharp
using GpuImageProcessing.Tests.Integration;
using GpuImageProcessing.Domain;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create an instance of the end-to-end processing tests
        var e2eTests = new EndToEndProcessingTests();

        // Test 1: Complete workflow - create filter, apply to image, verify results
        var filterConfig = new FilterConfiguration
        {
            Name = "TestGaussianBlur",
            FilterType = FilterType.Blur,
            IsActive = true,
            Priority = 1
        };
        filterConfig.SetParameter("Radius", 5);

        var testImage = new Image(1920, 1080, 3)
        {
            FileName = "test_image.jpg",
            Format = ImageFormat.Jpeg
        };

        // Run complete workflow test
        await e2eTests.CompleteWorkflow_CreateFilterApplyToImage_RecordsSuccessfully(filterConfig, testImage);
        Console.WriteLine("Complete workflow test passed!");

        // Test 2: Batch processing with multiple images processed concurrently
        var batchImages = new List<Image> {
            new Image(1920, 1080, 3) { FileName = "image1.jpg" },
            new Image(2560, 1440, 3) { FileName = "image2.jpg" },
            new Image(1280, 720, 3) { FileName = "image3.jpg" }
        };

        await e2eTests.BatchProcessing_MultipleImages_ProcessedConcurrently(batchImages, filterConfig);
        Console.WriteLine("Batch processing test passed!");

        // Test 3: Filter configuration validation before processing
        bool isValid = await e2eTests.FilterConfiguration_ValidationWorks_BeforeProcessing(filterConfig);
        Console.WriteLine($"Filter validation test: {(isValid ? "PASSED" : "FAILED")}");

        // Test 4: Image validation with invalid dimensions
        var invalidImage = new Image(0, 0, 3); // Invalid dimensions
        try
        {
            await e2eTests.ImageValidation_InvalidDimensionsRejected_BeforeProcessing(invalidImage);
            Console.WriteLine("Image validation correctly rejected invalid dimensions");
        }
        catch (ValidationException)
        {
            Console.WriteLine("Image validation correctly rejected invalid dimensions");
        }

        // Test 5: Performance monitoring records metrics for all operations
        var metrics = await e2eTests.PerformanceMonitoring_RecordsMetricsForAllOperations(batchImages, filterConfig);
        Console.WriteLine($"Performance metrics recorded: {metrics.TotalOperationsCount} operations");

        // Test 6: Multiple filters applied sequentially in order
        var filterChain = new FilterChain
        {
            Name = "MultiFilterChain"
        };
        filterChain.AddStep(Guid.NewGuid()); // Add first filter
        filterChain.AddStep(Guid.NewGuid()); // Add second filter

        await e2eTests.MultipleFilters_AppliedSequentially_InOrder(filterChain, testImage);
        Console.WriteLine("Multiple filters test passed!");

        // Test 7: GPU memory management - allocate and deallocate are balanced
        var memoryStats = await e2eTests.GpuMemoryManagement_AllocateAndDeallocate_AreBalanced();
        Console.WriteLine($"Memory balanced: Allocated={memoryStats.AllocatedMB}MB, Deallocated={memoryStats.DeallocatedMB}MB");

        // Test 8: Processing statistics calculated correctly after operations
        var stats = await e2eTests.ProcessingStatistics_CalculatedCorrectly_AfterOperations(batchImages, filterConfig);
        Console.WriteLine($"Processing statistics: Success={stats.SuccessCount}, Failed={stats.FailedCount}");

        // Test 9: Filter chain build and execute produces expected output
        var chainResult = await e2eTests.FilterChain_BuildAndExecute_ProducesExpectedOutput(filterChain, testImage);
        Console.WriteLine($"Filter chain result: {chainResult.Status}");

        // Test 10: Image batch with various statuses tracks progress correctly
        var mixedBatch = new ImageBatch
        {
            Name = "MixedStatusBatch"
        };
        mixedBatch.AddImage(Guid.NewGuid());
        mixedBatch.AddImage(Guid.NewGuid());

        await e2eTests.ImageBatch_WithVariousStatuses_TracksProgressCorrectly(mixedBatch);
        Console.WriteLine("Batch progress tracking test passed!");

        // Test 11: Cancel batch processing stops active operations
        var cancellableBatch = new ImageBatch { Name = "CancellableBatch" };
        bool cancellationResult = await e2eTests.CancelBatchProcessing_StopsActiveOperations(cancellableBatch);
        Console.WriteLine($"Batch cancellation: {(cancellationResult ? "SUCCESSFUL" : "FAILED")}");

        // Test 12: Error handling for invalid filter application records failure
        var errorImage = new Image(1920, 1080, 3);
        await e2eTests.ErrorHandling_InvalidFilterApplication_RecordsFailure(filterConfig, errorImage);
        Console.WriteLine("Error handling test passed!");

        // Test 13: Device management selects best GPU for processing
        var bestDevice = await e2eTests.DeviceManagement_SelectsBestGpu_ForProcessing();
        Console.WriteLine($"Best device selected: {bestDevice?.Name ?? "None found"}");

        // Test 14: Result persistence saves processing results
        var resultPath = await e2eTests.ResultPersistence_SavesProcessingResults(testImage, filterConfig);
        Console.WriteLine($"Result saved to: {resultPath}");

        // Test 15: Configuration validation ensures consistency across services
        var configValidation = await e2eTests.ConfigurationValidation_EnsuresConsistency_AcrossServices();
        Console.WriteLine($"Configuration validation: {(configValidation ? "PASSED" : "FAILED")}");
    }
}
```


## MetricsPublisher

The `MetricsPublisher` class provides a centralized mechanism for publishing performance metrics to external monitoring systems. It supports multiple formats including JSON, Prometheus, and InfluxDB, allowing metrics to be sent to various observability platforms. The publisher buffers metrics in memory and automatically flushes them to registered endpoints, ensuring minimal impact on application performance while maintaining reliable metrics delivery.

### Usage Example

```csharp
using GpuImageProcessing.Integration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create a metrics publisher with default buffer size (100 metrics)
        var publisher = new MetricsPublisher();
        
        // Register endpoints for different monitoring systems
        publisher.RegisterEndpoint(
            url: "https://prometheus.example.com/api/v1/write",
            format: MetricsFormat.Prometheus,
            apiKey: "prometheus-secret-key"
        );
        
        publisher.RegisterEndpoint(
            url: "https://influxdb.example.com/api/v2/write",
            format: MetricsFormat.InfluxDb,
            apiKey: "influxdb-secret-token"
        );
        
        // Record basic metrics
        publisher.RecordMetric("gpu_processing_time_ms", 45.2, 
            new Dictionary<string, string> { { "gpu_type", "NVIDIA RTX 4090" } });
        
        publisher.RecordMetric("batch_size", 150, 
            new Dictionary<string, string> { { "batch_name", "summer_photos_2024" } });
        
        // Record timing metrics
        publisher.RecordTiming("image_filtering", TimeSpan.FromMilliseconds(125.7), 
            new Dictionary<string, string> { { "filter_type", "gaussian_blur" } });
        
        // Manually flush metrics (automatically happens when buffer is full)
        await publisher.FlushAsync();
        
        // Dispose to ensure all metrics are flushed
        await publisher.DisposeAsync();
    }
}
```

## BatchProcessingPipelineTests

The `BatchProcessingPipelineTests` class provides comprehensive unit tests for the `BatchProcessingPipeline` class, validating batch image processing pipeline functionality. It tests error handling scenarios (null batches, invalid batches), success scenarios (all images succeed, all images fail, partial failures), progress reporting, retry logic, constructor validation, and output directory creation. Each test uses mock dependencies to isolate the pipeline behavior and verify correct behavior under various conditions.

### Usage Example

```csharp
using GpuImageProcessing.Tests.Pipeline;
using GpuImageProcessing.Domain;
using GpuImageProcessing.Pipeline;
using GpuImageProcessing.Services;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Create the test class instance
        var tests = new BatchProcessingPipelineTests();

        // Test 1: Verify null batch throws ArgumentNullException
        await tests.RunAsync_NullBatch_ThrowsArgumentNullException();
        Console.WriteLine("Test 1 passed: Null batch throws ArgumentNullException");

        // Test 2: Verify invalid batch throws ProcessingException
        await tests.RunAsync_InvalidBatch_ThrowsProcessingException();
        Console.WriteLine("Test 2 passed: Invalid batch throws ProcessingException");

        // Test 3: Verify all images succeed returns full success result
        await tests.RunAsync_AllImagesSucceed_ReturnsFullSuccessResult();
        Console.WriteLine("Test 3 passed: All images succeed returns success result");

        // Test 4: Verify all images fail returns full failure result
        await tests.RunAsync_AllImagesFail_ReturnsFullFailureResult();
        Console.WriteLine("Test 4 passed: All images fail returns failure result");

        // Test 5: Verify partial failure returns correct counts
        await tests.RunAsync_PartialFailure_ReturnsCorrectCounts();
        Console.WriteLine("Test 5 passed: Partial failure returns correct counts");

        // Test 6: Verify progress events are raised for each image
        await tests.RunAsync_RaisesProgressChangedForEachImage();
        Console.WriteLine("Test 6 passed: Progress events raised for each image");

        // Test 7: Verify output directory is created
        await tests.RunAsync_CreatesOutputDirectory();
        Console.WriteLine("Test 7 passed: Output directory is created");

        // Test 8: Verify completed batch has correct status
        await tests.RunAsync_CompletedBatchHasCorrectStatus();
        Console.WriteLine("Test 8 passed: Completed batch has correct status");

        // Test 9: Verify retry logic up to MaxRetries
        await tests.RunAsync_RetriesFailedImageUpToMaxRetries();
        Console.WriteLine("Test 9 passed: Retry logic works correctly");

        // Test 10: Verify constructor throws with null processing service
        tests.Constructor_NullProcessingService_ThrowsArgumentNullException();
        Console.WriteLine("Test 10 passed: Constructor validates processing service");

        // Test 11: Verify constructor throws with null options
        tests.Constructor_NullOptions_ThrowsArgumentNullException();
        Console.WriteLine("Test 11 passed: Constructor validates options");
    }
}
```

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