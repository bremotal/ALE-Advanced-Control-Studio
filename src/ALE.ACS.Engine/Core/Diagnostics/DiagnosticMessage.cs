namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Represents a diagnostic message emitted during engineering or execution.
/// </summary>
public sealed class DiagnosticMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
    /// </summary>
    /// <param name="severity">The severity.</param>
    /// <param name="category">The category.</param>
    /// <param name="code">The diagnostic code.</param>
    /// <param name="text">The message text.</param>
    /// <param name="timestamp">The timestamp.</param>
    public DiagnosticMessage(DiagnosticSeverity severity, DiagnosticCategory category, string code, string text, DateTime timestamp)
    {
        Severity = severity;
        Category = category;
        Code = code;
        Text = text;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the severity.
    /// </summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the category.
    /// </summary>
    public DiagnosticCategory Category { get; }

    /// <summary>
    /// Gets the diagnostic code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the message text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the timestamp of the diagnostic.
    /// </summary>
    public DateTime Timestamp { get; }
}
