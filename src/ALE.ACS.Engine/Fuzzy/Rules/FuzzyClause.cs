using ALE.ACS.Engine.Fuzzy.Variables;

namespace ALE.ACS.Engine.Fuzzy.Rules;

/// <summary>
/// Represents a single fuzzy condition, such as "Temperature IS High".
/// </summary>
public sealed class FuzzyClause
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyClause"/> class.
    /// </summary>
    /// <param name="variable">The fuzzy variable referenced by the clause.</param>
    /// <param name="term">The fuzzy term referenced by the clause.</param>
    /// <param name="isNegated">Indicates whether the clause is negated.</param>
    public FuzzyClause(FuzzyVariable variable, FuzzyTerm term, bool isNegated = false)
    {
        Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        Term = term ?? throw new ArgumentNullException(nameof(term));

        if (!variable.Terms.Contains(term))
        {
            throw new ArgumentException("The supplied term does not belong to the provided variable.", nameof(term));
        }

        IsNegated = isNegated;
    }

    /// <summary>
    /// Gets the fuzzy variable referenced by the clause.
    /// </summary>
    public FuzzyVariable Variable { get; }

    /// <summary>
    /// Gets the fuzzy term referenced by the clause.
    /// </summary>
    public FuzzyTerm Term { get; }

    /// <summary>
    /// Gets a value indicating whether the clause is negated.
    /// </summary>
    public bool IsNegated { get; }
}
