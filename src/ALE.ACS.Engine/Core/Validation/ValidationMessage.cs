namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Represents a single validation message.
/// </summary>
public sealed class ValidationMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
    /// </summary>
    /// <param name="severity">The severity of the message.</param>
    /// <param name="category">The category of the message.</param>
    /// <param name="code">The message code.</param>
    /// <param name="text">The message text.</param>
    public ValidationMessage(ValidationSeverity severity, ValidationCategory category, string code, string text)
    {
        Severity = severity;
        Category = category;
        Code = code;
        Text = text;
    }

    /// <summary>
    /// Gets the severity.
    /// </summary>
    public ValidationSeverity Severity { get; }

    /// <summary>
    /// Gets the validation category.
    /// </summary>
    public ValidationCategory Category { get; }

    /// <summary>
    /// Gets the message code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the message text.
    /// </summary>
    public string Text { get; }
}
