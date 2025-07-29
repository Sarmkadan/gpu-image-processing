# AsyncTaskQueue

A lightweight, priority-aware queue for managing asynchronous tasks with cancellation support, designed for coordinating GPU image processing workloads where task ordering and resource constraints matter. The queue maintains strict FIFO ordering within priority levels and provides detailed introspection into task lifecycle.

## API

### `public AsyncTaskQueue(Func<CancellationToken, Task> action, string name = null, int priority = 0)`

Initializes a new task queue with a shared action delegate executed by all enqueued tasks.

- **Parameters**:
  - `action`: The async delegate invoked for each task. Receives a cancellation token to monitor for task cancellation.
  - `name`: Optional human-readable identifier for the queue. Defaults to `null`.
  - `priority`: Numeric priority level. Lower values indicate higher priority. Defaults to `0`.
- **Throws**: `ArgumentNullException` if `action` is `null`.

---

### `public Guid EnqueueTask(Func<CancellationToken, Task> taskAction = null)`

Enqueues a new task into the queue, optionally overriding the queue’s default action.

- **Parameters**:
  - `taskAction`: Optional per-task action override. If `null`, the queue’s default `Action` is used.
- **Returns**: A `Guid` uniquely identifying the enqueued task.
- **Throws**:
  - `InvalidOperationException` if the queue is not running (`StartAsync` was not called).
  - `ObjectDisposedException` if the queue has been disposed.

---

### `public async Task StartAsync()`

Starts processing tasks from the queue. Must be called before enqueuing tasks.

- **Returns**: A `Task` representing the asynchronous operation. The task completes when the queue is stopped via cancellation or disposal.
- **Throws**: `InvalidOperationException` if already running or if `StartAsync` is called again after completion.

---

### `public bool CancelTask(Guid taskId)`

Attempts to cancel a specific task by its identifier.

- **Parameters**:
  - `taskId`: The identifier of the task to cancel.
- **Returns**: `true` if the task was found and cancellation was requested; `false` otherwise.
- **Throws**: `InvalidOperationException` if the queue is not running.

---

### `public QueueStatus GetStatus()`

Retrieves the current operational status of the queue.

- **Returns**: A `QueueStatus` value indicating whether the queue is idle, running, or stopped.

---

### `public List<TaskInfo> GetAllTasks()`

Retrieves metadata for all tasks currently in the queue or recently completed.

- **Returns**: A list of `TaskInfo` objects describing each task’s state, timing, and outcome.
- **Throws**: `ObjectDisposedException` if the queue has been disposed.

---
### `public Guid Id`

Gets the unique identifier of the queue instance.

- **Type**: `Guid`
- **Access**: Read-only

---
### `public string Name`

Gets the human-readable name of the queue.

- **Type**: `string`
- **Access**: Read-only

---
### `public Func<CancellationToken, Task> Action`

Gets the default async action executed by tasks enqueued without an override.

- **Type**: `Func<CancellationToken, Task>`
- **Access**: Read-only

---
### `public int Priority`

Gets the numeric priority level of the queue. Lower values indicate higher priority.

- **Type**: `int`
- **Access**: Read-only

---
### `public TaskState State`

Gets the current lifecycle state of the queue (e.g., `Running`, `Stopped`, `Faulted`).

- **Type**: `TaskState`
- **Access**: Read-only

---
### `public DateTime EnqueuedAt`

Gets the UTC timestamp when the queue was instantiated.

- **Type**: `DateTime`
- **Access**: Read-only

---
### `public DateTime? StartedAt`

Gets the UTC timestamp when `StartAsync` was invoked, or `null` if not yet started.

- **Type**: `DateTime?`
- **Access**: Read-only

---
### `public DateTime? CompletedAt`

Gets the UTC timestamp when the queue completed processing, or `null` if still running.

- **Type**: `DateTime?`
- **Access**: Read-only

---
### `public Exception Exception`

Gets the exception that caused the queue to fault, if applicable.

- **Type**: `Exception`
- **Access**: Read-only

---
### `public int CompareTo(AsyncTaskQueue other)`

Compares this queue to another for priority ordering.

- **Parameters**:
  - `other`: The queue to compare with.
- **Returns**: A signed integer indicating relative priority: less than zero if this queue has higher priority, zero if equal, greater than zero if lower.
- **Throws**: `ArgumentNullException` if `other` is `null`.

---
### `public void Enqueue(T item)`

Adds an item to the queue. Not used directly by consumers; primarily for internal priority queue implementation.

- **Parameters**:
  - `item`: The item to enqueue.
- **Throws**: `InvalidOperationException` if the queue is not running.

---
### `public T Dequeue()`

Removes and returns the highest-priority item from the queue. Not used directly by consumers; primarily for internal priority queue implementation.

- **Returns**: The dequeued item.
- **Throws**:
  - `InvalidOperationException` if the queue is empty or not running.
  - `ObjectDisposedException` if the queue has been disposed.

---
### `public List<T> GetAll()`

Retrieves all items currently in the queue. Not used directly by consumers; primarily for internal priority queue implementation.

- **Returns**: A list of all queued items.
- **Throws**: `ObjectDisposedException` if the queue has been disposed.

---
### `public bool IsRunning`

Indicates whether the queue is actively processing tasks.

- **Type**: `bool`
- **Access**: Read-only

## Usage

### Example 1: Basic GPU Task Queue
