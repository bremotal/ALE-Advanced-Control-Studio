using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ALE.ACS.Presentation.ViewModels.Projects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ALE.ACS.Presentation.Shell.Panes;

/// <summary>
/// Painel responsável por exibir a estrutura do projeto.
/// </summary>
public partial class ProjectExplorerPane : UserControl
{
    public ProjectExplorerPane()
    {
        InitializeComponent();

        var app = System.Windows.Application.Current as App;
        var hostField = typeof(App).GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic);
        var host = hostField?.GetValue(app) as IHost;

        if (host is null)
            throw new InvalidOperationException("The presentation host is not available.");

        DataContext = host.Services.GetRequiredService<ProjectExplorerViewModel>();
    }
}