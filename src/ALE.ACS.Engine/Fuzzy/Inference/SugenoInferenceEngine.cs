using ALE.ACS.Engine.Fuzzy.Rules;

namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// Implements Sugeno-style fuzzy inference with weighted average aggregation.
/// </summary>
public sealed class SugenoInferenceEngine : IFuzzyInferenceEngine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SugenoInferenceEngine"/> class.
    /// </summary>
    /// <param name="outputFunction">The Sugeno output function used by the engine.</param>
    public SugenoInferenceEngine(SugenoOutputFunction outputFunction)
    {
        OutputFunction = outputFunction ?? throw new ArgumentNullException(nameof(outputFunction));
    }

    /// <inheritdoc />
    public InferenceEngineType Type => InferenceEngineType.Sugeno;

    /// <summary>
    /// Gets the Sugeno output function used by the engine.
    /// </summary>
    public SugenoOutputFunction OutputFunction { get; }

    /// <inheritdoc />
    public double Evaluate(Dictionary<string, double> inputs, IEnumerable<FuzzyRule> rules)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(rules);

        double weightedSum = 0.0;
        double weightSum = 0.0;

        foreach (var rule in rules)
        {
            if (rule is null || rule.Antecedents.Count == 0)
            {
                continue;
            }

            var activation = EvaluateAntecedents(inputs, rule);
            var crispInput = EvaluateCrispInput(inputs, rule);
            var consequentOutput = OutputFunction.Evaluate(crispInput);

            weightedSum += activation * consequentOutput;
            weightSum += activation;
        }

        return Math.Abs(weightSum) < 1e-12 ? 0.0 : weightedSum / weightSum;
    }

    private static double EvaluateAntecedents(Dictionary<string, double> inputs, FuzzyRule rule)
    {
        double value = 1.0;

        foreach (var clause in rule.Antecedents)
        {
            var crispInput = inputs[clause.Variable.Name];
            var membership = clause.Term.Evaluate(crispInput);
            value = rule.Operator switch
            {
                FuzzyOperator.And => Math.Min(value, membership),
                FuzzyOperator.Or => Math.Max(value, membership),
                _ => value
            };
        }

        return value;
    }

    private static double EvaluateCrispInput(Dictionary<string, double> inputs, FuzzyRule rule)
    {
        if (rule.Consequent is null)
        {
            return 0.0;
        }

        var consequentVariableName = rule.Consequent.Variable.Name;
        return inputs.TryGetValue(consequentVariableName, out var value) ? value : 0.0;
    }
}
