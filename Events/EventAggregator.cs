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

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Central event bus for publish-subscribe pattern across the application.
    /// Supports synchronous and asynchronous event handling with topic subscriptions.
    /// </summary>
    public class EventAggregator
    {
        private readonly Dictionary<string, List<EventSubscription>> _subscriptions;
        private readonly Dictionary<string, Func<ProcessingEvent, Task>> _asyncHandlers;
        private readonly ReaderWriterLockSlim _subscriptionLock;
        private bool _disposed = false;

        public EventAggregator()
        {
            _subscriptions = new Dictionary<string, List<EventSubscription>>();
            _asyncHandlers = new Dictionary<string, Func<ProcessingEvent, Task>>();
            _subscriptionLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Subscribes to events of a specific type
        /// </summary>
        public IDisposable Subscribe<T>(Action<T> handler) where T : ProcessingEvent
        {
            ThrowIfDisposed();

            var eventType = typeof(T).Name;
            var subscription = new EventSubscription
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                Handler = (e) => handler((T)e),
                SubscribedAt = DateTime.UtcNow
            };

            _subscriptionLock.EnterWriteLock();
            try
            {
                if (!_subscriptions.ContainsKey(eventType))
                    _subscriptions[eventType] = new List<EventSubscription>();

                _subscriptions[eventType].Add(subscription);
            }
            finally
            {
                _subscriptionLock.ExitWriteLock();
            }

            return new EventSubscriptionDisposable(this, subscription);
        }

        /// <summary>
        /// Subscribes to events asynchronously
        /// </summary>
        public IDisposable SubscribeAsync<T>(Func<T, Task> handler) where T : ProcessingEvent
        {
            ThrowIfDisposed();

            var eventType = typeof(T).Name;
            var id = Guid.NewGuid();
            var key = $"{eventType}_{id}";

            _asyncHandlers[key] = (e) => handler((T)e);

            _subscriptionLock.EnterWriteLock();
            try
            {
                if (!_subscriptions.ContainsKey(eventType))
                    _subscriptions[eventType] = new List<EventSubscription>();

                _subscriptions[eventType].Add(new EventSubscription
                {
                    Id = id,
                    EventType = eventType,
                    IsAsync = true,
                    SubscribedAt = DateTime.UtcNow
                });
            }
            finally
            {
                _subscriptionLock.ExitWriteLock();
            }

            return new EventSubscriptionDisposable(this, key);
        }

        /// <summary>
        /// Publishes an event to all subscribers
        /// </summary>
        public async Task PublishAsync(ProcessingEvent @event)
        {
            ThrowIfDisposed();

            var eventType = @event.GetType().Name;

            _subscriptionLock.EnterReadLock();
            try
            {
                if (!_subscriptions.TryGetValue(eventType, out var subs))
                    return;

                var subscribersCopy = subs.ToList(); // Copy to avoid issues if unsub during iteration

                var tasks = new List<Task>();

                foreach (var subscription in subscribersCopy)
                {
                    try
                    {
                        if (subscription.IsAsync)
                        {
                            var key = $"{eventType}_{subscription.Id}";
                            if (_asyncHandlers.TryGetValue(key, out var asyncHandler))
                                tasks.Add(asyncHandler(@event));
                        }
                        else
                        {
                            subscription.Handler?.Invoke(@event);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other subscribers
                        Console.Error.WriteLine($"Error in event handler: {ex.Message}");
                    }
                }

                if (tasks.Count > 0)
                    await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                _subscriptionLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Publishes an event synchronously
        /// </summary>
        public void Publish(ProcessingEvent @event)
        {
            PublishAsync(@event).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets subscription statistics
        /// </summary>
        public SubscriptionStats GetStats()
        {
            _subscriptionLock.EnterReadLock();
            try
            {
                var totalSubscriptions = _subscriptions.Values.Sum(s => s.Count);
                return new SubscriptionStats
                {
                    TotalEventTypes = _subscriptions.Count,
                    TotalSubscriptions = totalSubscriptions,
                    EventTypes = _subscriptions.Keys.ToList()
                };
            }
            finally
            {
                _subscriptionLock.ExitReadLock();
            }
        }

        internal void Unsubscribe(EventSubscription subscription)
        {
            _subscriptionLock.EnterWriteLock();
            try
            {
                if (_subscriptions.TryGetValue(subscription.EventType, out var subs))
                {
                    subs.RemoveAll(s => s.Id == subscription.Id);
                }
            }
            finally
            {
                _subscriptionLock.ExitWriteLock();
            }
        }

        internal void UnsubscribeAsync(string key)
        {
            _asyncHandlers.Remove(key);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("EventAggregator");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _subscriptionLock?.Dispose();
        }

        private class EventSubscription
        {
            public Guid Id { get; set; }
            public string EventType { get; set; }
            public Action<ProcessingEvent> Handler { get; set; }
            public bool IsAsync { get; set; }
            public DateTime SubscribedAt { get; set; }
        }

        private class EventSubscriptionDisposable : IDisposable
        {
            private readonly EventAggregator _aggregator;
            private readonly EventSubscription _subscription;
            private readonly string _asyncKey;

            public EventSubscriptionDisposable(EventAggregator aggregator, EventSubscription subscription)
            {
                _aggregator = aggregator;
                _subscription = subscription;
            }

            public EventSubscriptionDisposable(EventAggregator aggregator, string asyncKey)
            {
                _aggregator = aggregator;
                _asyncKey = asyncKey;
            }

            public void Dispose()
            {
                if (_subscription != null)
                    _aggregator.Unsubscribe(_subscription);

                if (!string.IsNullOrEmpty(_asyncKey))
                    _aggregator.UnsubscribeAsync(_asyncKey);
            }
        }
    }

    public class SubscriptionStats
    {
        public int TotalEventTypes { get; set; }
        public int TotalSubscriptions { get; set; }
        public List<string> EventTypes { get; set; }
    }

    public abstract class ProcessingEvent
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Source { get; set; }

        protected ProcessingEvent()
        {
            Id = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
        }
    }
}
