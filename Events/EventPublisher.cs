// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Event publisher for processing lifecycle events.
    /// Implements pub-sub pattern for decoupled event handling.
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger<EventPublisher> _logger;
        private readonly Dictionary<string, List<Delegate>> _subscribers;
        private readonly object _lockObject = new object();

        public EventPublisher(ILogger<EventPublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subscribers = new Dictionary<string, List<Delegate>>();
        }

        /// <summary>
        /// Subscribes to an event type with an async event handler.
        /// </summary>
        public void Subscribe<T>(string eventType, Func<T, Task> handler) where T : ProcessingEvent
        {
            if (string.IsNullOrWhiteSpace(eventType) || handler == null)
                throw new ArgumentException("Event type and handler cannot be null");

            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _subscribers[eventType] = handlers;
                }

                handlers.Add(handler);

                _logger.LogDebug(
                    "Subscribed to event - Type: {EventType}, Subscribers: {SubscriberCount}",
                    eventType,
                    handlers.Count);
            }
        }

        /// <summary>
        /// Subscribes to an event type with a synchronous event handler.
        /// </summary>
        public void Subscribe<T>(string eventType, Action<T> handler) where T : ProcessingEvent
        {
            if (string.IsNullOrWhiteSpace(eventType) || handler == null)
                throw new ArgumentException("Event type and handler cannot be null");

            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _subscribers[eventType] = handlers;
                }

                handlers.Add(handler);

                _logger.LogDebug(
                    "Subscribed to event - Type: {EventType}, Subscribers: {SubscriberCount}",
                    eventType,
                    handlers.Count);
            }
        }

        /// <summary>
        /// Publishes an event to all subscribers asynchronously.
        /// </summary>
        public async Task PublishAsync<T>(T @event) where T : ProcessingEvent
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            string eventType = @event.EventType;

            _logger.LogInformation(
                "Publishing event - Type: {EventType}, EventId: {EventId}",
                eventType,
                @event.EventId);

            List<Delegate> handlers;
            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(eventType, out handlers) || handlers.Count == 0)
                {
                    _logger.LogDebug("No subscribers for event - Type: {EventType}", eventType);
                    return;
                }

                handlers = new List<Delegate>(handlers);
            }

            foreach (var handler in handlers)
            {
                try
                {
                    if (handler is Func<T, Task> asyncHandler)
                    {
                        await asyncHandler(@event);
                    }
                    else if (handler is Action<T> syncHandler)
                    {
                        syncHandler(@event);
                    }

                    _logger.LogDebug(
                        "Event handler executed - Type: {EventType}, Handler: {HandlerType}",
                        eventType,
                        handler.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Event handler failed - Type: {EventType}, Error: {ErrorMessage}",
                        eventType,
                        ex.Message);
                }
            }

            _logger.LogDebug(
                "Event published successfully - Type: {EventType}, Subscribers: {SubscriberCount}",
                eventType,
                handlers.Count);
        }

        /// <summary>
        /// Unsubscribes from an event type.
        /// </summary>
        public bool Unsubscribe<T>(string eventType, Delegate handler) where T : ProcessingEvent
        {
            if (string.IsNullOrWhiteSpace(eventType) || handler == null)
                return false;

            lock (_lockObject)
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    bool removed = handlers.Remove(handler);
                    if (removed)
                    {
                        _logger.LogDebug(
                            "Unsubscribed from event - Type: {EventType}, Remaining: {SubscriberCount}",
                            eventType,
                            handlers.Count);
                    }

                    return removed;
                }

                return false;
            }
        }

        /// <summary>
        /// Clears all subscribers for an event type.
        /// </summary>
        public void ClearSubscribers(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType))
                return;

            lock (_lockObject)
            {
                _subscribers.Remove(eventType);
                _logger.LogInformation("Cleared all subscribers - Type: {EventType}", eventType);
            }
        }

        /// <summary>
        /// Gets the count of subscribers for an event type.
        /// </summary>
        public int GetSubscriberCount(string eventType)
        {
            lock (_lockObject)
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    return handlers.Count;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets all registered event types.
        /// </summary>
        public IReadOnlyList<string> GetEventTypes()
        {
            lock (_lockObject)
            {
                return _subscribers.Keys.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Interface for event publishing.
    /// </summary>
    public interface IEventPublisher
    {
        void Subscribe<T>(string eventType, Func<T, Task> handler) where T : ProcessingEvent;
        void Subscribe<T>(string eventType, Action<T> handler) where T : ProcessingEvent;
        Task PublishAsync<T>(T @event) where T : ProcessingEvent;
        bool Unsubscribe<T>(string eventType, Delegate handler) where T : ProcessingEvent;
        void ClearSubscribers(string eventType);
        int GetSubscriberCount(string eventType);
        IReadOnlyList<string> GetEventTypes();
    }
}
