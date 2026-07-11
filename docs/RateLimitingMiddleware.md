# RateLimitingMiddleware

Middleware component that enforces rate limiting using a token bucket algorithm to control API request throughput. It tracks token consumption across concurrent operations and enforces maximum request rates by delaying or rejecting requests when tokens are unavailable. Designed for high-throughput GPU image processing pipelines where resource contention must be managed.

## API

### `RateLimitingMiddleware`
Constructor that initializes the rate limiter with default configuration values. Creates a token bucket with `MaxTokens` capacity, `RefillRate` replenishment rate, and `MaxConcurrentOperations` concurrency limit.

### `string GetName()`
Returns the name identifier of the middleware instance. Used for logging and pipeline diagnostics.

**Returns**
`string` – The middleware name.

### `int GetPriority()`
Returns the execution priority of the middleware within the processing pipeline. Higher values execute earlier.

**Returns**
`int` – The priority value.

### `async Task<MiddlewareResult> ExecuteAsync(RequestMiddlewareContext context)`
Processes a request through the rate limiter. Consumes tokens according to `TokenCost`, enforces concurrency limits, and returns a result indicating whether the request was allowed, delayed, or rejected.

**Parameters**
- `context` – The request context containing the operation to be rate-limited.

**Returns**
`Task<MiddlewareResult>` – A task that resolves to a `MiddlewareResult` indicating success, delay, or rejection.

**Throws**
`ArgumentNullException` – If `context` is null.

### `RateLimitStatus GetStatus()`
Retrieves the current operational status of the rate limiter, including token availability and concurrency metrics.

**Returns**
`RateLimitStatus` – An object containing `AvailableTokens`, `ActiveOperations`, and `MaxConcurrentOperations`.

### `int MaxTokens`
Gets or sets the maximum number of tokens the bucket can hold. Determines the burst capacity of the rate limiter.

### `int RefillRate`
Gets or sets the number of tokens added to the bucket per second. Controls the sustained request rate.

### `int TokenCost`
Gets or sets the number of tokens consumed by each request. Higher values reduce throughput for expensive operations.

### `int MaxConcurrentOperations`
Gets or sets the maximum number of concurrent operations allowed. Enforces parallelism limits.

### `int RetryAfterSeconds`
Gets or sets the delay (in seconds) to suggest to clients when rate-limited. Used in response headers.

### `TokenBucket`
Gets the internal token bucket state, including current token count and last refill timestamp. Exposed for monitoring and diagnostics.

### `bool TryConsume(int tokens)`
Attempts to consume a specified number of tokens from the bucket. Returns true if successful, false if insufficient tokens are available.

**Parameters**
- `tokens` – The number of tokens to consume.

**Returns**
`bool` – True if tokens were consumed; false otherwise.

**Throws**
`ArgumentOutOfRangeException` – If `tokens` is less than or equal to zero.

### `int AvailableTokens`
Gets the current number of available tokens in the bucket.

### `double UtilizationPercent`
Gets the current utilization percentage of the token bucket (0.0 to 100.0). Calculated as `(MaxTokens - AvailableTokens) / MaxTokens * 100`.

### `int ActiveOperations`
Gets the current number of active operations using tokens.

## Usage

```csharp
// Example 1: Basic rate-limited GPU image processing
var rateLimiter = new RateLimitingMiddleware
{
    MaxTokens = 10,
    RefillRate = 2,
    TokenCost = 1,
    MaxConcurrentOperations = 5
};

var context = new RequestMiddlewareContext
{
    Operation = "gpu-process",
    PayloadSize = 1024
};

var result = await rateLimiter.ExecuteAsync(context);

if (result.Status == MiddlewareStatus.Allowed)
{
    await ProcessImageWithGpuAsync(context.Payload);
}
```

```csharp
// Example 2: Dynamic rate limiting with feedback
var limiter = new RateLimitingMiddleware
{
    MaxTokens = 20,
    RefillRate = 5,
    TokenCost = 2,
    MaxConcurrentOperations = 8,
    RetryAfterSeconds = 5
};

if (limiter.TryConsume(limiter.TokenCost))
{
    var status = limiter.GetStatus();
    Console.WriteLine($"Rate: {status.UtilizationPercent:F1}% | Active: {status.ActiveOperations}");
    await ExecuteLimitedOperationAsync();
}
else
{
    var retryAfter = limiter.RetryAfterSeconds;
    Console.WriteLine($"Rate-limited. Retry after {retryAfter}s.");
}
```

## Notes

Token consumption and refill operations are thread-safe due to internal locking. The `TokenBucket` state is updated atomically, and `TryConsume` checks availability under lock to prevent race conditions. However, high contention scenarios may cause brief delays during peak throughput.

When `MaxConcurrentOperations` is reached, new requests are rejected immediately without token deduction. This prevents resource exhaustion but may lead to cascading rejections under sustained load spikes.

The `UtilizationPercent` calculation assumes `MaxTokens` is positive. If set to zero, the value will be undefined. Always validate configuration before use.

Token refill occurs on each `TryConsume` or `ExecuteAsync` call, ensuring fairness across concurrent threads. This design prevents starvation but may introduce slight latency variance under high load.
