using ALE.ACS.Engine.Fuzzy.Expressions;

namespace ALE.ACS.Engine.Fuzzy.Runtime;

/// <summary>
/// Represents a compiled fuzzy controller that can evaluate a rule set.
/// </summary>
public sealed class CompiledController
{
    private readonly IReadOnlyList<CompiledFuzzyRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledController"/> class.
    /// </summary>
    /// <param name="rules">The compiled fuzzy rules.</param>
    public CompiledController(IEnumerable<CompiledFuzzyRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = [.. rules.Where(static rule => rule is not null)];
    }

    /// <summary>
    /// Executes the compiled rule set against the supplied inputs.
    /// </summary>
    /// <param name="inputs">The input dictionary.</param>
    /// <returns>The aggregated controller output.</returns>
    public double Execute(IReadOnlyDictionary<string, double> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        return _rules.Count == 0
            ? 0.0
            : _rules.Select(rule => rule.Evaluate(inputs)).DefaultIfEmpty(0.0).Max();
    }
}
