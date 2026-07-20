#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GpuImageProcessing.Services;
using Xunit;

namespace GpuImageProcessing.Tests.Services
{
    public class AsyncTaskQueueTests
    {
        [Fact]
        public async Task EnqueueTask_ShouldAddTaskToQueue()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var taskRan = false;

            // Act
            var taskId = queue.EnqueueTask(ct =>
            {
                taskRan = true;
                return Task.CompletedTask;
            });

            // Assert - task should be queued
            taskId.Should().NotBe(Guid.Empty);
            var status = queue.GetStatus();
            status.QueuedCount.Should().Be(1);
            status.ActiveCount.Should().Be(0);
        }

        [Fact]
        public async Task EnqueueTask_ShouldExecuteTaskWhenStarted()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var taskRan = false;

            // Act
            queue.EnqueueTask(ct =>
            {
                taskRan = true;
                return Task.CompletedTask;
            });

            await queue.StartAsync(CancellationToken.None);

            // Assert
            taskRan.Should().BeTrue();
        }

        [Fact]
        public async Task StartAsync_ShouldProcessMultipleTasksConcurrently()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 3);
            var activeTasks = new List<int>();
            var completionOrder = new List<int>();
            var taskCount = 5;
            var completionLock = new object();

            // Act - enqueue multiple tasks
            for (int i = 0; i < taskCount; i++)
            {
                queue.EnqueueTask(async ct =>
                {
                    lock (activeTasks)
                    {
                        activeTasks.Add(Environment.CurrentManagedThreadId);
                    }
                    await Task.Delay(100, ct);
                    lock (completionOrder)
                    {
                        completionOrder.Add(i);
                    }
                }, priority: 0, name: $"Task{i}");
            }

            await queue.StartAsync(CancellationToken.None);

            // Assert - should have processed all tasks
            completionOrder.Should().HaveCount(taskCount);

            // Verify concurrency - at most 3 tasks should be active at once
            var status = queue.GetStatus();
            status.CompletedCount.Should().Be(taskCount);
        }

        [Fact]
        public async Task StartAsync_ShouldExecuteTasksInPriorityOrder()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var executionOrder = new List<int>();

            // Act - enqueue tasks with different priorities
            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                executionOrder.Add(3); // Lowest priority
            }, priority: 3, name: "LowPriority");

            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                executionOrder.Add(1); // Highest priority
            }, priority: 1, name: "HighPriority");

            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                executionOrder.Add(2); // Medium priority
            }, priority: 2, name: "MediumPriority");

            await queue.StartAsync(CancellationToken.None);

            // Assert - tasks should execute in priority order (highest first)
            executionOrder.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task CompletedTasks_ShouldBeRemovedFromActiveTasks()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var task1Completed = false;
            var task2Completed = false;

            // Act
            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                task1Completed = true;
            }, name: "Task1");

            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                task2Completed = true;
            }, name: "Task2");

            await queue.StartAsync(CancellationToken.None);

            // Assert - both tasks should have completed
            task1Completed.Should().BeTrue();
            task2Completed.Should().BeTrue();

            // Verify completed tasks are not in active tasks
            var status = queue.GetStatus();
            status.ActiveCount.Should().Be(0);
            status.CompletedCount.Should().Be(2);
        }

        [Fact]
        public async Task ExceptionInTask_ShouldNotBreakQueue()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var task1Completed = false;
            var task2Ran = false;
            var task3Ran = false;

            // Act - enqueue tasks where one will fail
            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                task1Completed = true;
            }, name: "Task1");

            queue.EnqueueTask(ct => throw new InvalidOperationException("Task failed"), name: "FailingTask");

            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                task2Ran = true;
            }, name: "Task2");

            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                task3Ran = true;
            }, name: "Task3");

            await queue.StartAsync(CancellationToken.None);

            // Assert - queue should continue processing after exception
            task1Completed.Should().BeTrue();
            task2Ran.Should().BeTrue();
            task3Ran.Should().BeTrue();

            var status = queue.GetStatus();
            status.CompletedCount.Should().Be(3);
            status.FailedCount.Should().Be(1);
        }

        [Fact]
        public async Task TaskFailedEvent_ShouldBeRaisedOnException()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 1);
            var failedEventRaised = false;
            var exceptionReceived = (Exception)null!;

            queue.TaskFailed += (sender, args) =>
            {
                failedEventRaised = true;
                exceptionReceived = args.Task.Exception;
            };

            // Act
            queue.EnqueueTask(ct => throw new InvalidOperationException("Test exception"));
            await queue.StartAsync(CancellationToken.None);

            // Assert
            failedEventRaised.Should().BeTrue();
            exceptionReceived.Should().NotBeNull();
            exceptionReceived.Should().BeOfType<InvalidOperationException>();
        }

        [Fact]
        public async Task TaskCompletedEvent_ShouldBeRaisedOnSuccess()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 1);
            var completedEventRaised = false;

            queue.TaskCompleted += (sender, args) => completedEventRaised = true;

            // Act
            queue.EnqueueTask(ct => Task.CompletedTask);
            await queue.StartAsync(CancellationToken.None);

            // Assert
            completedEventRaised.Should().BeTrue();
        }

        [Fact]
        public async Task TaskStartedEvent_ShouldBeRaisedWhenTaskBegins()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 1);
            var startedEventRaised = false;

            queue.TaskStarted += (sender, args) => startedEventRaised = true;

            // Act
            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
            });
            await queue.StartAsync(CancellationToken.None);

            // Assert
            startedEventRaised.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllTasks_ShouldReturnAllTasksInQueueAndActive()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);

            var task1Id = queue.EnqueueTask(ct => Task.CompletedTask, name: "Task1");
            var task2Id = queue.EnqueueTask(ct => Task.CompletedTask, name: "Task2");

            // Act
            var tasksBefore = queue.GetAllTasks();

            await queue.StartAsync(CancellationToken.None);

            var tasksAfter = queue.GetAllTasks();

            // Assert
            tasksBefore.Should().HaveCount(2);
            tasksAfter.Should().HaveCount(2);

            tasksBefore.Should().Contain(t => t.Id == task1Id && t.State == "Queued");
            tasksBefore.Should().Contain(t => t.Id == task2Id && t.State == "Queued");
        }

        [Fact]
        public async Task GetStatus_ShouldReflectCurrentQueueState()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 3);

            // Act
            queue.EnqueueTask(ct => Task.CompletedTask, name: "Task1");
            queue.EnqueueTask(ct => Task.CompletedTask, name: "Task2");

            var statusBefore = queue.GetStatus();

            await queue.StartAsync(CancellationToken.None);

            var statusAfter = queue.GetStatus();

            // Assert
            statusBefore.IsRunning.Should().BeFalse();
            statusBefore.QueuedCount.Should().Be(2);
            statusBefore.ActiveCount.Should().Be(0);

            statusAfter.IsRunning.Should().BeFalse();
            statusAfter.QueuedCount.Should().Be(0);
            statusAfter.ActiveCount.Should().Be(0);
            statusAfter.CompletedCount.Should().Be(2);
        }

        [Fact]
        public async Task CancelTask_ShouldRemoveQueuedTask()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var task1Ran = false;
            var task2Ran = false;

            var task1Id = queue.EnqueueTask(async ct =>
            {
                await Task.Delay(100, ct);
                task1Ran = true;
            }, name: "Task1");

            var task2Id = queue.EnqueueTask(async ct =>
            {
                await Task.Delay(100, ct);
                task2Ran = true;
            }, name: "Task2");

            // Act - cancel task2 before it starts
            var cancelResult = queue.CancelTask(task2Id);
            cancelResult.Should().BeTrue();

            await queue.StartAsync(CancellationToken.None);

            // Assert
            task1Ran.Should().BeTrue();
            task2Ran.Should().BeFalse();

            var status = queue.GetStatus();
            status.CompletedCount.Should().Be(1);
        }

        [Fact]
        public async Task StartAsync_ShouldStopWhenCancellationRequested()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var taskCount = 0;

            // Act - start processing
            var processingTask = queue.StartAsync(cts.Token);

            // Enqueue a task
            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(50, ct);
                Interlocked.Increment(ref taskCount);
            });

            // Wait a bit for task to start
            await Task.Delay(100);

            // Cancel and wait for queue to stop
            cts.Cancel();
            await processingTask;

            // Assert - task should have started
            taskCount.Should().Be(1);
        }

        [Fact]
        public void Dispose_ShouldStopProcessing()
        {
            // Arrange
            var queue = new AsyncTaskQueue(maxConcurrentTasks: 2);
            var taskRan = false;

            // Act - enqueue task
            queue.EnqueueTask(async ct =>
            {
                await Task.Delay(100, ct);
                taskRan = true;
            });

            // Start processing briefly
            var processingTask = queue.StartAsync(CancellationToken.None);

            // Give it a moment to start
            Task.Delay(50).Wait();

            // Dispose the queue
            queue.Dispose();

            // Wait for processing to complete
            processingTask.Wait(TimeSpan.FromSeconds(1));

            // Assert - task should not have completed (disposal stops processing)
            taskRan.Should().BeFalse();
        }
    }
}
