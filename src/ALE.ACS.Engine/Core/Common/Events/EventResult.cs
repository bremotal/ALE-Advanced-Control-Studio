namespace ALE.ACS.Engine.Core.Common.Events;

/// <summary>
/// Represents the outcome of an event dispatch operation.
/// </summary>
public sealed class EventResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventResult"/> class.
    /// </summary>
    /// <param name="success">Whether the event processing succeeded.</param>
    /// <param name="message">An optional result message.</param>
    public EventResult(bool success, string? message)
    {
        Success = success;
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the event succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the optional result message.
    /// </summary>
    public string? Message { get; }
}
