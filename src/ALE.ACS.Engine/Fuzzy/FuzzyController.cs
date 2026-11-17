using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.Inference;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;

namespace ALE.ACS.Engine.Fuzzy;

/// <summary>
/// Public facade for configuring and executing fuzzy controllers.
/// </summary>
public sealed class FuzzyController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyController"/> class.
    /// </summary>
    public FuzzyController(
        string name,
        List<FuzzyVariable> inputs,
        FuzzyVariable output,
        List<FuzzyRule> rules,
        IFuzzyInferenceEngine inferenceEngine,
        IDefuzzifier? defuzzifier = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(inferenceEngine);

        if (inputs.Count == 0)
        {
            throw new ArgumentException("At least one input variable is required.", nameof(inputs));
        }

        if (rules.Count == 0)
        {
            throw new ArgumentException("At least one rule is required.", nameof(rules));
        }

        if (inferenceEngine.Type == InferenceEngineType.Mamdani && defuzzifier is null)
        {
            throw new ArgumentException("A defuzzifier is required when using the Mamdani inference engine.", nameof(defuzzifier));
        }

        Name = name;
        Inputs = inputs.AsReadOnly();
        Output = output;
        Rules = rules.AsReadOnly();
        InferenceEngine = inferenceEngine;
        Defuzzifier = defuzzifier;
        EngineType = inferenceEngine.Type;
    }

    /// <summary>
    /// Gets the controller name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the configured input variables.
    /// </summary>
    public IReadOnlyList<FuzzyVariable> Inputs { get; }

    /// <summary>
    /// Gets the configured output variable.
    /// </summary>
    public FuzzyVariable Output { get; }

    /// <summary>
    /// Gets the configured fuzzy rules.
    /// </summary>
    public IReadOnlyList<FuzzyRule> Rules { get; }

    /// <summary>
    /// Gets the configured inference engine type.
    /// </summary>
    public InferenceEngineType EngineType { get; }

    /// <summary>
    /// Gets the configured inference engine.
    /// </summary>
    public IFuzzyInferenceEngine InferenceEngine { get; }

    /// <summary>
    /// Gets the configured defuzzifier, if any.
    /// </summary>
    public IDefuzzifier? Defuzzifier { get; }

    /// <summary>
    /// Evaluates the fuzzy controller for the supplied crisp inputs.
    /// </summary>
    public double Evaluate(Dictionary<string, double> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Count != Inputs.Count)
        {
            throw new ArgumentException("The number of supplied inputs does not match the configured input variables.", nameof(inputs));
        }

        foreach (var variable in Inputs)
        {
            if (!inputs.ContainsKey(variable.Name))
            {
                throw new ArgumentException($"Missing input for variable '{variable.Name}'.", nameof(inputs));
            }
        }

        // Sugeno returns crisp directly.
        if (EngineType == InferenceEngineType.Sugeno)
        {
            return InferenceEngine.Evaluate(inputs, Rules);
        }

        // Mamdani: compute the activated consequent MFs + weights, then
        // defuzzify over the REAL output range (not the old fixed [0,100]).
        if (Defuzzifier is null)
        {
            return 0.0;
        }

        var activatedFunctions = InferenceEngine.EvaluateActivated(inputs, Rules);
        if (activatedFunctions.Count == 0)
        {
            // Nenhuma regra disparou — ponto médio do range como fallback seguro.
            var (midMin, midMax) = FuzzySpec.ComputeOutputRange(Output);
            return (midMin + midMax) * 0.5;
        }

        var (outputMin, outputMax) = FuzzySpec.ComputeOutputRange(Output);
        return Defuzzifier.Defuzzify(activatedFunctions, outputMin, outputMax);
    }
}
