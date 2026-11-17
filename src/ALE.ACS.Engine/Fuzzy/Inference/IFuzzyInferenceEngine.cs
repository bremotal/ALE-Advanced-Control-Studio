using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
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
    /// <remarks>
    /// For Sugeno engines this returns the weighted-average crisp output.
    /// For Mamdani engines this method is kept for backward compatibility
    /// and returns 0.0; use <see cref="EvaluateActivated"/> plus a defuzzifier
    /// to obtain a real crisp value for Mamdani.
    /// </remarks>
    double Evaluate(Dictionary<string, double> inputs, IEnumerable<FuzzyRule> rules);

    /// <summary>
    /// Evaluates the rules and returns the activated consequent membership
    /// functions along with their activation weights, ready for defuzzification.
    /// </summary>
    /// <param name="inputs">Map of variable names to crisp input values.</param>
    /// <param name="rules">The fuzzy rules to evaluate.</param>
    /// <returns>
    /// A list of (membership function, activation weight) pairs for the
    /// consequent of each rule whose activation is greater than zero.
    /// </returns>
    /// <remarks>
    /// Mamdani engines implement this to expose the aggregated fuzzy set
    /// (each consequent MF clipped by its rule's activation degree).
    /// Sugeno engines throw <see cref="NotSupportedException"/> because they
    /// return crisp outputs directly via <see cref="Evaluate"/>.
    /// </remarks>
    List<(IMembershipFunction Function, double Weight)> EvaluateActivated(
        Dictionary<string, double> inputs,
        IEnumerable<FuzzyRule> rules);
}
