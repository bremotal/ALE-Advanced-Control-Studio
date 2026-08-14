namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Defines a persistence provider for engineering projects.
/// </summary>
public interface IPersistenceProvider
{
    /// <summary>
    /// Saves the provided project snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="options">The save options.</param>
    /// <returns>The repository result.</returns>
    RepositoryResult Save(ProjectSnapshot snapshot, SaveOptions options);

    /// <summary>
    /// Loads a project snapshot by identifier.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="options">The load options.</param>
    /// <returns>The repository result.</returns>
    RepositoryResult Load(string id, LoadOptions options);

    /// <summary>
    /// Determines whether a project snapshot exists.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <returns>True when the project exists.</returns>
    bool Exists(string id);

    /// <summary>
    /// Deletes a project snapshot.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="options">The delete options.</param>
    /// <returns>The repository result.</returns>
    RepositoryResult Delete(string id, DeleteOptions options);

    /// <summary>
    /// Validates a project snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to validate.</param>
    /// <returns>The repository result.</returns>
    RepositoryResult Validate(ProjectSnapshot snapshot);
}
