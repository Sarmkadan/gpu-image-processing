using BenchmarkDotNet.Attributes;
using GpuImageProcessing.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GpuImageProcessing.Benchmarks
{
    [MemoryDiagnoser]
    public class EventAggregatorBenchmarks
    {
        private EventAggregator _eventAggregator = null!;
        private TestEvent _testEvent = null!;
        private List<IDisposable> _subscriptions = null!;

        // Parameter for the number of subscribers
        [Params(10, 100, 1000)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _eventAggregator = new EventAggregator();
            _testEvent = new TestEvent
            {
                Id = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                Source = "Benchmark"
            };
            _subscriptions = new List<IDisposable>();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        [Benchmark]
        public void SubscribeSync()
        {
            for (int i = 0; i < N; i++)
            {
                var subscription = _eventAggregator.Subscribe<TestEvent>(HandleEvent);
                _subscriptions.Add(subscription);
            }
        }

        [Benchmark]
        public void SubscribeAsync()
        {
            for (int i = 0; i < N; i++)
            {
                var subscription = _eventAggregator.SubscribeAsync<TestEvent>(HandleEventAsync);
                _subscriptions.Add(subscription);
            }
        }

        [Benchmark]
        public void PublishSync()
        {
            // First, subscribe N handlers
            for (int i = 0; i < N; i++)
            {
                var subscription = _eventAggregator.Subscribe<TestEvent>(HandleEvent);
                _subscriptions.Add(subscription);
            }

            // Then publish the event
            _eventAggregator.Publish(_testEvent);

            // Clean up subscriptions for this iteration
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        [Benchmark]
        public async Task PublishAsync()
        {
            // First, subscribe N handlers (mix of sync and async)
            for (int i = 0; i < N; i++)
            {
                if (i % 2 == 0)
                {
                    var subscription = _eventAggregator.Subscribe<TestEvent>(HandleEvent);
                    _subscriptions.Add(subscription);
                }
                else
                {
                    var subscription = _eventAggregator.SubscribeAsync<TestEvent>(HandleEventAsync);
                    _subscriptions.Add(subscription);
                }
            }

            // Then publish the event asynchronously
            await _eventAggregator.PublishAsync(_testEvent);

            // Clean up subscriptions for this iteration
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        private void HandleEvent(TestEvent @event)
        {
            // Intentionally left empty - we're measuring the invocation overhead
        }

        private Task HandleEventAsync(TestEvent @event)
        {
            // Intentionally left empty - we're measuring the invocation overhead
            return Task.CompletedTask;
        }

        private class TestEvent : DomainEvent
        {
            // No additional properties needed for this benchmark
        }
    }
}