namespace ALE.ACS.Engine.Fuzzy.Expressions;

/// <summary>
/// Represents a logical operator node such as AND, OR or NOT.
/// </summary>
public sealed class OperatorNode : FuzzyExpressionNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperatorNode"/> class.
    /// </summary>
    /// <param name="op">The operator symbol.</param>
    /// <param name="children">The child expressions.</param>
    public OperatorNode(string op, IEnumerable<FuzzyExpressionNode> children)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(op);
        ArgumentNullException.ThrowIfNull(children);

        Operator = op.ToUpperInvariant();
        Children = [.. children.Where(static child => child is not null)];
    }

    /// <summary>
    /// Gets the logical operator.
    /// </summary>
    public string Operator { get; }

    /// <summary>
    /// Gets the child expressions.
    /// </summary>
    public IReadOnlyList<FuzzyExpressionNode> Children { get; }
}
