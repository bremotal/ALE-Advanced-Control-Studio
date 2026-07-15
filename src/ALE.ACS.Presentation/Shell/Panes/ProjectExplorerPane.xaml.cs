using System.Windows.Controls;
using ALE.ACS.Presentation.Services.Projects;
using ALE.ACS.Presentation.ViewModels.Projects;

namespace ALE.ACS.Presentation.Shell.Panes;

/// <summary>
/// Painel responsável por exibir a estrutura do projeto.
/// </summary>
public partial class ProjectExplorerPane : UserControl
{
    public ProjectExplorerPane()
    {
        InitializeComponent();

        var service = new FakeProjectService();
        DataContext = new ProjectExplorerViewModel(service);
    }
}