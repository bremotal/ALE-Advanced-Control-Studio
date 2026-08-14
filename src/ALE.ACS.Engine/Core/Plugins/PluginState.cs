namespace ALE.ACS.Engine.Core.Plugins;

/// <summary>
/// Defines the lifecycle state of a plugin.
/// </summary>
public enum PluginState
{
    /// <summary>
    /// The plugin has been registered but not initialized.
    /// </summary>
    Registered,

    /// <summary>
    /// The plugin has been initialized.
    /// </summary>
    Initialized,

    /// <summary>
    /// The plugin has been shut down.
    /// </summary>
    ShutDown
}
