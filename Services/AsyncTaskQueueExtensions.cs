using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides extension methods for <see cref="AsyncTaskQueue"/> to enhance functionality
    /// with common queue operations and task management utilities.
    /// </summary>
    public static class AsyncTaskQueueExtensions
    {
        /// <summary>
        /// Enqueues a new task with the specified action and optional priority.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <param name="action">The async action to execute.</param>
        /// <param name="priority">Optional priority (default: 0).</param>
        /// <param name="name">Optional name for the task.</param>
        /// <returns>The enqueued task's unique identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> or <paramref name="action"/> is null.</exception>
        public static Guid EnqueueTask(this AsyncTaskQueue queue, Func<CancellationToken, Task> action, int priority = 0, string name = null)
        {
            ArgumentNullException.ThrowIfNull(queue);
            ArgumentNullException.ThrowIfNull(action);

            return queue.EnqueueTask(action, priority, name);
        }

        /// <summary>
        /// Gets all tasks from the queue that match the specified state.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <param name="state">The task state to filter by.</param>
        /// <returns>Read-only list of matching tasks.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static IReadOnlyList<TaskInfo> GetTasksByState(this AsyncTaskQueue queue, TaskState state)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetAllTasks()
                .Where(t => string.Equals(t.State, state.ToString(), StringComparison.Ordinal))
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets the count of tasks in the specified state.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <param name="state">The task state to count.</param>
        /// <returns>The number of tasks in the specified state.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static int GetTaskCount(this AsyncTaskQueue queue, TaskState state)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetAllTasks()
                .Count(t => string.Equals(t.State, state.ToString(), StringComparison.Ordinal));
        }

        /// <summary>
        /// Gets the total number of tasks currently in the queue.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>The total task count.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static int GetTotalTaskCount(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetAllTasks().Count;
        }

        /// <summary>
        /// Gets the oldest task in the queue (first to be processed).
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>The oldest task, or null if queue is empty.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static TaskInfo GetOldestTask(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetAllTasks()
                .OrderBy(t => t.EnqueuedAt)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the newest task in the queue (most recently added).
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>The newest task, or null if queue is empty.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static TaskInfo GetNewestTask(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetAllTasks()
                .OrderByDescending(t => t.EnqueuedAt)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets tasks that are currently running.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>Read-only list of running tasks.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static IReadOnlyList<TaskInfo> GetRunningTasks(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetTasksByState(TaskState.Running);
        }

        /// <summary>
        /// Gets tasks that are queued (waiting to be processed).
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>Read-only list of queued tasks.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static IReadOnlyList<TaskInfo> GetQueuedTasks(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetTasksByState(TaskState.Queued);
        }

        /// <summary>
        /// Gets tasks that have completed successfully.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>Read-only list of completed tasks.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static IReadOnlyList<TaskInfo> GetCompletedTasks(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetTasksByState(TaskState.Completed);
        }

        /// <summary>
        /// Gets tasks that have failed.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>Read-only list of failed tasks.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static IReadOnlyList<TaskInfo> GetFailedTasks(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetTasksByState(TaskState.Failed);
        }

        /// <summary>
        /// Gets tasks that have been cancelled.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>Read-only list of cancelled tasks.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static IReadOnlyList<TaskInfo> GetCancelledTasks(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetTasksByState(TaskState.Cancelled);
        }

        /// <summary>
        /// Gets the duration of the specified task in milliseconds.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <param name="taskId">The task identifier.</param>
        /// <returns>The duration in milliseconds, or null if task hasn't completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="taskId"/> is empty.</exception>
        public static long? GetTaskDurationMilliseconds(this AsyncTaskQueue queue, Guid taskId)
        {
            ArgumentNullException.ThrowIfNull(queue);
            ArgumentException.ThrowIfNullOrEmpty(taskId.ToString());

            var task = queue.GetAllTasks().FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return null;

            if (task.CompletedAt == null)
                return null;

            return (long)(task.CompletedAt.Value - task.StartedAt!.Value).TotalMilliseconds;
        }

        /// <summary>
        /// Gets the priority-adjusted position of a task in the queue.
        /// Lower priority values are processed first.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <param name="taskId">The task identifier.</param>
        /// <returns>The position (0-based), or -1 if task not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="taskId"/> is empty.</exception>
        public static int GetTaskPosition(this AsyncTaskQueue queue, Guid taskId)
        {
            ArgumentNullException.ThrowIfNull(queue);
            ArgumentException.ThrowIfNullOrEmpty(taskId.ToString());

            var tasks = queue.GetAllTasks();
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return -1;

            return tasks.Count(t => t.Priority < task.Priority ||
                                (t.Priority == task.Priority &&
                                 string.Compare(t.EnqueuedAt.ToString(CultureInfo.InvariantCulture),
                                              task.EnqueuedAt.ToString(CultureInfo.InvariantCulture),
                                              StringComparison.Ordinal) < 0));
        }

        /// <summary>
        /// Checks if the queue contains any tasks in the specified state.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <param name="state">The task state to check.</param>
        /// <returns>True if any tasks match the state; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static bool HasTasksInState(this AsyncTaskQueue queue, TaskState state)
        {
            ArgumentNullException.ThrowIfNull(queue);

            return queue.GetTasksByState(state).Count > 0;
        }

        /// <summary>
        /// Gets the average duration of completed tasks in milliseconds.
        /// </summary>
        /// <param name="queue">The queue instance.</param>
        /// <returns>The average duration in milliseconds, or null if no tasks completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="queue"/> is null.</exception>
        public static double? GetAverageTaskDurationMilliseconds(this AsyncTaskQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);

            var completedTasks = queue.GetCompletedTasks();
            if (completedTasks.Count == 0)
                return null;

            var durations = completedTasks
                .Select(t => (t.CompletedAt!.Value - t.StartedAt!.Value).TotalMilliseconds)
                .ToList();

            return durations.Average();
        }
    }
}
