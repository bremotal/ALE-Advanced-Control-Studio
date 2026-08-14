namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Dispatches events to all subscribed handlers.
/// </summary>
public sealed class EventDispatcher
{
    private readonly EventBus _eventBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
    /// </summary>
    /// <param name="eventBus">The event bus instance.</param>
    public EventDispatcher(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// Publishes an event to the bus.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event to publish.</param>
    public void Dispatch<TEvent>(TEvent @event) where TEvent : class, IEvent
        => _eventBus.Publish(@event);
}
