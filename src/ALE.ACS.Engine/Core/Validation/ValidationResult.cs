namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Represents the outcome of a validation pipeline run.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Whether the target passed validation.</param>
    /// <param name="messages">The validation messages.</param>
    /// <param name="executionAllowed">Whether execution is allowed.</param>
    public ValidationResult(bool isValid, IReadOnlyCollection<ValidationMessage> messages, bool executionAllowed)
    {
        IsValid = isValid;
        Messages = messages;
        Warnings = messages.Where(message => message.Severity == ValidationSeverity.Warning).ToList();
        Errors = messages.Where(message => message.Severity is ValidationSeverity.Error or ValidationSeverity.Critical).ToList();
        ExecutionAllowed = executionAllowed;
    }

    /// <summary>
    /// Gets a value indicating whether validation passed.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the full set of validation messages.
    /// </summary>
    public IReadOnlyCollection<ValidationMessage> Messages { get; }

    /// <summary>
    /// Gets the warnings emitted by the validation.
    /// </summary>
    public IReadOnlyCollection<ValidationMessage> Warnings { get; }

    /// <summary>
    /// Gets the errors emitted by the validation.
    /// </summary>
    public IReadOnlyCollection<ValidationMessage> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether execution is allowed.
    /// </summary>
    public bool ExecutionAllowed { get; }
}
