namespace GpuImageProcessing.Events;

public static class EventPublisherExtensions
{
    /// <summary>
    /// Clears all subscribers for the specified event type.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="eventType">The event type.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="eventType"/> is null or empty.</exception>
    public static void ClearSubscribersForEventType<T>(this EventPublisher publisher, string eventType) 
        where T : ProcessingEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        foreach (var et in publisher.GetEventTypes())
        {
            if (et == eventType)
            {
                publisher.Unsubscribe<T>(et, null);
            }
        }
    }

    /// <summary>
    /// Gets the number of subscribers for the specified event type.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="eventType">The event type.</param>
    /// <returns>The number of subscribers.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="eventType"/> is null or empty.</exception>
    public static int GetSubscriberCountForEventType<T>(this EventPublisher publisher, string eventType) 
        where T : ProcessingEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(eventType);

        var count = 0;
        foreach (var et in publisher.GetEventTypes())
        {
            if (et == eventType)
            {
                count += publisher.GetSubscriberCount(et);
            }
        }
        return count;
    }

    /// <summary>
    /// Publishes an event asynchronously and returns the number of subscribers that received the event.
    /// </summary>
    /// <typeparam name="T">The type of event.</typeparam>
    /// <param name="publisher">The event publisher.</param>
    /// <param name="event">The event to publish.</param>
    /// <returns>The number of subscribers that received the event.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is null or <paramref name="event"/> is null.</exception>
    public static async Task<int> PublishAndGetSubscriberCountAsync<T>(this EventPublisher publisher, T @event) 
        where T : ProcessingEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(@event);

        await publisher.PublishAsync(@event);
        return publisher.GetSubscriberCount(typeof(T).Name);
    }
}
