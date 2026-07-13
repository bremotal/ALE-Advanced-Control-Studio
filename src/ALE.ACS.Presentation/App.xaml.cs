using ALE.ACS.Application.DependencyInjection;
using ALE.ACS.Presentation.DependencyInjection;
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

    public App()
    {
        _host = Host
            .CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Application
                services.AddApplication();

                // Presentation
                services.AddPresentation();
            })
            .Build();
    }

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();

        MainWindow = mainWindow;

        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();

        base.OnExit(e);
    }
}