namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Represents an execution graph composed by execution nodes.
/// </summary>
public sealed class ExecutionGraph
{
    private readonly List<ExecutionNode> _nodes = [];

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddNode(ExecutionNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        _nodes.Add(node);
    }

    /// <summary>
    /// Gets the nodes contained in the graph.
    /// </summary>
    public IReadOnlyCollection<ExecutionNode> Nodes => _nodes;
}
