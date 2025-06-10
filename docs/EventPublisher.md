# EventPublisher

The `EventPublisher` class provides a generic, type‑based event system that decouples event senders from receivers. It allows subscribing handlers (both synchronous and asynchronous) to specific event types, publishing events asynchronously, and managing subscriptions programmatically. This is useful for in‑process messaging where components need to react to events without direct references to each other.

## API

### `public EventPublisher()`

Initializes a new instance of the `EventPublisher` class.

---

### `public void Subscribe<T>(Action<T> handler)`

Subscribes a synchronous handler for events of type `T`.

- **Type parameter `T`**: The event data type.
- **Parameter `handler`**: A delegate that will be invoked when an event of type `T` is published.
- **Throws**: `ArgumentNullException` if `handler` is `null`.

---

### `public void Subscribe<T>(Func<T, Task> asyncHandler)`

Subscribes an asynchronous handler for events of type `T`.

- **Type parameter `T`**: The event data type.
- **Parameter `asyncHandler`**: A delegate that returns a `Task` and will be awaited when an event of type `T` is published.
- **Throws**: `ArgumentNullException` if `asyncHandler` is `null`.

---

### `public async Task PublishAsync<T>(T eventData)`

Publishes an event of type `T`, invoking all subscribed handlers (both synchronous and asynchronous) in the order they were added. Synchronous handlers are executed first, then asynchronous handlers are awaited.

- **Type parameter `T`**: The event data type.
- **Parameter `eventData`**: The event payload to pass to each handler.
- **Returns**: A `Task` that completes when all handlers have finished execution.
- **Throws**: `ArgumentNullException` if `eventData` is `null`.

---

### `public bool Unsubscribe<T>(Action<T> handler)`

Removes a previously subscribed synchronous handler for events of type `T`.

- **Type parameter `T`**: The event data type.
- **Parameter `handler`**: The handler to remove.
- **Returns**: `true` if the handler was found and removed; `false` otherwise.
- **Throws**: `ArgumentNullException` if `handler` is `null`.

---

### `public bool Unsubscribe<T>(Func<T, Task> asyncHandler)`

Removes a previously subscribed asynchronous handler for events of type `T`.

- **Type parameter `T`**: The event data type.
- **Parameter `asyncHandler`**: The async handler to remove.
- **Returns**: `true` if the handler was found and removed; `false` otherwise.
- **Throws**: `ArgumentNullException` if `asyncHandler` is `null`.

---

### `public void ClearSubscribers()`

Removes all subscribers for all event types. After calling this method, no handlers remain registered.

---

### `public int GetSubscriberCount()`

Returns the total number of subscribers across all event types.

- **Returns**: The total count of registered handlers (both synchronous and asynchronous).

---

### `public IReadOnlyList<string> GetEventTypes()`

Returns a read‑only list of the event type names (full type names) for which at least one subscriber is registered.

- **Returns**: A list of strings representing the event types that have active subscriptions.

## Usage

### Example 1: Basic synchronous subscription and publishing

```csharp
using System;
using System.Threading.Tasks;

public class OrderPlacedEvent
{
    public int OrderId { get; set; }
}

public class OrderService
{
    private readonly EventPublisher _publisher = new EventPublisher();

    public void SubscribeToOrders()
    {
        _publisher.Subscribe<OrderPlacedEvent>(OnOrderPlaced);
    }

    private void OnOrderPlaced(OrderPlacedEvent e)
    {
        Console.WriteLine($"Order {e.OrderId} placed.");
    }

    public async Task PlaceOrderAsync(int orderId)
    {
        // ... business logic ...
        await _publisher.PublishAsync(new OrderPlacedEvent { OrderId = orderId });
    }
}

// Usage
var service = new OrderService();
service.SubscribeToOrders();
await service.PlaceOrderAsync(42);
// Output: Order 42 placed.
```

### Example 2: Async handlers and unsubscribing

```csharp
using System;
using System.Threading.Tasks;

public class UserLoggedInEvent
{
    public string UserName { get; set; }
}

public class AuditLogger
{
    private readonly EventPublisher _publisher;

    public AuditLogger(EventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task LogLoginAsync(UserLoggedInEvent e)
    {
        await Task.Delay(100); // simulate async I/O
        Console.WriteLine($"Audit: {e.UserName} logged in.");
    }

    public void StartAuditing()
    {
        _publisher.Subscribe<UserLoggedInEvent>(LogLoginAsync);
    }

    public void StopAuditing()
    {
        _publisher.Unsubscribe<UserLoggedInEvent>(LogLoginAsync);
    }
}

// Usage
var publisher = new EventPublisher();
var audit = new AuditLogger(publisher);
audit.StartAuditing();

await publisher.PublishAsync(new UserLoggedInEvent { UserName = "alice" });
// Output: Audit: alice logged in.

audit.StopAuditing();
await publisher.PublishAsync(new UserLoggedInEvent { UserName = "bob" });
// No output – handler was removed.
```

## Notes

- **Duplicate subscriptions**: Subscribing the same handler instance multiple times will cause it to be invoked multiple times per event. To avoid this, ensure handlers are subscribed only once, or use `Unsubscribe` before re‑subscribing.
- **Unsubscribing a non‑existent handler**: `Unsubscribe` returns `false` without throwing an exception.
- **Publishing with no subscribers**: `PublishAsync` completes immediately without error when no handlers are registered for the given event type.
- **Handler execution order**: Synchronous handlers are invoked in the order they were added, followed by asynchronous handlers in the order they were added. Asynchronous handlers are awaited sequentially, not concurrently.
- **Thread safety**: The `EventPublisher` class is **not thread‑safe**. Concurrent calls to `Subscribe`, `Unsubscribe`, `ClearSubscribers`, or `PublishAsync` from multiple threads may cause race conditions or undefined behavior. If concurrent access is required, external synchronization (e.g., a lock) must be used.
- **Handler exceptions**: If a synchronous handler throws an exception, the exception propagates immediately from `PublishAsync`, and subsequent handlers are not invoked. For asynchronous handlers, an exception thrown inside the handler will cause the returned `Task` to fault, and `PublishAsync` will propagate that exception. Consider wrapping handler invocations in try‑catch blocks if fault tolerance is needed.
