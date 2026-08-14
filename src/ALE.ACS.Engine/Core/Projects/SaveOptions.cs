namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Defines options used when saving a project snapshot.
/// </summary>
public sealed class SaveOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveOptions"/> class.
    /// </summary>
    /// <param name="provider">The persistence provider name.</param>
    public SaveOptions(string provider)
    {
        Provider = provider;
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider { get; }
}
