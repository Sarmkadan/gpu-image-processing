# WebhookHandler

The `WebhookHandler` class provides a robust mechanism for managing, dispatching, and monitoring webhook subscriptions within the `gpu-image-processing` pipeline. It encapsulates the lifecycle of a specific webhook endpoint, handling registration, event dispatching with configurable retry policies, and state tracking for failure management. This component ensures reliable asynchronous notification delivery for image processing events while maintaining internal state regarding activity status, retry counts, and timing configurations.

## API

### Constructors

#### `public WebhookHandler`
Initializes a new instance of the `WebhookHandler` class. This constructor sets up the internal state required for tracking webhook activity, initializing counters such as `FailureCount` to zero, and preparing the retry policy configuration based on default or injected settings.

### Methods

#### `public string RegisterWebhook`
Registers the current handler instance with the central webhook management system.
*   **Return Value**: Returns a unique string identifier confirming the successful registration of the webhook endpoint.
*   **Exceptions**: May throw an exception if the `WebhookUrl` is invalid, the endpoint is unreachable during validation, or if a duplicate registration is attempted for the same context.

#### `public bool UnregisterWebhook`
Removes the current webhook subscription from the active listener list.
*   **Return Value**: Returns `true` if the webhook was successfully found and removed; returns `false` if the webhook was not previously registered or has already been removed.
*   **Exceptions**: Generally does not throw exceptions; failures are indicated via the boolean return value.

#### `public async Task DispatchEventAsync<T>`
Asynchronously sends an event payload to the configured `WebhookUrl`.
*   **Parameters**:
    *   `T`: The generic type representing the event data payload.
*   **Return Value**: Returns a `Task` that completes when the dispatch attempt (including any configured retries) finishes.
*   **Exceptions**: Throws an exception if the event fails to serialize, if the HTTP transport fails after exhausting the `MaxRetries` limit defined in the `RetryPolicy`, or if the handler is marked as inactive (`IsActive` is false).

#### `public List<WebhookSubscription> GetWebhooks`
Retrieves a list of all active webhook subscriptions currently managed by this handler or its associated context.
*   **Return Value**: A `List<WebhookSubscription>` containing the details of registered subscriptions.
*   **Exceptions**: Does not typically throw exceptions unless the internal subscription store is corrupted or inaccessible.

#### `public void Dispose`
Releases unmanaged resources and performs cleanup operations associated with the `WebhookHandler`. This includes canceling any pending retry tasks and deregistering the webhook if it is currently active.
*   **Exceptions**: Should be implemented to suppress exceptions during disposal to ensure application stability during shutdown.

### Properties

#### `public string Id`
Gets the unique identifier assigned to this specific webhook handler instance. This ID is used for logging, tracking, and management operations.

#### `public string WebhookUrl`
Gets or sets the target Uniform Resource Locator (URL) where events will be dispatched. This must be a valid HTTP or HTTPS endpoint.

#### `public string EventType`
Gets or sets the specific type of event (e.g., "ImageProcessed", "JobFailed") that this handler is subscribed to. Dispatching logic often filters events based on this property.

#### `public DateTime CreatedAt`
Gets the timestamp indicating when this `WebhookHandler` instance was created and initialized.

#### `public bool IsActive`
Gets or sets a boolean value indicating whether the webhook is currently enabled. If `false`, `DispatchEventAsync<T>` will short-circuit and not attempt to send data.

#### `public int FailureCount`
Gets the current number of consecutive failed delivery attempts. This counter is typically reset upon a successful dispatch and incremented upon failure, influencing the `IsActive` status if a threshold is exceeded.

#### `public WebhookRetryPolicy RetryPolicy`
Gets or sets the object defining the strategy for retrying failed requests. This object encapsulates the logic for backoff calculations and retry conditions.

#### `public int MaxRetries`
Gets or sets the maximum number of retry attempts the handler will make for a single failed event dispatch before marking the operation as permanently failed.

