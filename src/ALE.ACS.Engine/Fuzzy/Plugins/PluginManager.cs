using System.Collections.Concurrent;

namespace ALE.ACS.Engine.Fuzzy.Plugins;

/// <summary>
/// Discovers and manages fuzzy plugins at runtime.
/// </summary>
public sealed class PluginManager
{
    private readonly ConcurrentDictionary<string, CustomMembershipFunctionPlugin> _membershipPlugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CustomDefuzzifierPlugin> _defuzzifierPlugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CustomRuleOperatorPlugin> _operatorPlugins = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a membership function plugin.
    /// </summary>
    /// <param name="plugin">Plugin instance.</param>
    public void RegisterMembershipFunctionPlugin(CustomMembershipFunctionPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _membershipPlugins[plugin.Name] = plugin;
    }

    /// <summary>
    /// Registers a defuzzifier plugin.
    /// </summary>
    /// <param name="plugin">Plugin instance.</param>
    public void RegisterDefuzzifierPlugin(CustomDefuzzifierPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _defuzzifierPlugins[plugin.Name] = plugin;
    }

    /// <summary>
    /// Registers a custom rule operator plugin.
    /// </summary>
    /// <param name="plugin">Plugin instance.</param>
    public void RegisterRuleOperatorPlugin(CustomRuleOperatorPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        _operatorPlugins[plugin.Name] = plugin;
    }

    /// <summary>
    /// Attempts to retrieve a membership function plugin by name.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <param name="plugin">Resolved plugin when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    public bool TryGetMembershipFunctionPlugin(string name, out CustomMembershipFunctionPlugin? plugin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _membershipPlugins.TryGetValue(name, out plugin);
    }

    /// <summary>
    /// Attempts to retrieve a defuzzifier plugin by name.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <param name="plugin">Resolved plugin when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    public bool TryGetDefuzzifierPlugin(string name, out CustomDefuzzifierPlugin? plugin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _defuzzifierPlugins.TryGetValue(name, out plugin);
    }

    /// <summary>
    /// Attempts to retrieve a rule operator plugin by name.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <param name="plugin">Resolved plugin when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    public bool TryGetRuleOperatorPlugin(string name, out CustomRuleOperatorPlugin? plugin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _operatorPlugins.TryGetValue(name, out plugin);
    }
}
