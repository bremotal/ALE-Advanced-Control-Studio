namespace ALE.ACS.Engine.Fuzzy.Plugins;

/// <summary>
/// Plugin that registers a custom fuzzy logical operator.
/// </summary>
public sealed class CustomRuleOperatorPlugin : IFuzzyPlugin
{
    private readonly Func<double, double, double> _operator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRuleOperatorPlugin"/> class.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <param name="op">Custom logical operator delegate.</param>
    public CustomRuleOperatorPlugin(string name, Func<double, double, double> op)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _operator = op ?? throw new ArgumentNullException(nameof(op));

        Name = name;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Evaluates the custom logical operator.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>Operator result.</returns>
    public double Evaluate(double left, double right) => _operator(left, right);
}
