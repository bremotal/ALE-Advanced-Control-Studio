namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Defines the severity level for validation messages.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Information,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error,

    /// <summary>
    /// Critical error message.
    /// </summary>
    Critical
}
