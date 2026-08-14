namespace ALE.ACS.Engine.Fuzzy.Diagnostics;

/// <summary>
/// Represents a single validation issue produced by the fuzzy consistency checker.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="code">The diagnostic code.</param>
    /// <param name="severity">The diagnostic severity.</param>
    /// <param name="message">The text describing the issue.</param>
    public ValidationResult(DiagnosticCode code, DiagnosticSeverity severity, string message)
    {
        Code = code;
        Severity = severity;
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// Gets the diagnostic code.
    /// </summary>
    public DiagnosticCode Code { get; }

    /// <summary>
    /// Gets the severity of the diagnostic.
    /// </summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public string Message { get; }
}
