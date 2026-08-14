namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Defines options used when deleting a project snapshot.
/// </summary>
public sealed class DeleteOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOptions"/> class.
    /// </summary>
    /// <param name="provider">The persistence provider name.</param>
    public DeleteOptions(string provider)
    {
        Provider = provider;
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider { get; }
}
