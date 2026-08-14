namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Represents a node inside an execution graph.
/// </summary>
public sealed class ExecutionNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionNode"/> class.
    /// </summary>
    /// <param name="blockId">The block identifier.</param>
    /// <param name="inputs">The input names.</param>
    /// <param name="outputs">The output names.</param>
    /// <param name="dependencies">The dependency identifiers.</param>
    public ExecutionNode(Guid blockId, IReadOnlyCollection<string>? inputs, IReadOnlyCollection<string>? outputs, IReadOnlyCollection<Guid>? dependencies)
    {
        BlockId = blockId;
        Inputs = inputs ?? Array.Empty<string>();
        Outputs = outputs ?? Array.Empty<string>();
        Dependencies = dependencies ?? Array.Empty<Guid>();
    }

    /// <summary>
    /// Gets the associated block identifier.
    /// </summary>
    public Guid BlockId { get; }

    /// <summary>
    /// Gets the input names.
    /// </summary>
    public IReadOnlyCollection<string> Inputs { get; }

    /// <summary>
    /// Gets the output names.
    /// </summary>
    public IReadOnlyCollection<string> Outputs { get; }

    /// <summary>
    /// Gets the dependencies of the node.
    /// </summary>
    public IReadOnlyCollection<Guid> Dependencies { get; }
}
