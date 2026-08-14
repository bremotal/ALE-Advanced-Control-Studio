namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Represents an execution-oriented log for diagnostic messaging.
/// </summary>
public sealed class ExecutionLog
{
    private readonly List<DiagnosticMessage> _messages = [];

    /// <summary>
    /// Adds a diagnostic message to the execution log.
    /// </summary>
    /// <param name="message">The diagnostic message.</param>
    public void Add(DiagnosticMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Gets all messages in the execution log.
    /// </summary>
    public IReadOnlyCollection<DiagnosticMessage> Messages => _messages;
}
