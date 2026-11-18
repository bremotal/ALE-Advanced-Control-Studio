using ALE.ACS.Application.Interfaces;
using ALE.ACS.Engine.Core.Projects;
using ALE.ACS.Infrastructure.FileSystem;
using ALE.ACS.Infrastructure.Persistence;
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
        this IServiceCollection services,
        string? projectStoreDirectory = null)
    {
        var baseDirectory = projectStoreDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ALE.ACS",
                "Projects");

        // File System
        services.AddSingleton<IFileSystem, PhysicalFileSystem>();

        // Persistence (Core/Projects provider concreto + repositório de projetos)
        services.AddSingleton<IPersistenceProvider>(sp =>
            new FilePersistenceProvider(
                sp.GetRequiredService<IFileSystem>(),
                baseDirectory));
        services.AddSingleton<IProjectRepository, JsonProjectRepository>();

        // Serialization: o Core/Common/Serialization tem apenas o manager;
        // os serializers concretos vivem em Engine/Fuzzy/Serialization.
        // Para persistência de projetos usamos System.Text.Json diretamente
        // no JsonProjectRepository — não há registro extra aqui.

        // Logging: a Presentation usa Microsoft.Extensions.Logging via host.
        // Nada a registrar aqui (logger é injetado onde preciso).

        // Configuration: sem config externa por enquanto.

        return services;
    }
}
