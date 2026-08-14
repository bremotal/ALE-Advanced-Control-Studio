using ALE.ACS.Engine.Fuzzy.Defuzzification;

namespace ALE.ACS.Engine.Fuzzy.Plugins;

/// <summary>
/// Plugin that registers a custom defuzzifier factory.
/// </summary>
public sealed class CustomDefuzzifierPlugin : IFuzzyPlugin
{
    private readonly Func<IDefuzzifier> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomDefuzzifierPlugin"/> class.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <param name="factory">Defuzzifier factory.</param>
    public CustomDefuzzifierPlugin(string name, Func<IDefuzzifier> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        Name = name;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Creates a defuzzifier instance.
    /// </summary>
    /// <returns>The created defuzzifier.</returns>
    public IDefuzzifier Create() => _factory();
}
