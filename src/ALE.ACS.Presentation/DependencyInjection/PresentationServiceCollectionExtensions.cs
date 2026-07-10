using ALE.ACS.Presentation.Shell.Dialogs.Projects;
using ALE.ACS.Presentation.ViewModels;
using ALE.ACS.Presentation.ViewModels.Projects;
using ALE.ACS.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Presentation.DependencyInjection;

/// <summary>
/// Registra os serviços da camada de apresentação.
/// </summary>
public static class PresentationServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddTransient<NewProjectDialog>();
        services.AddTransient<NewProjectViewModel>();

        return services;
    }
}