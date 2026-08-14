namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Defines the severity of a diagnostic message.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// The diagnostic is for tracing.
    /// </summary>
    Trace,

    /// <summary>
    /// The diagnostic is informational.
    /// </summary>
    Information,

    /// <summary>
    /// The diagnostic is a warning.
    /// </summary>
    Warning,

    /// <summary>
    /// The diagnostic is an error.
    /// </summary>
    Error,

    /// <summary>
    /// The diagnostic is a critical error.
    /// </summary>
    Critical
}
