# EventAggregatorExtensions

EventAggregatorExtensions provides a set of static extension methods designed to simplify the interaction with an `IEventAggregator` implementation, enabling decoupled, type-safe messaging within the application. These extensions facilitate subscribing to, unsubscribing from, and publishing messages, providing both synchronous and asynchronous support alongside robust error handling during the publication process.

## API

### Subscribe<T>
Subscribes a synchronous handler to messages of type `T`.
- **Parameters**: `IEventAggregator` (the aggregator instance), `Action<T>` (the handler to execute when a message of type `T` is published).
- **Returns**: `IDisposable` (a subscription token that removes the handler when disposed).

### SubscribeAsync<T>
Subscribes an asynchronous handler to messages of type `T`.
- **Parameters**: `IEventAggregator` (the aggregator instance), `Func<T, Task>` (the asynchronous handler to execute when a message of type `T` is published).
- **Returns**: `IDisposable` (a subscription token that removes the handler when disposed).

### PublishAsync
Publishes a message asynchronously to all registered handlers of the message's type.
- **Parameters**: `IEventAggregator` (the aggregator instance), `object` (the message instance to publish).
- **Returns**: `Task` (a task that completes when all registered handlers for the message type have finished processing).

### PublishWithErrorCapture
Publishes a message synchronously to all registered handlers of the message's type and aggregates any exceptions thrown by those handlers.
- **Parameters**: `IEventAggregator` (the aggregator instance), `object` (the message instance to publish).
- **Returns**: `List<Exception>` (a collection of exceptions thrown during the publication process; returns an empty list if all handlers execute successfully).

## Usage

```csharp
// Example 1: Synchronous subscription and unsubscription
var subscription = aggregator.Subscribe<ImageLoadedEvent>(evt => {
    Console.WriteLine($"Image loaded: {evt.FilePath}");
});
// ... later
subscription.Dispose();

// Example 2: Asynchronous publication with error handling
try {
    await aggregator.PublishAsync(new ProcessImageCommand(data));
} catch (Exception ex) {
    // Handles exceptions that occur within the publication mechanism itself
    logger.LogError(ex, "Critical failure during publication");
}
```

## Notes

- **Thread-Safety**: These extension methods rely entirely on the thread-safety guarantees of the underlying `IEventAggregator` implementation for managing subscriber lists and dispatching messages.
- **Execution Order**: The order in which multiple handlers are invoked is not guaranteed.
- **Exception Handling**: 
    - `PublishWithErrorCapture` is designed to collect exceptions from multiple handlers to ensure all handlers are given the opportunity to process the message, even if one throws an exception.
    - `PublishAsync` behavior regarding exceptions depends on the underlying `IEventAggregator` implementation; typically, unhandled exceptions in asynchronous handlers may fault the returned task.
- **Asynchronous Execution**: When using `SubscribeAsync`, ensure the underlying aggregator implementation properly awaits the returned `Task` to avoid race conditions or premature application shutdown.
