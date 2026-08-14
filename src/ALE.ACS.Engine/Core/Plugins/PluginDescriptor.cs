namespace ALE.ACS.Engine.Core.Plugins;

/// <summary>
/// Describes an engineering plugin.
/// </summary>
public sealed class PluginDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginDescriptor"/> class.
    /// </summary>
    /// <param name="id">The plugin identifier.</param>
    /// <param name="name">The plugin name.</param>
    /// <param name="version">The plugin version.</param>
    /// <param name="author">The plugin author.</param>
    /// <param name="description">The plugin description.</param>
    /// <param name="capabilities">The plugin capabilities.</param>
    public PluginDescriptor(string id, string name, string version, string author, string description, IReadOnlyCollection<string> capabilities)
    {
        Id = id;
        Name = name;
        Version = version;
        Author = author;
        Description = description;
        Capabilities = capabilities;
    }

    /// <summary>
    /// Gets the plugin unique identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the plugin name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the plugin version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the plugin author.
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// Gets the plugin description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the plugin capabilities.
    /// </summary>
    public IReadOnlyCollection<string> Capabilities { get; }
}
