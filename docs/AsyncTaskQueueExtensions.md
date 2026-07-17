# AsyncTaskQueueExtensions

Extension methods for managing and inspecting an asynchronous task queue, primarily used to track and control the execution of image processing tasks in GPU-accelerated pipelines.

## API

### `EnqueueTask`
Enqueues a new task for asynchronous execution and returns a unique identifier for the task.
- **Parameters**: `queue` – The task queue instance, `action` – The asynchronous action to enqueue.
- **Returns**: A `Guid` uniquely identifying the enqueued task.
- **Throws**: `ArgumentNullException` if `queue` or `action` is `null`.

### `GetTasksByState`
Retrieves all tasks currently in the specified execution state.
- **Parameters**: `queue` – The task queue, `state` – The task state to filter by.
- **Returns**: An `IReadOnlyList<TaskInfo>` containing all tasks matching the given state. Empty list if none match.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetTaskCount`
Returns the number of tasks currently in the queue, regardless of state.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `int` representing the total number of tasks in the queue.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetTotalTaskCount`
Returns the total number of tasks that have ever been enqueued in the queue.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `int` representing the total task count, including completed, failed, and cancelled tasks.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetOldestTask`
Retrieves the oldest task in the queue based on enqueue time.
- **Parameters**: `queue` – The task queue.
- **Returns**: A `TaskInfo` representing the oldest task, or `null` if the queue is empty.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetNewestTask`
Retrieves the most recently enqueued task.
- **Parameters**: `queue` – The task queue.
- **Returns**: A `TaskInfo` representing the newest task, or `null` if the queue is empty.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetRunningTasks`
Retrieves all tasks currently executing.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `IReadOnlyList<TaskInfo>` of currently running tasks. Empty list if none are running.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetQueuedTasks`
Retrieves all tasks that are queued and not yet started.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `IReadOnlyList<TaskInfo>` of queued tasks. Empty list if none are queued.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetCompletedTasks`
Retrieves all tasks that have completed successfully.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `IReadOnlyList<TaskInfo>` of completed tasks. Empty list if none have completed.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetFailedTasks`
Retrieves all tasks that failed during execution.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `IReadOnlyList<TaskInfo>` of failed tasks. Empty list if none have failed.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetCancelledTasks`
Retrieves all tasks that were cancelled before completion.
- **Parameters**: `queue` – The task queue.
- **Returns**: An `IReadOnlyList<TaskInfo>` of cancelled tasks. Empty list if none were cancelled.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetTaskDurationMilliseconds`
Retrieves the duration in milliseconds of a specific task, if available.
- **Parameters**: `queue` – The task queue, `taskId` – The unique identifier of the task.
- **Returns**: A `long?` representing the task duration in milliseconds, or `null` if the task does not exist or duration is not recorded.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetTaskPosition`
Retrieves the current position of a task in the queue, if it is queued.
- **Parameters**: `queue` – The task queue, `taskId` – The unique identifier of the task.
- **Returns**: An `int` representing the 0-based position in the queue, or `-1` if the task is not queued or does not exist.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `HasTasksInState`
Determines whether any tasks in the queue are in the specified state.
- **Parameters**: `queue` – The task queue, `state` – The task state to check.
- **Returns**: `true` if at least one task is in the given state; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

### `GetAverageTaskDurationMilliseconds`
Calculates the average duration in milliseconds of all completed tasks in the queue.
- **Parameters**: `queue` – The task queue.
- **Returns**: A `double?` representing the average duration in milliseconds, or `null` if no tasks have completed.
- **Throws**: `ArgumentNullException` if `queue` is `null`.

## Usage
