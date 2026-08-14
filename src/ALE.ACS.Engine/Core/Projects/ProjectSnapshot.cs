namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Represents a persisted snapshot of an engineering project.
/// </summary>
public sealed class ProjectSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectSnapshot"/> class.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="name">The project name.</param>
    /// <param name="version">The project version.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="updatedAt">The update timestamp.</param>
    /// <param name="payload">The serialized payload.</param>
    /// <param name="metadata">The metadata.</param>
    public ProjectSnapshot(string id, string name, string version, DateTime createdAt, DateTime updatedAt, string payload, IReadOnlyDictionary<string, string> metadata)
    {
        Id = id;
        Name = name;
        Version = version;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Payload = payload;
        Metadata = metadata;
    }

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the project version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Gets the serialized payload.
    /// </summary>
    public string Payload { get; }

    /// <summary>
    /// Gets the metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
