namespace GpuImageProcessing.Events;

public static class EventPublisherExtensions
{
    /// <summary>
    /// Clears all subscribers for the specified event type.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="eventType">The event type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="publisher"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="eventType"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static void ClearSubscribersForEventType<T>(this EventPublisher publisher, string eventType)
        where T : ProcessingEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        publisher.ClearSubscribers(eventType);
    }

    /// <summary>
    /// Gets the number of subscribers for the specified event type.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="eventType">The event type.</param>
    /// <returns>The number of subscribers for the specified event type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="publisher"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="eventType"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static int GetSubscriberCountForEventType<T>(this EventPublisher publisher, string eventType)
        where T : ProcessingEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        return publisher.GetSubscriberCount(eventType);
    }

    /// <summary>
    /// Publishes an event asynchronously and returns the number of subscribers that received the event.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="event">The event to publish.</param>
    /// <returns>The number of subscribers that received the event.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="publisher"/> or <paramref name="event"/> is <see langword="null"/>.</exception>
    public static async Task<int> PublishAndGetSubscriberCountAsync<T>(this EventPublisher publisher, T @event)
        where T : ProcessingEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(@event);

        await publisher.PublishAsync(@event);
        return publisher.GetSubscriberCount(@event.EventType);
    }
}