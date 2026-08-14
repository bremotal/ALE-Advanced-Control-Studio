namespace ALE.ACS.Engine.Fuzzy.Expressions;

/// <summary>
/// Base class for nodes in a fuzzy rule expression AST.
/// </summary>
public abstract class FuzzyExpressionNode
{
}

/// <summary>
/// Represents a clause node such as Temperature IS High.
/// </summary>
public sealed class ClauseNode : FuzzyExpressionNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClauseNode"/> class.
    /// </summary>
    /// <param name="variableName">The variable name.</param>
    /// <param name="termName">The term name.</param>
    /// <param name="isNegated">Indicates whether the clause is negated.</param>
    public ClauseNode(string variableName, string termName, bool isNegated = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(termName);

        VariableName = variableName;
        TermName = termName;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Gets the term name.
    /// </summary>
    public string TermName { get; }

    /// <summary>
    /// Gets a value indicating whether the clause is negated.
    /// </summary>
    public bool IsNegated { get; }
}

/// <summary>
/// Represents a constant value in the expression tree.
/// </summary>
public sealed class ConstantNode : FuzzyExpressionNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstantNode"/> class.
    /// </summary>
    /// <param name="value">The constant value.</param>
    public ConstantNode(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the constant value.
    /// </summary>
    public double Value { get; }
}

/// <summary>
/// Represents a variable reference in the expression tree.
/// </summary>
public sealed class VariableNode : FuzzyExpressionNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariableNode"/> class.
    /// </summary>
    /// <param name="name">The variable reference.</param>
    public VariableNode(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Gets the variable reference name.
    /// </summary>
    public string Name { get; }
}
