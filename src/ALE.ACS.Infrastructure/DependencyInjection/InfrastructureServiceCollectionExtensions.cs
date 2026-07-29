using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Infrastructure.DependencyInjection;

/// <summary>
/// Registra os serviços da camada de infraestrutura.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos os serviços da infraestrutura.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        // File System

        // Persistence

        // Serialization

        // Logging

        // Configuration

        return services;
    }
}