using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Infrastructure.DependencyInjection;

/// <summary>
/// Registra os serviços da camada de infraestrutura.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}