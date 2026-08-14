namespace ALE.ACS.Engine.Core.Applications;

/// <summary>
/// Represents a reusable engineering project model for the core framework.
/// </summary>
public sealed class EngineeringProject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EngineeringProject"/> class.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="name">The project name.</param>
    /// <param name="description">The project description.</param>
    /// <param name="version">The project version.</param>
    /// <param name="applications">The engineering applications.</param>
    /// <param name="metadata">The metadata associated with the project.</param>
    public EngineeringProject(
        Guid id,
        string name,
        string? description,
        string version,
        IReadOnlyCollection<ControlApplication>? applications,
        IReadOnlyDictionary<string, string>? metadata)
    {
        Id = id;
        Name = name;
        Description = description;
        Version = version;
        Applications = applications ?? [];
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the unique identifier for the project.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional project description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the project version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the engineering applications covered by the project.
    /// </summary>
    public IReadOnlyCollection<ControlApplication> Applications { get; }

    /// <summary>
    /// Gets the metadata associated with the project.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
