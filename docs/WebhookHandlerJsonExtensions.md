# WebhookHandlerJsonExtensions

Provides JSON serialization and deserialization extensions for `WebhookHandler` objects, along with access to aggregated webhook subscription counts and the underlying subscription list. This type serves as the bridge between the `WebhookHandler` domain model and its JSON representation, supporting both lossless round-tripping and fault-tolerant parsing.

## API

### `ToJson`

```csharp
public static string ToJson(this WebhookHandler handler)
```

Serializes a `WebhookHandler` instance to its JSON string representation.

- **Parameters**: `handler` — the `WebhookHandler` to serialize. Must not be null.
- **Returns**: A non-null, non-empty JSON string.
- **Throws**: `ArgumentNullException` when `handler` is null. May throw `JsonException` if the object graph contains values that cannot be serialized.

### `FromJson`

```csharp
public static WebhookHandler? FromJson(this string json)
```

Deserializes a JSON string into a `WebhookHandler` instance.

- **Parameters**: `json` — a JSON string previously produced by `ToJson` or structurally equivalent.
- **Returns**: A populated `WebhookHandler` if parsing succeeds; `null` if `json` is null, empty, or whitespace.
- **Throws**: `JsonException` when the input is structurally invalid or contains fields that cannot be mapped to the `WebhookHandler` model. Does not throw for null/empty input.

### `TryFromJson`

```csharp
public static bool TryFromJson(this string json, out WebhookHandler? result)
```

Attempts to deserialize a JSON string without throwing on malformed input.

- **Parameters**: `json` — the JSON string to parse; `result` — on success, receives the deserialized `WebhookHandler`; on failure, receives `null`.
- **Returns**: `true` if deserialization succeeded and `result` is non-null; `false` if `json` is null/empty or the JSON is structurally invalid.
- **Throws**: Never throws. All exceptions are caught internally and translated to a `false` return.

### `ActiveWebhookCount`

```csharp
public int ActiveWebhookCount { get; }
```

Gets the number of webhook subscriptions currently in an active state. The definition of "active" is determined by the `WebhookSubscription` state model (e.g., not expired, not paused). This property reflects a point-in-time count derived from `Subscriptions`.

### `TotalWebhookCount`

```csharp
public int TotalWebhookCount { get; }
```

Gets the total number of webhook subscriptions regardless of their state. This includes active, paused, expired, and any other lifecycle states present in `Subscriptions`.

### `Subscriptions`

```csharp
public List<WebhookSubscription>? Subscriptions { get; }
```

The underlying list of webhook subscriptions. May be `null` if no subscriptions have been registered. Modifications to this list directly affect the values returned by `ActiveWebhookCount` and `TotalWebhookCount`.

## Usage

### Example 1: Round-trip serialization and deserialization

```csharp
WebhookHandler handler = new WebhookHandler();
// ... populate handler with subscriptions ...

// Serialize to JSON for storage or transmission
string json = handler.ToJson();

// Later: reconstruct the handler from JSON
WebhookHandler? restored = json.FromJson();
if (restored != null)
{
    Console.WriteLine($"Restored {restored.TotalWebhookCount} subscriptions, {restored.ActiveWebhookCount} active.");
}
```

### Example 2: Safe parsing with TryFromJson

```csharp
string incomingJson = GetJsonFromExternalSource(); // may be malformed

if (incomingJson.TryFromJson(out WebhookHandler? handler) && handler != null)
{
    // Inspect subscription state without risk of exceptions
    if (handler.ActiveWebhookCount > 0)
    {
        foreach (var sub in handler.Subscriptions ?? Enumerable.Empty<WebhookSubscription>())
        {
            Console.WriteLine($"Subscription {sub.Id} is active.");
        }
    }
}
else
{
    Console.WriteLine("Invalid or empty JSON payload; skipping webhook processing.");
}
```

## Notes

- **Null handling**: `FromJson` and `TryFromJson` treat null, empty, and whitespace strings as absence of data, returning `null` or `false` respectively. `ToJson` does not accept null input and will throw immediately.
- **Mutable subscription list**: `Subscriptions` returns a direct reference to the internal list. External code can add or remove items, which immediately changes `ActiveWebhookCount` and `TotalWebhookCount`. No snapshot or defensive copy is made.
- **Thread safety**: None of the members are inherently thread-safe. Concurrent reads of `ActiveWebhookCount` and `TotalWebhookCount` while another thread modifies `Subscriptions` may yield inconsistent or stale values. Synchronization is the caller's responsibility.
- **JSON exceptions**: `FromJson` surfaces `JsonException` for structural problems (missing required fields, type mismatches). `TryFromJson` swallows all such exceptions. If detailed error diagnostics are needed, prefer `FromJson` wrapped in a try-catch block over `TryFromJson`.
- **Active count semantics**: `ActiveWebhookCount` is a computed property with no setter. Its value depends entirely on the state flags of items within `Subscriptions` at the time of access. There is no caching or deferred evaluation.
