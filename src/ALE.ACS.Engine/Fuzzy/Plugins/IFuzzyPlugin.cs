namespace ALE.ACS.Engine.Fuzzy.Plugins;

/// <summary>
/// Base contract for fuzzy engine plugins.
/// </summary>
public interface IFuzzyPlugin
{
    /// <summary>
    /// Gets the unique plugin name.
    /// </summary>
    string Name { get; }
}
