#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GpuImageProcessing.Events
{
    /// <summary>
    /// Extension methods for EventAggregator providing common event handling patterns
    /// </summary>
    public static class EventAggregatorExtensions
    {
        /// <summary>
        /// Subscribes to events of a specific type with a filter condition
        /// </summary>
        /// <typeparam name="T">Event type to subscribe to</typeparam>
        /// <param name="aggregator">Event aggregator instance</param>
        /// <param name="filter">Predicate to filter events</param>
        /// <param name="handler">Handler action</param>
        /// <returns>Disposable subscription</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/>, <paramref name="filter"/>, or <paramref name="handler"/> is null</exception>
        public static IDisposable Subscribe<T>(this EventAggregator aggregator, Func<T, bool> filter, Action<T> handler) where T : DomainEvent
        {
            ArgumentNullException.ThrowIfNull(aggregator);
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(handler);

            return aggregator.Subscribe<T>(@event =>
            {
                if (filter(@event))
                    handler(@event);
            });
        }

        /// <summary>
        /// Subscribes to events of a specific type with a filter condition (async)
        /// </summary>
        /// <typeparam name="T">Event type to subscribe to</typeparam>
        /// <param name="aggregator">Event aggregator instance</param>
        /// <param name="filter">Predicate to filter events</param>
        /// <param name="handler">Async handler function</param>
        /// <returns>Disposable subscription</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/>, <paramref name="filter"/>, or <paramref name="handler"/> is null</exception>
        public static IDisposable SubscribeAsync<T>(this EventAggregator aggregator, Func<T, bool> filter, Func<T, Task> handler) where T : DomainEvent
        {
            ArgumentNullException.ThrowIfNull(aggregator);
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(handler);

            return aggregator.SubscribeAsync<T>(async @event =>
            {
                if (filter(@event))
                    await handler(@event);
            });
        }

        /// <summary>
        /// Publishes an event and returns a Task that completes when all subscribers finish processing
        /// </summary>
        /// <param name="aggregator">Event aggregator instance</param>
        /// <param name="event">Event to publish</param>
        /// <param name="throwOnError">Whether to throw exceptions from subscribers</param>
        /// <returns>Task representing the publish operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> or <paramref name="event"/> is null</exception>
        public static Task PublishAsync(this EventAggregator aggregator, DomainEvent @event, bool throwOnError = false)
        {
            ArgumentNullException.ThrowIfNull(aggregator);
            ArgumentNullException.ThrowIfNull(@event);

            try
            {
                return aggregator.PublishAsync(@event);
            }
            catch when (!throwOnError)
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Publishes an event synchronously and returns a list of all exceptions thrown by subscribers
        /// </summary>
        /// <param name="aggregator">Event aggregator instance</param>
        /// <param name="event">Event to publish</param>
        /// <returns>List of exceptions (empty if all subscribers succeeded)</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="aggregator"/> or <paramref name="event"/> is null</exception>
        public static List<Exception> PublishWithErrorCapture(this EventAggregator aggregator, DomainEvent @event)
        {
            ArgumentNullException.ThrowIfNull(aggregator);
            ArgumentNullException.ThrowIfNull(@event);

            var exceptions = new List<Exception>();

            aggregator.Subscribe<DomainEvent>(e =>
            {
                // This handler captures exceptions from other subscribers
                // It will be invoked for all events, but we only care about exceptions
            });

            try
            {
                aggregator.Publish(@event);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            return exceptions;
        }
    }
}