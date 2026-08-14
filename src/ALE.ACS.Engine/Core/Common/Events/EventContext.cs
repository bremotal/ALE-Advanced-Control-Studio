namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Provides contextual information available while processing events.
/// </summary>
public sealed class EventContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventContext"/> class.
    /// </summary>
    /// <param name="metadata">The metadata associated with the event context.</param>
    public EventContext(IReadOnlyDictionary<string, object?>? metadata)
    {
        Metadata = metadata ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the metadata associated with the context.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; }
}
