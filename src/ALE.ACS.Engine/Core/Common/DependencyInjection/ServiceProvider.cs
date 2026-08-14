namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Resolves services registered in a service collection.
/// </summary>
public sealed class ServiceProvider
{
    private readonly ServiceCollection _services;
    private readonly Dictionary<Type, object> _singletons = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public ServiceProvider(ServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Resolves a service by type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <returns>The resolved service instance.</returns>
    public object Resolve(Type serviceType)
    {
        var descriptor = _services.Descriptors.FirstOrDefault(item => item.ServiceType == serviceType)
            ?? throw new InvalidOperationException($"No service registered for {serviceType.FullName}.");

        return descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton when _singletons.TryGetValue(serviceType, out var singleton) => singleton,
            ServiceLifetime.Singleton => CreateAndCache(serviceType, descriptor),
            ServiceLifetime.Scoped => CreateInstance(descriptor),
            ServiceLifetime.Transient => CreateInstance(descriptor),
            _ => throw new InvalidOperationException($"Unsupported lifetime {descriptor.Lifetime}.")
        };
    }

    private object CreateAndCache(Type serviceType, ServiceDescriptor descriptor)
    {
        var instance = CreateInstance(descriptor);
        _singletons[serviceType] = instance;
        return instance;
    }

    private object CreateInstance(ServiceDescriptor descriptor)
    {
        var implementationType = descriptor.ImplementationType;
        var constructor = implementationType.GetConstructors().FirstOrDefault();
        if (constructor is null)
        {
            return Activator.CreateInstance(implementationType)!;
        }

        var dependencies = constructor.GetParameters()
            .Select(parameter => Resolve(parameter.ParameterType))
            .ToArray();

        return constructor.Invoke(dependencies);
    }
}
