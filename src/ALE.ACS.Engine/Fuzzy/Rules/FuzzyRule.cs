using System.Collections.ObjectModel;

namespace ALE.ACS.Engine.Fuzzy.Rules;

/// <summary>
/// Represents a complete fuzzy rule with antecedents and a consequent.
/// </summary>
public sealed class FuzzyRule
{
    private readonly List<FuzzyClause> _antecedents = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyRule"/> class.
    /// </summary>
    /// <param name="antecedents">The list of antecedent clauses.</param>
    /// <param name="op">The logical operator used to combine antecedents.</param>
    /// <param name="consequent">The consequent clause.</param>
    public FuzzyRule(IEnumerable<FuzzyClause> antecedents, FuzzyOperator op, FuzzyClause consequent)
    {
        ArgumentNullException.ThrowIfNull(antecedents);
        ArgumentNullException.ThrowIfNull(consequent);

        _antecedents.AddRange(antecedents.Where(a => a is not null));

        if (_antecedents.Count == 0)
        {
            throw new ArgumentException("A fuzzy rule must contain at least one antecedent.", nameof(antecedents));
        }

        Operator = op;
        Consequent = consequent;
    }

    /// <summary>
    /// Gets the antecedent clauses of the rule.
    /// </summary>
    public IReadOnlyList<FuzzyClause> Antecedents => _antecedents.AsReadOnly();

    /// <summary>
    /// Gets the logical operator used to combine the antecedents.
    /// </summary>
    public FuzzyOperator Operator { get; }

    /// <summary>
    /// Gets the consequent clause of the rule.
    /// </summary>
    public FuzzyClause Consequent { get; }
}
