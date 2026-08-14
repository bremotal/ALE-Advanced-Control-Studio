using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ALE.ACS.Presentation.ViewModels.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace ALE.ACS.Presentation.Shell.Panes;

/// <summary>
/// Painel responsável por exibir a estrutura do projeto.
/// </summary>
public partial class ProjectExplorerPane : UserControl
{
    public ProjectExplorerPane()
    {
        InitializeComponent();

        if (DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        var app = System.Windows.Application.Current as ALE.ACS.Presentation.App;

        if (app is null)
        {
            return;
        }

        DataContext = app.Services
            .GetRequiredService<ProjectExplorerViewModel>();
    }
}