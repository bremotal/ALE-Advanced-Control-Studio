using System.Collections.Concurrent;

namespace ALE.ACS.Application.EventBus;

/// <summary>
/// Defines the contract for publishing and subscribing integration events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a handler for the specified event type.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="handler">Event handler.</param>
    void Subscribe<TEvent>(Action<TEvent> handler);

    /// <summary>
    /// Publishes an event to all subscribed handlers.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <param name="eventMessage">Event message instance.</param>
    void Publish<TEvent>(TEvent eventMessage);
}

/// <summary>
/// Thread-safe in-memory event bus for IDE workflow communication.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscriptions = new();
    private readonly object _sync = new();

    /// <inheritdoc />
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_sync)
        {
            var handlers = _subscriptions.GetOrAdd(typeof(TEvent), _ => []);
            handlers.Add(handler);
        }
    }

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent eventMessage)
    {
        ArgumentNullException.ThrowIfNull(eventMessage);

        List<Delegate> handlers;
        lock (_sync)
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out handlers!))
            {
                return;
            }

            handlers = [.. handlers];
        }

        foreach (var handler in handlers.Cast<Action<TEvent>>())
        {
            handler(eventMessage);
        }
    }
}

/// <summary>
/// Event raised when a project is created through the IDE flow.
/// </summary>
/// <param name="ProjectName">Name of the created project.</param>
public sealed record ProjectCreatedEvent(string ProjectName);

/// <summary>
/// Event raised when a controller is created through the IDE flow.
/// </summary>
/// <param name="ControllerName">Name of the created controller.</param>
public sealed record ControllerCreatedEvent(string ControllerName);

/// <summary>
/// Event raised when a controller is renamed through the IDE flow.
/// </summary>
/// <param name="PreviousName">Controller name before the update.</param>
/// <param name="ControllerName">Controller name after the update.</param>
public sealed record ControllerRenamedEvent(
    string PreviousName,
    string ControllerName);
/// <summary>
/// Event raised when a project is closed to clear UI elements.
/// </summary>
public sealed record ProjectClosedEvent();