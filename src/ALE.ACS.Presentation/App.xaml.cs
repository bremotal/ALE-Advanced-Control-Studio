using ALE.ACS.Application.DependencyInjection;
using ALE.ACS.Export.DependencyInjection;
using ALE.ACS.Infrastructure.DependencyInjection;
using ALE.ACS.Presentation.DependencyInjection;
using ALE.ACS.Presentation.Shell.Themes;
using ALE.ACS.Presentation.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ALE.ACS.Presentation;

/// <summary>
/// Classe principal da aplicação.
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    public IServiceProvider Services => _host.Services;
    public App()
    {
        _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Infrastructure (persistência, file system, providers concretos)
                services.AddInfrastructure();

                // Application (services de projeto, controller, modelo + event bus)
                services.AddApplication();

                // Export (providers JSON + PLC + ExportManager)
                services.AddExport();

                // Presentation (views, viewmodels, services de UI)
                services.AddPresentation();
            })
            .Build();
    }

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        await _host.StartAsync();

        // Inicializa o ThemeManager: detecta preferência salva, registra listener
        // de mudança de tema do Windows (modo Auto) e aplica o tema efetivo.
        ThemeManager.Initialize();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();

        MainWindow = mainWindow;

        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        // Remove o listener de SystemEvents para evitar leak após shutdown.
        ThemeManager.Shutdown();

        await _host.StopAsync();

        _host.Dispose();

        base.OnExit(e);
    }
}