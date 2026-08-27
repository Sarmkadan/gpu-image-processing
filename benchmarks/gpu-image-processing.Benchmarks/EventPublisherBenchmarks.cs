using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Benchmarks
{
    [MemoryDiagnoser]
    public class EventPublisherBenchmarks
    {
        private EventPublisher _publisher = null!;
        private readonly List<JobStartedEvent> _events = new();
        private readonly List<Func<JobStartedEvent, Task>> _asyncHandlers = new();
        private readonly List<Action<JobStartedEvent>> _syncHandlers = new();

        [Params(10, 100, 1000)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Use null logger to avoid ILogger dependency in benchmarks
            var logger = NullLogger<EventPublisher>.Instance;
            _publisher = new EventPublisher(logger);

            // Create test events
            for (int i = 0; i < N; i++)
            {
                _events.Add(new JobStartedEvent
                {
                    JobId = Guid.NewGuid().ToString(),
                    JobName = $"Job {i}",
                    TotalImages = 100,
                    FilterIds = new List<string> { "grayscale" },
                    TransformIds = new List<string> { "resize" }
                });
            }

            // Create handlers
            for (int i = 0; i < N; i++)
            {
                int index = i; // capture for closure
                _asyncHandlers.Add(async (ev) =>
                {
                    await Task.Yield();
                    // Simulate some work
                    _ = ev.JobId.Length;
                });

                _syncHandlers.Add(ev =>
                {
                    // Simulate some work
                    _ = ev.JobId.Length;
                });
            }
        }

        [Benchmark]
        public void SubscribeAsyncHandlers()
        {
            // Subscribe N async handlers to a single event type
            string eventType = "job.started";
            for (int i = 0; i < N; i++)
            {
                _publisher.Subscribe<JobStartedEvent>(eventType, _asyncHandlers[i]);
            }
        }

        [Benchmark]
        public void SubscribeSyncHandlers()
        {
            // Subscribe N sync handlers to a single event type
            string eventType = "job.started.sync";
            for (int i = 0; i < N; i++)
            {
                _publisher.Subscribe<JobStartedEvent>(eventType, _syncHandlers[i]);
            }
        }

        [Benchmark]
        public async Task PublishAsync_NoHandlers()
        {
            // Publish to event type with no subscribers
            string eventType = "job.started.none";
            foreach (var ev in _events)
            {
                await _publisher.PublishAsync(ev);
            }
        }

        [Benchmark]
        public async Task PublishAsync_WithHandlers()
        {
            // First subscribe handlers
            string eventType = "job.started.with";
            for (int i = 0; i < N; i++)
            {
                _publisher.Subscribe<JobStartedEvent>(eventType, _asyncHandlers[i]);
            }

            // Then publish events
            foreach (var ev in _events)
            {
                await _publisher.PublishAsync(ev);
            }
        }

        [Benchmark]
        public bool Unsubscribe_ExistingHandler()
        {
            // Subscribe then unsubscribe
            string eventType = "job.started.unsub";
            var handler = _asyncHandlers[0];
            _publisher.Subscribe<JobStartedEvent>(eventType, handler);
            return _publisher.Unsubscribe<JobStartedEvent>(eventType, handler);
        }

        [Benchmark]
        public void ClearSubscribers_Multiple()
        {
            // Subscribe many handlers then clear
            string eventType = "job.started.clear";
            for (int i = 0; i < N; i++)
            {
                _publisher.Subscribe<JobStartedEvent>(eventType, _asyncHandlers[i]);
            }
            _publisher.ClearSubscribers(eventType);
        }

        [Benchmark]
        public int GetSubscriberCount_AfterSubscribes()
        {
            string eventType = "job.started.count";
            for (int i = 0; i < N; i++)
            {
                _publisher.Subscribe<JobStartedEvent>(eventType, _asyncHandlers[i]);
            }
            return _publisher.GetSubscriberCount(eventType);
        }

        [Benchmark]
        public IReadOnlyList<string> GetEventTypes_AfterSubscribes()
        {
            // Subscribe to multiple event types
            for (int i = 0; i < N; i++)
            {
                string eventType = $"event.type.{i}";
                _publisher.Subscribe<JobStartedEvent>(eventType, _asyncHandlers[0]);
            }
            return _publisher.GetEventTypes();
        }
    }
}