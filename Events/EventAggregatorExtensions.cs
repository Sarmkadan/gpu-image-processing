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
        /// <param name="aggregator">EventAggregator instance</param>
        /// <param name="filter">Predicate to filter events</param>
        /// <param name="handler">Handler action</param>
        /// <returns>Disposable subscription</returns>
        public static IDisposable Subscribe<T>(this EventAggregator aggregator, Func<T, bool> filter, Action<T> handler) where T : DomainEvent
        {
            if (aggregator == null)
                throw new ArgumentNullException(nameof(aggregator));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

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
        /// <param name="aggregator">EventAggregator instance</param>
        /// <param name="filter">Predicate to filter events</param>
        /// <param name="handler">Async handler function</param>
        /// <returns>Disposable subscription</returns>
        public static IDisposable SubscribeAsync<T>(this EventAggregator aggregator, Func<T, bool> filter, Func<T, Task> handler) where T : DomainEvent
        {
            if (aggregator == null)
                throw new ArgumentNullException(nameof(aggregator));

            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return aggregator.SubscribeAsync<T>(async @event =>
            {
                if (filter(@event))
                    await handler(@event);
            });
        }

        /// <summary>
        /// Publishes an event and returns a Task that completes when all subscribers finish processing
        /// </summary>
        /// <param name="aggregator">EventAggregator instance</param>
        /// <param name="event">Event to publish</param>
        /// <returns>Task representing the publish operation</returns>
        public static Task PublishAsync(this EventAggregator aggregator, DomainEvent @event, bool throwOnError = false)
        {
            if (aggregator == null)
                throw new ArgumentNullException(nameof(aggregator));

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            try
            {
                return aggregator.PublishAsync(@event);
            }
            catch
            {
                if (throwOnError)
                    throw;
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Publishes an event synchronously and returns a list of all exceptions thrown by subscribers
        /// </summary>
        /// <param name="aggregator">EventAggregator instance</param>
        /// <param name="event">Event to publish</param>
        /// <returns>List of exceptions (empty if all subscribers succeeded)</returns>
        public static List<Exception> PublishWithErrorCapture(this EventAggregator aggregator, DomainEvent @event)
        {
            if (aggregator == null)
                throw new ArgumentNullException(nameof(aggregator));

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var exceptions = new List<Exception>();

            aggregator.Subscribe<DomainEvent>(e => exceptions.Add(new InvalidOperationException("Capture point")));

            try
            {
                aggregator.Publish(@event);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            return exceptions.Where(e => e is not InvalidOperationException).ToList();
        }
    }
}