using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Fuzzy.Plugins;

/// <summary>
/// Plugin that registers a custom membership function factory.
/// </summary>
public sealed class CustomMembershipFunctionPlugin : IFuzzyPlugin
{
    private readonly Func<IMembershipFunction> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomMembershipFunctionPlugin"/> class.
    /// </summary>
    /// <param name="name">Plugin name.</param>
    /// <param name="factory">Membership function factory.</param>
    public CustomMembershipFunctionPlugin(string name, Func<IMembershipFunction> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        Name = name;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Creates a membership function instance.
    /// </summary>
    /// <returns>The created membership function.</returns>
    public IMembershipFunction Create() => _factory();
}
