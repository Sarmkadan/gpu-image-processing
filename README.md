// existing content ...

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