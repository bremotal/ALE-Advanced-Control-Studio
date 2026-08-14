namespace ALE.ACS.Engine.Fuzzy.Expressions;

/// <summary>
/// Compiles a fuzzy expression AST into an evaluable rule delegate.
/// </summary>
public sealed class FuzzyRuleCompiler
{
    /// <summary>
    /// Compiles the supplied AST into a reusable compiled rule.
    /// </summary>
    /// <param name="expression">The expression node to compile.</param>
    /// <returns>A compiled fuzzy rule.</returns>
    public CompiledFuzzyRule Compile(FuzzyExpressionNode expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return new CompiledFuzzyRule(expression, CompileNode(expression));
    }

    private static Func<IReadOnlyDictionary<string, double>, double> CompileNode(FuzzyExpressionNode expression)
    {
        return expression switch
        {
            OperatorNode { Operator: "AND" } op => inputs =>
            {
                var values = op.Children.Select(child => CompileNode(child)(inputs)).ToArray();
                return values.Length == 0 ? 0.0 : values.Min();
            },
            OperatorNode { Operator: "OR" } op => inputs =>
            {
                var values = op.Children.Select(child => CompileNode(child)(inputs)).ToArray();
                return values.Length == 0 ? 0.0 : values.Min();
            },
            OperatorNode { Operator: "NOT" } op => inputs =>
            {
                var childValue = CompileNode(op.Children[0])(inputs);
                return 1.0 - childValue;
            },
            ClauseNode clause => inputs =>
            {
                var key = $"{clause.VariableName}:{clause.TermName}";
                return inputs.TryGetValue(key, out var value) ? value : 0.0;
            },
            ConstantNode constant => _ => constant.Value,
            VariableNode variable => inputs => inputs.TryGetValue(variable.Name, out var value) ? value : 0.0,
            _ => _ => 0.0
        };
    }
}

/// <summary>
/// Represents a compiled fuzzy rule that can be evaluated efficiently.
/// </summary>
public sealed class CompiledFuzzyRule
{
    private readonly Func<IReadOnlyDictionary<string, double>, double> _evaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledFuzzyRule"/> class.
    /// </summary>
    /// <param name="expression">The AST expression.</param>
    /// <param name="evaluator">The compiled evaluator.</param>
    public CompiledFuzzyRule(FuzzyExpressionNode expression, Func<IReadOnlyDictionary<string, double>, double> evaluator)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    /// <summary>
    /// Gets the expression root node.
    /// </summary>
    public FuzzyExpressionNode Expression { get; }

    /// <summary>
    /// Evaluates the compiled rule against the supplied degrees.
    /// </summary>
    /// <param name="inputs">The rule input degrees.</param>
    /// <returns>The resulting degree.</returns>
    public double Evaluate(IReadOnlyDictionary<string, double> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        return _evaluator(inputs);
    }
}
