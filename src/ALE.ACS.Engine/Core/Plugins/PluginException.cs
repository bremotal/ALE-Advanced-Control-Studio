namespace ALE.ACS.Engine.Core.Plugins;

/// <summary>
/// Represents failures that occur during plugin lifecycle operations.
/// </summary>
public sealed class PluginException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public PluginException(string message) : base(message)
    {
    }
}
