using ALE.ACS.Engine.Fuzzy.Rules;

namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// A placeholder Mamdani inference engine.
/// </summary>
public sealed class MamdaniInferenceEngine : IFuzzyInferenceEngine
{
    /// <inheritdoc />
    public InferenceEngineType Type => InferenceEngineType.Mamdani;

    /// <inheritdoc />
    public double Evaluate(Dictionary<string, double> inputs, IEnumerable<FuzzyRule> rules)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(rules);

        return 0.0;
    }
}
