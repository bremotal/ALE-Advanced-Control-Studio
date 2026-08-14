namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Represents a domain event.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the timestamp of the event.
    /// </summary>
    DateTime Timestamp { get; }
}
