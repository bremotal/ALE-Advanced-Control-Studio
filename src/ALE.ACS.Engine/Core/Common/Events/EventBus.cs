namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Lightweight event bus for publishing events to handlers.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = [];

    /// <summary>
    /// Registers a handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="handler">The handler.</param>
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers = [];
            _handlers[typeof(TEvent)] = handlers;
        }

        handlers.Add(handler);
    }

    /// <summary>
    /// Removes a handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="handler">The handler.</param>
    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers.Remove(handler);
        }
    }

    /// <summary>
    /// Publishes an event to all registered handlers.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event to publish.</param>
    public void Publish<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            foreach (var handler in handlers.Cast<IEventHandler<TEvent>>())
            {
                handler.Handle(@event);
            }
        }
    }
}
