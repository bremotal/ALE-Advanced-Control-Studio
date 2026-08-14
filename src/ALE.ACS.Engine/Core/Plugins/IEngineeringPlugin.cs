namespace ALE.ACS.Engine.Core.Plugins;

/// <summary>
/// Represents a plugin that can be initialized and shut down.
/// </summary>
public interface IEngineeringPlugin
{
    /// <summary>
    /// Gets the plugin descriptor.
    /// </summary>
    PluginDescriptor Descriptor { get; }

    /// <summary>
    /// Initializes the plugin.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Shuts down the plugin.
    /// </summary>
    void Shutdown();
}
