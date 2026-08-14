namespace ALE.ACS.Engine.Core.Applications;

/// <summary>
/// Represents an engineering application or problem definition.
/// </summary>
public sealed class ControlApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControlApplication"/> class.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="name">The application name.</param>
    /// <param name="description">The application description.</param>
    /// <param name="strategies">The associated engineering strategies.</param>
    public ControlApplication(Guid id, string name, string? description, IReadOnlyCollection<EngineeringStrategy>? strategies)
    {
        Id = id;
        Name = name;
        Description = description;
        Strategies = strategies ?? [];
    }

    /// <summary>
    /// Gets the unique identifier for the application.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional description of the application.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the engineering strategies related to the application.
    /// </summary>
    public IReadOnlyCollection<EngineeringStrategy> Strategies { get; }
}
