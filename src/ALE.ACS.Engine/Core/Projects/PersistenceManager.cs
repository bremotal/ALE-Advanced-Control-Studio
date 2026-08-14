namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Coordinates persistence providers.
/// </summary>
public sealed class PersistenceManager
{
    private readonly IReadOnlyCollection<IPersistenceProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersistenceManager"/> class.
    /// </summary>
    /// <param name="providers">The persistence providers.</param>
    public PersistenceManager(IReadOnlyCollection<IPersistenceProvider>? providers)
    {
        _providers = providers ?? Array.Empty<IPersistenceProvider>();
    }

    /// <summary>
    /// Saves the provided snapshot using the first provider.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="options">The save options.</param>
    /// <returns>The repository result.</returns>
    public RepositoryResult Save(ProjectSnapshot snapshot, SaveOptions options)
    {
        if (_providers.Count == 0)
        {
            return RepositoryResult.CreateFailure("No persistence providers were registered.");
        }

        var provider = _providers.First();
        return provider.Save(snapshot, options);
    }

    /// <summary>
    /// Loads a snapshot using the first provider.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="options">The load options.</param>
    /// <returns>The repository result.</returns>
    public RepositoryResult Load(string id, LoadOptions options)
    {
        if (_providers.Count == 0)
        {
            return RepositoryResult.CreateFailure("No persistence providers were registered.");
        }

        var provider = _providers.First();
        return provider.Load(id, options);
    }

    /// <summary>
    /// Deletes a snapshot using the first provider.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="options">The delete options.</param>
    /// <returns>The repository result.</returns>
    public RepositoryResult Delete(string id, DeleteOptions options)
    {
        if (_providers.Count == 0)
        {
            return RepositoryResult.CreateFailure("No persistence providers were registered.");
        }

        var provider = _providers.First();
        return provider.Delete(id, options);
    }
}
