namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Stores service registrations for the dependency injection framework.
/// </summary>
public sealed class ServiceCollection
{
    private readonly List<ServiceDescriptor> _descriptors = [];

    /// <summary>
    /// Adds a singleton service registration.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <returns>The current service collection.</returns>
    public ServiceCollection AddSingleton(Type serviceType, Type implementationType)
    {
        _descriptors.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Singleton));
        return this;
    }

    /// <summary>
    /// Adds a scoped service registration.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <returns>The current service collection.</returns>
    public ServiceCollection AddScoped(Type serviceType, Type implementationType)
    {
        _descriptors.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Scoped));
        return this;
    }

    /// <summary>
    /// Adds a transient service registration.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="implementationType">The implementation type.</param>
    /// <returns>The current service collection.</returns>
    public ServiceCollection AddTransient(Type serviceType, Type implementationType)
    {
        _descriptors.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
        return this;
    }

    /// <summary>
    /// Gets the registered descriptors.
    /// </summary>
    public IReadOnlyCollection<ServiceDescriptor> Descriptors => _descriptors;
}