#### `public int InitialDelayMs`
Gets or sets the initial delay in milliseconds before the first retry attempt is made following a failure.

#### `public int MaxDelayMs`
Gets or sets the upper limit in milliseconds for the delay between retry attempts, preventing excessive wait times during exponential backoff.

#### `public double BackoffMultiplier`
Gets or sets the multiplier applied to the delay duration after each failed attempt. This value drives the exponential backoff algorithm (e.g., a value of 2.0 doubles the wait time per retry).

## Usage

### Example 1: Initializing and Registering a Webhook
This example demonstrates how to instantiate a `WebhookHandler`, configure its target URL and event type, and register it to start receiving notifications.

```csharp
using GpuImageProcessing.Webhooks;

// Initialize the handler
var handler = new WebhookHandler
{
    WebhookUrl = "https://api.example.com/notifications/image-complete",
    EventType = "ImageProcessingCompleted",
    MaxRetries = 3,
    InitialDelayMs = 1000,
    BackoffMultiplier = 2.0,
    MaxDelayMs = 30000
};

// Register the webhook and capture the confirmation ID
try 
{
    string registrationId = handler.RegisterWebhook();
    Console.WriteLine($"Webhook registered with ID: {registrationId}");
}
catch (Exception ex)
{
    Console.WriteLine($"Registration failed: {ex.Message}");
}
```

### Example 2: Dispatching an Event with Error Handling
This example shows how to dispatch a typed event payload asynchronously, utilizing the built-in retry policy and checking the handler's active status.

```csharp
using GpuImageProcessing.Webhooks;
using GpuImageProcessing.Events;

public async Task NotifyCompletion(WebhookHandler handler, ImageResult result)
{
    if (!handler.IsActive)
    {
        Console.WriteLine("Webhook is inactive; skipping dispatch.");
        return;
    }

    try 
    {
        // Dispatch the event with generic type safety
        await handler.DispatchEventAsync<ImageResult>(result);
        
        // Reset failure count on success (if not handled internally)
        Console.WriteLine($"Event dispatched successfully. Current failures: {handler.FailureCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Event dispatch failed after retries: {ex.Message}");
        Console.WriteLine($"Total consecutive failures: {handler.FailureCount}");
        
        // Optional: Deactivate handler if failures exceed critical threshold
        if (handler.FailureCount >= handler.MaxRetries)
        {
            handler.IsActive = false;
            Console.WriteLine("Handler deactivated due to excessive failures.");
        }
    }
}
```

## Notes

*   **Thread Safety**: While the property getters and setters are standard C# accessors, the `FailureCount` and `IsActive` properties are modified during asynchronous operations (`DispatchEventAsync<T>`). In high-concurrency scenarios where multiple threads might trigger dispatches or modify configuration simultaneously, external synchronization (e.g., `lock` statements) may be required to prevent race conditions on state updates.
*   **Retry Logic Overflow**: When configuring `InitialDelayMs`, `MaxDelayMs`, and `BackoffMultiplier`, ensure that the calculated delay does not exceed integer limits or result in logical inconsistencies where the initial delay is greater than the maximum delay. The handler assumes valid numeric ranges for these properties.
*   **Disposal Behavior**: Calling `Dispose` renders the instance unusable for further `DispatchEventAsync<T>` calls. Attempting to dispatch events after disposal may result in `ObjectDisposedException` or silent failures depending on the internal implementation of the underlying HTTP client.
*   **Failure Count Persistence**: The `FailureCount` property tracks consecutive failures in memory. If the application restarts, this counter resets to zero unless explicitly persisted to an external store by the consuming application.
*   **Generic Type Constraints**: The `DispatchEventAsync<T>` method accepts any type `T`. It is the responsibility of the caller to ensure that `T` is serializable to the format expected by the remote `WebhookUrl` (typically JSON); otherwise, serialization exceptions will occur during dispatch.
