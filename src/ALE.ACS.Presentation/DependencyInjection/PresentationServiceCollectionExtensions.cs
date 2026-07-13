using ALE.ACS.Application.Interfaces;
using ALE.ACS.Application.Projects;
using ALE.ACS.Presentation.Services.Dialogs;
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
        // Serviços da Presentation
        services.AddSingleton<IDialogService, DialogService>();

        // Serviços da Application
        services.AddSingleton<IProjectService, ProjectService>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<NewProjectViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<NewProjectDialog>();

        return services;
    }
}