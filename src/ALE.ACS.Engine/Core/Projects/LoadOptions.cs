namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Defines options used when loading a project snapshot.
/// </summary>
public sealed class LoadOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadOptions"/> class.
    /// </summary>
    /// <param name="provider">The persistence provider name.</param>
    public LoadOptions(string provider)
    {
        Provider = provider;
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider { get; }
}
