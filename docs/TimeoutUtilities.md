# TimeoutUtilities

Utility class providing helpers for time-bound operations, cancellation, and retry logic in asynchronous workflows. Designed to simplify handling of timeouts, retries, and condition waits with consistent cancellation support and bounded timeouts.

## API

### `public static async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, CancellationToken cancellationToken = default)`

Executes the provided asynchronous operation with a strict timeout. If the operation does not complete within the specified timeout, it is canceled and a `TimeoutException` is thrown.

- **operation**: The asynchronous function to execute.
- **timeout**: Maximum allowed duration for the operation.
- **cancellationToken**: Optional token to observe for early cancellation.
- **Return value**: The result of the operation if completed in time.
- **Throws**: `TimeoutException` if the operation exceeds the timeout; `OperationCanceledException` if canceled via token.

---

### `public static async Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, CancellationToken cancellationToken = default)`

Executes the provided asynchronous operation with a strict timeout. Similar to the generic version but for void operations.

- **operation**: The asynchronous action to execute.
- **timeout**: Maximum allowed duration for the operation.
- **cancellationToken**: Optional token to observe for early cancellation.
- **Throws**: `TimeoutException` if the operation exceeds the timeout; `OperationCanceledException` if canceled via token.

---

### `public static async Task<T> RetryWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, int maxRetries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)`

Executes the provided asynchronous operation with retry logic and a bounded timeout. Retries are attempted on transient failures (non-cancellation exceptions) up to `maxRetries` times, with optional delay between attempts. The total time including retries is bounded by `timeout`.

- **operation**: The asynchronous function to execute.
- **timeout**: Maximum total time allowed for all attempts combined.
- **maxRetries**: Maximum number of retry attempts (default: 3).
- **retryDelay**: Optional delay between retries (default: no delay).
- **cancellationToken**: Optional token to observe for early cancellation.
- **Return value**: The result of the first successful operation.
- **Throws**: `TimeoutException` if total time exceeds `timeout`; `OperationCanceledException` if canceled via token; original exception if all retries fail.

---

### `public static async Task<bool> WaitForConditionAsync(Func<bool> condition, TimeSpan timeout, TimeSpan? pollingInterval = null, CancellationToken cancellationToken = default)`

Waits asynchronously until the condition function returns `true`, or until the timeout is reached. Polls at regular intervals.

- **condition**: Function returning `true` when the desired state is reached.
- **timeout**: Maximum time to wait for the condition.
- **pollingInterval**: Optional interval between checks (default: 100ms).
- **cancellationToken**: Optional token to observe for early cancellation.
- **Return value**: `true` if condition becomes true within timeout; `false` otherwise.
- **Throws**: `OperationCanceledException` if canceled via token.

---
### `public static TimeSpan GetBoundedTimeout(TimeSpan timeout, TimeSpan? maxTimeout = null)`

Returns a timeout value bounded by an optional maximum. Useful to prevent excessively long timeouts in configurable systems.

- **timeout**: The requested timeout duration.
- **maxTimeout**: Optional upper bound for the timeout.
- **Return value**: The smaller of `timeout` or `maxTimeout`; if `maxTimeout` is `null`, returns `timeout`.

---
### `public static CancellationToken CreateLinkedToken(CancellationToken primary, TimeSpan timeout)`

Creates a new `CancellationToken` that is linked to the primary token and will be canceled when the specified timeout elapses.

- **primary**: The primary token to link to.
- **timeout**: The timeout duration after which the returned token is canceled.
- **Return value**: A linked cancellation token that combines both sources.

---
### `public static string FormatTimeout(TimeSpan timeout)`

Formats a `TimeSpan` representing a timeout into a human-readable string (e.g., "5s", "2m30s").

- **timeout**: The timeout to format.
- **Return value**: A compact, culture-invariant string representation.

## Usage

### Example 1: Executing a network call with timeout
