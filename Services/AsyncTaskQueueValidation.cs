using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GpuImageProcessing.Services
{
    /// <summary>
    /// Provides validation helpers for <see cref="AsyncTaskQueue"/> instances.
    /// </summary>
    public static class AsyncTaskQueueValidation
    {
        /// <summary>
        /// Validates the specified <see cref="AsyncTaskQueue"/> instance.
        /// </summary>
        /// <param name="value">The queue instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AsyncTaskQueue value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            var status = value.GetStatus();
            if (status == null)
            {
                errors.Add("GetStatus() returned null.");
            }

            var allTasks = value.GetAllTasks();
            if (allTasks == null)
            {
                errors.Add("GetAllTasks() returned null.");
            }
            else
            {
                if (allTasks.Any(t => t == null))
                {
                    errors.Add("GetAllTasks() returned a collection containing null elements.");
                }

                foreach (var task in allTasks)
                {
                    ValidateTask(task, errors);
                }
            }

            return errors.AsReadOnly();
        }

        private static void ValidateTask(TaskInfo task, List<string> errors)
        {
            if (task == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(task.Name))
            {
                errors.Add($"Task '{task.Id}' has null, empty, or whitespace Name.");
            }

            if (task.Priority < 0 || task.Priority > 100)
            {
                errors.Add($"Task '{task.Id}' has Priority {task.Priority}, which is out of range [0, 100].");
            }

            if (task.EnqueuedAt == default)
            {
                errors.Add($"Task '{task.Id}' has EnqueuedAt set to default(DateTime).");
            }
            else if (task.EnqueuedAt > DateTime.UtcNow.AddMinutes(1))
            {
                errors.Add($"Task '{task.Id}' has EnqueuedAt in the future ({task.EnqueuedAt:O}).");
            }

            if (task.StartedAt.HasValue)
            {
                if (task.StartedAt.Value == default)
                {
                    errors.Add($"Task '{task.Id}' has StartedAt set to default(DateTime).");
                }
                else if (task.StartedAt.Value < task.EnqueuedAt)
                {
                    errors.Add($"Task '{task.Id}' has StartedAt ({task.StartedAt.Value:O}) before EnqueuedAt ({task.EnqueuedAt:O}).");
                }
            }

            if (task.CompletedAt.HasValue)
            {
                if (task.CompletedAt.Value == default)
                {
                    errors.Add($"Task '{task.Id}' has CompletedAt set to default(DateTime).");
                }
                else if (task.CompletedAt.Value < (task.StartedAt ?? task.EnqueuedAt))
                {
                    errors.Add($"Task '{task.Id}' has CompletedAt ({task.CompletedAt.Value:O}) before StartedAt/EnqueuedAt.");
                }
            }

            if (task.DurationMs.HasValue && task.DurationMs < 0)
            {
                errors.Add($"Task '{task.Id}' has negative DurationMs ({task.DurationMs}).");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="AsyncTaskQueue"/> is valid.
        /// </summary>
        /// <param name="value">The queue instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this AsyncTaskQueue value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="AsyncTaskQueue"/> is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The queue instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the queue is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this AsyncTaskQueue value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"AsyncTaskQueue is invalid. Details:\n  - {
                    string.Join("\n  - ", errors.Select(e => e.Trim()))
                    }");
            }
        }
    }
}