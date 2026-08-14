namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Defines the category of a diagnostic message.
/// </summary>
public enum DiagnosticCategory
{
    /// <summary>
    /// Execution related diagnostics.
    /// </summary>
    Execution,

    /// <summary>
    /// Validation related diagnostics.
    /// </summary>
    Validation,

    /// <summary>
    /// Generic engineering diagnostics.
    /// </summary>
    Engineering
}
