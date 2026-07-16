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

`EventPublisher` provides a lightweight, in‑process publish‑subscribe mechanism for processing events.  
It lets you register asynchronous or synchronous handlers for a named event type, publish events to all registered handlers, and manage subscriptions (unsubscribe, clear, query counts, and list event types).

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

// existing content ...

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
            async ev => {
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
