# WebhookHandlerExtensions

Utility class providing static methods to query and inspect registered webhook subscriptions in the GPU image-processing pipeline. These methods expose internal tracking state without exposing the underlying collection directly, enabling callers to inspect active subscriptions, filter by event type or target URL, and retrieve oldest or most-recent active hooks.

## API

### `GetActiveWebhookCount()`
Returns the number of webhook subscriptions currently marked as active.
- **Parameters:** None.
- **Return value:** `int` – the count of active subscriptions.
- **Exceptions:** None.

### `GetWebhooksByEventType(string eventType)`
Retrieves all webhook subscriptions that match the specified event type.
- **Parameters:**
  - `eventType` – the event type string to filter by (case-sensitive).
- **Return value:** `List<WebhookSubscription>` – a new list containing matching subscriptions; empty if none match.
- **Exceptions:** Throws `ArgumentNullException` if `eventType` is `null`.

### `GetWebhooksByUrl(string url)`
Retrieves all webhook subscriptions whose target URL matches the provided string.
- **Parameters:**
  - `url` – the target URL string to filter by (case-sensitive).
- **Return value:** `List<WebhookSubscription>` – a new list containing matching subscriptions; empty if none match.
- **Exceptions:** Throws `ArgumentNullException` if `url` is `null`.

### `IsWebhookActive(string id)`
Determines whether a subscription with the given identifier is currently active.
- **Parameters:**
  - `id` – the unique identifier of the subscription.
- **Return value:** `bool` – `true` if the subscription exists and is active; otherwise `false`.
- **Exceptions:** None.

### `GetOldestActiveWebhook()`
Returns the oldest active webhook subscription based on registration time.
- **Parameters:** None.
- **Return value:** `WebhookSubscription?` – the oldest active subscription, or `null` if none are active.
- **Exceptions:** None.

### `GetMostRecentWebhook()`
Returns the most recently registered webhook subscription, regardless of its active status.
- **Parameters:** None.
- **Return value:** `WebhookSubscription?` – the most recent subscription, or `null` if none exist.
- **Exceptions:** None.

## Usage
