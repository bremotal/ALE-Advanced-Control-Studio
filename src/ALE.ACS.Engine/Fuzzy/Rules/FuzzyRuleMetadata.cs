using ALE.ACS.Engine.Fuzzy.Expressions;

namespace ALE.ACS.Engine.Fuzzy.Rules;

/// <summary>
/// Stores metadata about a fuzzy rule, including expression and execution properties.
/// </summary>
public sealed class FuzzyRuleMetadata
{
    /// <summary>
    /// Gets or sets the rule weight.
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// Gets or sets the priority of the rule.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the logical group name for the rule.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the AST expression associated with the rule.
    /// </summary>
    public FuzzyExpressionNode? Expression { get; set; }
}
