namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Represents failures that occur in the event system.
/// </summary>
public sealed class EventException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public EventException(string message) : base(message)
    {
    }
}
