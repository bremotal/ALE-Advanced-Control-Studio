namespace ALE.ACS.Engine.Core.Connections;

/// <summary>
/// Represents a connection between engineering variables or blocks.
/// </summary>
public sealed class EngineeringConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EngineeringConnection"/> class.
    /// </summary>
    /// <param name="id">The connection identifier.</param>
    /// <param name="sourceId">The source identifier.</param>
    /// <param name="targetId">The target identifier.</param>
    public EngineeringConnection(Guid id, Guid sourceId, Guid targetId)
    {
        Id = id;
        SourceId = sourceId;
        TargetId = targetId;
    }

    /// <summary>
    /// Gets the connection identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the source identifier.
    /// </summary>
    public Guid SourceId { get; }

    /// <summary>
    /// Gets the target identifier.
    /// </summary>
    public Guid TargetId { get; }
}
