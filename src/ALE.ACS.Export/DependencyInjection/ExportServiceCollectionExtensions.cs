using ALE.ACS.Application.Interfaces;
using ALE.ACS.Engine.Core.Export;
using ALE.ACS.Export.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Export.DependencyInjection;

/// <summary>
/// Registra os serviços da camada de exportação.
/// </summary>
public static class ExportServiceCollectionExtensions
{
    /// <summary>
    /// Registra providers de exportação e o <see cref="ExportManager"/>.
    /// </summary>
    public static IServiceCollection AddExport(this IServiceCollection services)
    {
        // Providers concretos
        services.AddSingleton<JsonExportProvider>();
        services.AddSingleton<PlcCodeExportProvider>();

        // Registro como IExportProvider para o ExportManager descobrir via DI
        services.AddSingleton<IExportProvider>(sp => sp.GetRequiredService<JsonExportProvider>());
        services.AddSingleton<IExportProvider>(sp => sp.GetRequiredService<PlcCodeExportProvider>());

        // ExportManager recebe a coleção de providers via DI
        services.AddSingleton<ExportManager>();

        return services;
    }
}
