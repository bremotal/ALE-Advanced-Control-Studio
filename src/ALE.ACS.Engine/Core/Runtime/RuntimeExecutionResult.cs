namespace ALE.ACS.Engine.Core.Runtime;

/// <summary>
/// Represents the outcome of a runtime execution step.
/// </summary>
public sealed class RuntimeExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeExecutionResult"/> class.
    /// </summary>
    /// <param name="success">Whether the step completed successfully.</param>
    /// <param name="message">A human-readable message.</param>
    public RuntimeExecutionResult(bool success, string? message = null)
    {
        Success = success;
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the step completed successfully.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the optional message for the step result.
    /// </summary>
    public string? Message { get; }
}
