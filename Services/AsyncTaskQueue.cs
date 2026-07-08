#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Async task queue for managing background task execution with priority and concurrency control.
    /// Supports task scheduling, cancellation, and progress tracking.
    /// </summary>
    public class AsyncTaskQueue
    {
        private readonly PriorityQueue<QueuedTask> _taskQueue;
        private readonly HashSet<QueuedTask> _activeTasks;
        private readonly int _maxConcurrentTasks;
        private readonly object _lockObject = new();
        private bool _isRunning;

        public event EventHandler<TaskEventArgs> TaskStarted;
        public event EventHandler<TaskEventArgs> TaskCompleted;
        public event EventHandler<TaskEventArgs> TaskFailed;

        public AsyncTaskQueue(int maxConcurrentTasks = 4)
        {
            _taskQueue = new PriorityQueue<QueuedTask>();
            _activeTasks = new HashSet<QueuedTask>();
            _maxConcurrentTasks = maxConcurrentTasks;
        }

        /// <summary>
        /// Enqueues a task for execution
        /// </summary>
        public Guid EnqueueTask(Func<CancellationToken, Task> taskAction, int priority = 0, string name = null)
        {
            var task = new QueuedTask
            {
                Id = Guid.NewGuid(),
                Action = taskAction,
                Priority = priority,
                Name = name ?? $"Task_{Guid.NewGuid().ToString().Substring(0, 8)}",
                EnqueuedAt = DateTime.UtcNow,
                State = TaskState.Queued
            };

            lock (_lockObject)
            {
                _taskQueue.Enqueue(task);
            }

            return task.Id;
        }

        /// <summary>
        /// Starts processing the task queue
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
                throw new InvalidOperationException("Task queue is already running");

            _isRunning = true;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    QueuedTask task = null;

                    lock (_lockObject)
                    {
                        if (_activeTasks.Count < _maxConcurrentTasks && _taskQueue.Count > 0)
                        {
                            task = _taskQueue.Dequeue();
                            _activeTasks.Add(task);
                        }
                    }

                    if (task != null)
                    {
                        _ = ProcessTaskAsync(task, cancellationToken);
                    }
                    else
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task ProcessTaskAsync(QueuedTask task, CancellationToken cancellationToken)
        {
            try
            {
                task.State = TaskState.Running;
                task.StartedAt = DateTime.UtcNow;
                OnTaskStarted(task);

                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromMinutes(5)); // 5-minute timeout

                await task.Action(cts.Token);

                task.State = TaskState.Completed;
                task.CompletedAt = DateTime.UtcNow;
                OnTaskCompleted(task);
            }
            catch (Exception ex)
            {
                task.State = TaskState.Failed;
                task.CompletedAt = DateTime.UtcNow;
                task.Exception = ex;
                OnTaskFailed(task);
            }
            finally
            {
                lock (_lockObject)
                {
                    _activeTasks.Remove(task);
                }
            }
        }

        /// <summary>
        /// Cancels a queued or running task
        /// </summary>
        public bool CancelTask(Guid taskId)
        {
            lock (_lockObject)
            {
                var task = _activeTasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null && task.State == TaskState.Queued)
                {
                    task.State = TaskState.Cancelled;
                    _activeTasks.Remove(task);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the current queue status
        /// </summary>
        public QueueStatus GetStatus()
        {
            lock (_lockObject)
            {
                var queued = _taskQueue.Count;
                var active = _activeTasks.Count;
                var completed = _activeTasks.Count(t => t.State == TaskState.Completed);
                var failed = _activeTasks.Count(t => t.State == TaskState.Failed);

                return new QueueStatus
                {
                    IsRunning = _isRunning,
                    QueuedCount = queued,
                    ActiveCount = active,
                    CompletedCount = completed,
                    FailedCount = failed,
                    MaxConcurrency = _maxConcurrentTasks
                };
            }
        }

        /// <summary>
        /// Gets information about all tasks
        /// </summary>
        public List<TaskInfo> GetAllTasks()
        {
            lock (_lockObject)
            {
                var allTasks = new List<QueuedTask>();
                allTasks.AddRange(_taskQueue.GetAll());
                allTasks.AddRange(_activeTasks);

                return allTasks.Select(t => new TaskInfo
                {
                    Id = t.Id,
                    Name = t.Name,
                    Priority = t.Priority,
                    State = t.State.ToString(),
                    EnqueuedAt = t.EnqueuedAt,
                    StartedAt = t.StartedAt,
                    CompletedAt = t.CompletedAt,
                    DurationMs = t.CompletedAt.HasValue
                        ? (t.CompletedAt.Value - (t.StartedAt ?? t.EnqueuedAt)).TotalMilliseconds
                        : null
                }).ToList();
            }
        }

        private void OnTaskStarted(QueuedTask task)
        {
            TaskStarted?.Invoke(this, new TaskEventArgs { Task = task });
        }

        private void OnTaskCompleted(QueuedTask task)
        {
            TaskCompleted?.Invoke(this, new TaskEventArgs { Task = task });
        }

        private void OnTaskFailed(QueuedTask task)
        {
            TaskFailed?.Invoke(this, new TaskEventArgs { Task = task });
        }

        public class QueuedTask : IComparable<QueuedTask>
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Func<CancellationToken, Task> Action { get; set; }
            public int Priority { get; set; }
            public TaskState State { get; set; }
            public DateTime EnqueuedAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public Exception Exception { get; set; }

            public int CompareTo(QueuedTask other)
            {
                // Higher priority tasks come first
                return other.Priority.CompareTo(Priority);
            }
        }

        private class PriorityQueue<T> where T : IComparable<T>
        {
            private readonly List<T> _items = new();

            public void Enqueue(T item)
            {
                _items.Add(item);
                _items.Sort();
            }

            public T Dequeue()
            {
                if (_items.Count == 0)
                    return default;

                var item = _items[0];
                _items.RemoveAt(0);
                return item;
            }

            public int Count => _items.Count;

            public List<T> GetAll() => new(_items);
        }
    }

    public class QueueStatus
    {
        public bool IsRunning { get; set; }
        public int QueuedCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public int MaxConcurrency { get; set; }
    }

    public class TaskInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public string State { get; set; }
        public DateTime EnqueuedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? DurationMs { get; set; }
    }

    public class TaskEventArgs : EventArgs
    {
        public AsyncTaskQueue.QueuedTask Task { get; set; }
    }

    public enum TaskState
    {
        Queued,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    public class QueuedTask
    {
        public Guid Id { get; set; }
        public TaskState State { get; set; }
    }
}
