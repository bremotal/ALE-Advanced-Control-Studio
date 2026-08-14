namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Provides shared context for dependency resolution.
/// </summary>
public sealed class DependencyContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyContext"/> class.
    /// </summary>
    /// <param name="services">The registered services.</param>
    public DependencyContext(IReadOnlyCollection<ServiceDescriptor>? services)
    {
        Services = services ?? Array.Empty<ServiceDescriptor>();
    }

    /// <summary>
    /// Gets the service descriptors.
    /// </summary>
    public IReadOnlyCollection<ServiceDescriptor> Services { get; }
}
