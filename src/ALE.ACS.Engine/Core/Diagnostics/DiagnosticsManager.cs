namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Centralizes the storage and retrieval of diagnostic messages.
/// </summary>
public sealed class DiagnosticsManager
{
    private readonly List<DiagnosticMessage> _messages = [];

    /// <summary>
    /// Adds a diagnostic message to the manager.
    /// </summary>
    /// <param name="message">The diagnostic message to add.</param>
    public void Add(DiagnosticMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        _messages.Add(message);
    }

    /// <summary>
    /// Gets all messages.
    /// </summary>
    /// <returns>The diagnostic messages.</returns>
    public IReadOnlyCollection<DiagnosticMessage> GetAll() => _messages;

    /// <summary>
    /// Gets messages filtered by severity.
    /// </summary>
    /// <param name="severity">The severity to filter by.</param>
    /// <returns>The filtered messages.</returns>
    public IReadOnlyCollection<DiagnosticMessage> GetMessages(DiagnosticSeverity severity)
        => _messages.Where(message => message.Severity == severity).ToList();

    /// <summary>
    /// Gets messages filtered by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>The filtered messages.</returns>
    public IReadOnlyCollection<DiagnosticMessage> GetMessages(DiagnosticCategory category)
        => _messages.Where(message => message.Category == category).ToList();
}
