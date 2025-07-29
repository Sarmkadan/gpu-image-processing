# NotificationService

Centralized service for broadcasting application events to configured channels. It aggregates notifications of different severities and types, tracks their origin, and propagates them to all registered channels asynchronously.

## API

### `public NotificationService(ConsoleNotificationChannel channel)`

Constructs a new `NotificationService` bound to the specified channel. The channel receives every notification sent through this service.

| Parameter | Purpose |
|-----------|---------|
| `channel` | The channel to which notifications will be dispatched. |

### `public void RegisterChannel(ConsoleNotificationChannel channel)`

Adds an additional channel to the list of channels that will receive notifications. Subsequent calls to any `Notify*` method will broadcast to all registered channels.

| Parameter | Purpose |
|-----------|---------|
| `channel` | The channel to register. |

### `public async Task NotifyProcessingCompletedAsync(string title, string message, Dictionary<string, object> details = null)`

Broadcasts a success notification indicating that an image processing operation completed.

| Parameter | Purpose | Required |
|-----------|---------|----------|
| `title` | Short, human-readable summary of the event. | Yes |
| `message` | Detailed description of what succeeded. | Yes |
| `details` | Optional key-value pairs with additional context (e.g., duration, bytes processed). | No |

Returns: A `Task` that completes when the notification has been sent to all registered channels.

### `public async Task NotifyProcessingFailedAsync(string title, string message, Dictionary<string, object> details = null)`

Broadcasts a failure notification indicating that an image processing operation did not complete.

| Parameter | Purpose | Required |
|-----------|---------|----------|
| `title` | Short, human-readable summary of the event. | Yes |
| `message` | Detailed description of the failure. | Yes |
| `details` | Optional key-value pairs with additional context (e.g., error message, stack trace). | No |

Returns: A `Task` that completes when the notification has been sent to all registered channels.

### `public async Task NotifyResourceLimitAsync(string title, string message, Dictionary<string, object> details = null)`

Broadcasts a warning notification indicating that a resource limit (CPU, memory, GPU) has been reached or exceeded.

| Parameter | Purpose | Required |
|-----------|---------|----------|
| `title` | Short, human-readable summary of the event. | Yes |
| `message` | Detailed description of the resource constraint. | Yes |
| `details` | Optional key-value pairs with additional context (e.g., limit value, current usage). | No |

Returns: A `Task` that completes when the notification has been sent to all registered channels.

### `public async Task NotifyHealthStatusAsync(string title, string message, Dictionary<string, object> details = null)`

Broadcasts a health-status notification indicating that a subsystem’s health state has changed.

| Parameter | Purpose | Required |
|-----------|---------|----------|
| `title` | Short, human-readable summary of the event. | Yes |
| `message` | Detailed description of the health change. | Yes |
| `details` | Optional key-value pairs with additional context (e.g., previous state, new state). | No |

Returns: A `Task` that completes when the notification has been sent to all registered channels.

### `public async Task NotifyAsync(NotificationType type, string title, string message, NotificationSeverity severity, Dictionary<string, object> details = null)`

Generic notification broadcaster that allows any combination of type, title, message, severity, and details.

| Parameter | Purpose | Required |
|-----------|---------|----------|
| `type` | Categorizes the notification (e.g., Processing, Health, Resource). | Yes |
| `title` | Short, human-readable summary of the event. | Yes |
| `message` | Detailed description of the event. | Yes |
| `severity` | Indicates the importance of the notification. | Yes |
| `details` | Optional key-value pairs with additional context. | No |

Returns: A `Task` that completes when the notification has been sent to all registered channels.

### `public async Task SendAsync()`

Forces an immediate, synchronous dispatch of all pending notifications to every registered channel. Useful for shutdown or critical-path scenarios where buffered notifications must be delivered before the application exits.

Returns: A `Task` that completes when all pending notifications have been sent.

### `public string Id`

Unique identifier for this instance of the notification service. Generated at construction time.

### `public NotificationType Type`

Gets the type of the last notification sent by any `Notify*` method. Defaults to `NotificationType.None` if no notification has been sent.

### `public string Title`

Gets the title of the last notification sent by any `Notify*` method. `null` if no notification has been sent.

### `public string Message`

Gets the message of the last notification sent by any `Notify*` method. `null` if no notification has been sent.

### `public NotificationSeverity Severity`

Gets the severity of the last notification sent by any `Notify*` method. Defaults to `NotificationSeverity.Info` if no notification has been sent.

### `public DateTime Timestamp`

Gets the timestamp of the last notification sent by any `Notify*` method. `DateTime.MinValue` if no notification has been sent.

### `public Dictionary<string, object> Details`

Gets the details dictionary of the last notification sent by any `Notify*` method. `null` if no notification has been sent or if the last notification had no details.

## Usage

```csharp
// Example 1: Track processing completion
var channel = new ConsoleNotificationChannel();
var service = new NotificationService(channel);

await service.NotifyProcessingCompletedAsync(
    "Processing finished",
    "Image processed successfully in 125 ms",
    new Dictionary<string, object> { ["durationMs"] = 125 }
);

// Example 2: Track resource limits
await service.NotifyResourceLimitAsync(
    "Memory threshold exceeded",
    "GPU memory usage reached 95% of limit",
    new Dictionary<string, object> {
        ["limitMB"] = 4096,
        ["currentMB"] = 3900
    }
);
```

## Notes

- **Thread safety**: All public members are safe to call concurrently from multiple threads. Internally, the service uses a lock to guard the list of channels and the last-notification state.
- **Channel registration**: Channels may be added or removed at any time; notifications already in flight are unaffected.
- **Last-notification state**: The `Id`, `Type`, `Title`, `Message`, `Severity`, `Timestamp`, and `Details` properties reflect the most recent notification sent via any `Notify*` method. They are updated atomically under the same lock used for channel management, so reads are consistent.
- **SendAsync**: Calling `SendAsync` while another `SendAsync` is in progress results in a deadlock; avoid overlapping calls.
- **Error handling**: If a channel throws during `SendAsync`, the exception is caught and logged (implementation detail), but subsequent channels continue to receive the notification.
