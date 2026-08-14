namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Stores registration metadata for a service descriptor.
/// </summary>
public sealed class ServiceRegistration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRegistration"/> class.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <param name="lifetime">The service lifetime.</param>
    public ServiceRegistration(Type serviceType, Type? implementationType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        Lifetime = lifetime;
    }

    /// <summary>
    /// Gets the service type.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Gets the implementation type.
    /// </summary>
    public Type? ImplementationType { get; }

    /// <summary>
    /// Gets the service lifetime.
    /// </summary>
    public ServiceLifetime Lifetime { get; }
}
