namespace ALE.ACS.Engine.Fuzzy.Rules;

/// <summary>
/// Defines how multiple antecedents in a fuzzy rule are combined.
/// </summary>
public enum FuzzyOperator
{
    /// <summary>
    /// Logical AND / minimum-based combination.
    /// </summary>
    And,

    /// <summary>
    /// Logical OR / maximum-based combination.
    /// </summary>
    Or
}
