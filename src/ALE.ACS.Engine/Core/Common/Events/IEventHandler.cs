namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Base contract for event handlers.
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Processes the supplied event.
    /// </summary>
    /// <param name="event">The event to process.</param>
    /// <param name="context">The event context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleAsync(IEvent @event, EventContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handles a specific event type.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public interface IEventHandler<in TEvent> : IEventHandler where TEvent : class, IEvent
{
    /// <summary>
    /// Processes the supplied event.
    /// </summary>
    /// <param name="event">The event to process.</param>
    void Handle(TEvent @event);
}
