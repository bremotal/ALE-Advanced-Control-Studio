namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Represents a subscription to an event bus channel.
/// </summary>
public sealed class EventSubscription
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSubscription"/> class.
    /// </summary>
    /// <param name="handler">The event handler.</param>
    /// <param name="priority">The handler priority.</param>
    public EventSubscription(IEventHandler handler, EventPriority priority)
    {
        Handler = handler;
        Priority = priority;
    }

    /// <summary>
    /// Gets the event handler.
    /// </summary>
    public IEventHandler Handler { get; }

    /// <summary>
    /// Gets the handler priority.
    /// </summary>
    public EventPriority Priority { get; }
}
