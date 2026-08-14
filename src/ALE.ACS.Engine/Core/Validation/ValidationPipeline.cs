namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Executes registered validation rules against an engineering target.
/// </summary>
public sealed class ValidationPipeline
{
    private readonly IReadOnlyCollection<IValidationRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationPipeline"/> class.
    /// </summary>
    /// <param name="rules">The validation rules to run.</param>
    public ValidationPipeline(IReadOnlyCollection<IValidationRule>? rules)
    {
        _rules = rules ?? Array.Empty<IValidationRule>();
    }

    /// <summary>
    /// Validates the supplied target.
    /// </summary>
    /// <param name="target">The target to validate.</param>
    /// <param name="context">The optional validation context.</param>
    /// <returns>The aggregated validation result.</returns>
    public ValidationResult Validate(object target, ValidationContext? context = null)
    {
        var validationContext = context ?? new ValidationContext(null);
        var messages = new List<ValidationMessage>();

        foreach (var rule in _rules)
        {
            var result = rule.Validate(target, validationContext);
            messages.AddRange(result.Messages);
        }

        var isValid = !messages.Any(message => message.Severity is ValidationSeverity.Error or ValidationSeverity.Critical);
        var executionAllowed = !messages.Any(message => message.Severity is ValidationSeverity.Error or ValidationSeverity.Critical);
        return new ValidationResult(isValid, messages, executionAllowed);
    }
}
