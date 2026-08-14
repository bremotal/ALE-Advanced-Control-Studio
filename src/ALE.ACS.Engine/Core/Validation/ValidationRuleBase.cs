namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Base implementation for validation rules.
/// </summary>
public abstract class ValidationRuleBase : IValidationRule
{
    /// <inheritdoc />
    public abstract ValidationResult Validate(object target, ValidationContext context);

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    protected static ValidationResult CreateSuccess()
        => new(true, Array.Empty<ValidationMessage>(), true);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="severity">The severity of the message.</param>
    /// <param name="category">The message category.</param>
    /// <param name="code">The message code.</param>
    /// <param name="text">The message text.</param>
    /// <param name="executionAllowed">Whether execution is allowed.</param>
    /// <returns>A failed validation result.</returns>
    protected static ValidationResult CreateFailure(ValidationSeverity severity, ValidationCategory category, string code, string text, bool executionAllowed = false)
        => new(false, new[] { new ValidationMessage(severity, category, code, text) }, executionAllowed);
}
