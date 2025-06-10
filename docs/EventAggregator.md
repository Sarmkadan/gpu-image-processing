# EventAggregator

A lightweight in-process event bus that enables loosely-coupled communication between components by publishing and subscribing to strongly-typed events. It supports both synchronous and asynchronous event handling, with built-in subscription tracking and statistics.

## API

### `public EventAggregator`
Initializes a new instance of the event aggregator with a unique identifier.

### `public IDisposable Subscribe<T>(Action<DomainEvent> handler)`
Subscribes a synchronous handler to events of type `T`.
- **Parameters**:
  - `handler`: The synchronous callback invoked when an event of type `T` is published.
- **Return value**: An `IDisposable` that unsubscribes the handler when disposed.
- **Throws**: `ArgumentNullException` if `handler` is `null`.

### `public IDisposable SubscribeAsync<T>(Func<DomainEvent, Task> handler)`
Subscribes an asynchronous handler to events of type `T`.
- **Parameters**:
  - `handler`: The asynchronous callback invoked when an event of type `T` is published.
- **Return value**: An `IDisposable` that unsubscribes the handler when disposed.
- **Throws**: `ArgumentNullException` if `handler` is `null`.

### `public async Task PublishAsync(DomainEvent event)`
Asynchronously publishes an event to all subscribers.
- **Parameters**:
  - `event`: The event to publish.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `public void Publish(DomainEvent event)`
Synchronously publishes an event to all subscribers.
- **Parameters**:
  - `event`: The event to publish.
- **Throws**: `ArgumentNullException` if `event` is `null`.

### `public SubscriptionStats GetStats()`
Returns aggregated statistics about current subscriptions.
- **Return value**: A `SubscriptionStats` object containing counts of event types and total subscriptions.

### `public void Dispose()`
Releases all subscriptions and cleans up resources.

### `public Guid Id`
Gets the unique identifier of the event aggregator instance.

## Usage

### Basic Synchronous Subscription
