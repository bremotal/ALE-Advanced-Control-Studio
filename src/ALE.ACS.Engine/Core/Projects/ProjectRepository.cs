using ALE.ACS.Engine.Core.Applications;

namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Responsible for EngineeringProject persistence operations.
/// </summary>
public sealed class ProjectRepository
{
    private readonly PersistenceManager _persistenceManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
    /// </summary>
    /// <param name="persistenceManager">The persistence manager.</param>
    public ProjectRepository(PersistenceManager persistenceManager)
    {
        _persistenceManager = persistenceManager ?? throw new ArgumentNullException(nameof(persistenceManager));
    }

    /// <summary>
    /// Saves the supplied engineering project.
    /// </summary>
    /// <param name="project">The engineering project.</param>
    /// <param name="options">The save options.</param>
    /// <returns>The repository result.</returns>
    public RepositoryResult Save(EngineeringProject project, SaveOptions options)
    {
        var snapshot = new ProjectSnapshot(project.Id.ToString(), project.Name, project.Version, DateTime.UtcNow, DateTime.UtcNow, "", new Dictionary<string, string>(project.Metadata));
        return _persistenceManager.Save(snapshot, options);
    }

    /// <summary>
    /// Loads a project snapshot.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="options">The load options.</param>
    /// <returns>The repository result.</returns>
    public RepositoryResult Load(string id, LoadOptions options)
        => _persistenceManager.Load(id, options);

    /// <summary>
    /// Deletes a project snapshot.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="options">The delete options.</param>
    /// <returns>The repository result.</returns>
    public RepositoryResult Delete(string id, DeleteOptions options)
        => _persistenceManager.Delete(id, options);
}
