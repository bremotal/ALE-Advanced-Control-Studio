using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Rules;

namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// Mamdani-style fuzzy inference engine.
/// </summary>
/// <remarks>
/// Implements the standard Mamdani inference process:
/// 1. For each rule, compute the activation degree (T-norm MIN over antecedents,
///    or S-norm MAX if the rule uses OR).
/// 2. Clip each consequent membership function by its rule's activation degree
///    (implication = MIN, the standard Mamdani implication).
/// 3. Aggregate via S-norm MAX (the defuzzifier iterates the union internally).
/// </remarks>
public sealed class MamdaniInferenceEngine : IFuzzyInferenceEngine
{
    /// <inheritdoc />
    public InferenceEngineType Type => InferenceEngineType.Mamdani;

    /// <inheritdoc />
    public double Evaluate(Dictionary<string, double> inputs, IEnumerable<FuzzyRule> rules)
    {
        // Backward-compatibility shim: Mamdani cannot produce a crisp value
        // without a defuzzifier. Callers should use EvaluateActivated + a
        // defuzzifier instead. Returns 0.0 to preserve the historical contract.
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(rules);
        return 0.0;
    }

    /// <inheritdoc />
    public List<(IMembershipFunction Function, double Weight)> EvaluateActivated(
        Dictionary<string, double> inputs,
        IEnumerable<FuzzyRule> rules)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(rules);

        var activated = new List<(IMembershipFunction Function, double Weight)>();

        foreach (var rule in rules)
        {
            if (rule is null || rule.Antecedents.Count == 0 || rule.Consequent is null)
            {
                continue;
            }

            var activation = EvaluateAntecedents(inputs, rule);
            if (activation <= 0.0)
            {
                continue;
            }

            activated.Add((rule.Consequent.Term.MembershipFunction, activation));
        }

        return activated;
    }

    private static double EvaluateAntecedents(Dictionary<string, double> inputs, FuzzyRule rule)
    {
        // AND starts at 1.0 (identity for MIN); OR starts at 0.0 (identity for MAX).
        double value = rule.Operator == FuzzyOperator.And ? 1.0 : 0.0;

        foreach (var clause in rule.Antecedents)
        {
            if (!inputs.TryGetValue(clause.Variable.Name, out var crispInput))
            {
                // Missing input: rule cannot fire.
                return 0.0;
            }

            var membership = clause.Term.Evaluate(crispInput);
            if (clause.IsNegated)
            {
                membership = 1.0 - membership;
            }

            value = rule.Operator switch
            {
                FuzzyOperator.And => Math.Min(value, membership),
                FuzzyOperator.Or => Math.Max(value, membership),
                _ => value
            };
        }

        return value;
    }
}
