namespace ALE.ACS.Engine.Core.Plugins;

/// <summary>
/// Coordinates the lifecycle of engineering plugins.
/// </summary>
public sealed class PluginManager
{
    private readonly Dictionary<string, IEngineeringPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, PluginState> _states = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a plugin.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    public void Register(IEngineeringPlugin plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        _plugins[plugin.Descriptor.Id] = plugin;
        _states[plugin.Descriptor.Id] = PluginState.Registered;
    }

    /// <summary>
    /// Initializes all registered plugins.
    /// </summary>
    public void InitializeAll()
    {
        foreach (var plugin in _plugins.Values)
        {
            plugin.Initialize();
            _states[plugin.Descriptor.Id] = PluginState.Initialized;
        }
    }

    /// <summary>
    /// Shuts down all registered plugins.
    /// </summary>
    public void ShutdownAll()
    {
        foreach (var plugin in _plugins.Values)
        {
            plugin.Shutdown();
            _states[plugin.Descriptor.Id] = PluginState.ShutDown;
        }
    }

    /// <summary>
    /// Lists all registered plugins.
    /// </summary>
    /// <returns>The registered plugins.</returns>
    public IReadOnlyCollection<IEngineeringPlugin> ListPlugins() => _plugins.Values.ToList();

    /// <summary>
    /// Gets the state of a registered plugin.
    /// </summary>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <returns>The current plugin state.</returns>
    public PluginState GetPluginState(string pluginId)
        => _states.TryGetValue(pluginId, out var state) ? state : PluginState.Registered;
}
