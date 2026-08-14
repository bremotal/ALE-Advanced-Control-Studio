using ALE.ACS.Engine.Fuzzy.Rules;

namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// Defines the contract for fuzzy inference engines.
/// </summary>
public interface IFuzzyInferenceEngine
{
    /// <summary>
    /// Gets the engine type.
    /// </summary>
    InferenceEngineType Type { get; }

    /// <summary>
    /// Evaluates the supplied fuzzy rules for the provided crisp inputs.
    /// </summary>
    /// <param name="inputs">Map of variable names to crisp input values.</param>
    /// <param name="rules">The fuzzy rules to evaluate.</param>
    /// <returns>The resulting crisp output value.</returns>
    double Evaluate(Dictionary<string, double> inputs, IEnumerable<FuzzyRule> rules);
}
