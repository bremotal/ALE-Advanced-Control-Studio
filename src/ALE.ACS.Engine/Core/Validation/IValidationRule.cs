namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Defines a reusable validation rule.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Validates the supplied target.
    /// </summary>
    /// <param name="target">The target to validate.</param>
    /// <param name="context">The validation context.</param>
    /// <returns>The validation result.</returns>
    ValidationResult Validate(object target, ValidationContext context);
}
