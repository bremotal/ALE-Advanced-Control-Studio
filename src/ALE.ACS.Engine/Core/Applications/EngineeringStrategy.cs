namespace ALE.ACS.Engine.Core.Applications;

/// <summary>
/// Represents a reusable engineering strategy pipeline.
/// </summary>
public sealed class EngineeringStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EngineeringStrategy"/> class.
    /// </summary>
    /// <param name="id">The strategy identifier.</param>
    /// <param name="name">The strategy name.</param>
    /// <param name="description">The strategy description.</param>
    /// <param name="blocks">The blocks that compose the strategy.</param>
    public EngineeringStrategy(Guid id, string name, string? description, IReadOnlyCollection<EngineeringBlock>? blocks)
    {
        Id = id;
        Name = name;
        Description = description;
        Blocks = blocks ?? [];
    }

    /// <summary>
    /// Gets the unique identifier of the strategy.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the strategy name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional description of the strategy.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the blocks that compose the strategy.
    /// </summary>
    public IReadOnlyCollection<EngineeringBlock> Blocks { get; }
}
