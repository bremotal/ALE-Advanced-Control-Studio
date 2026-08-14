using ALE.ACS.Application.Interfaces;
using ALE.ACS.Application.EventBus;
using ALE.ACS.Application.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Application.DependencyInjection;

/// <summary>
/// Registra os serviços da camada Application.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        return services;
    }
}