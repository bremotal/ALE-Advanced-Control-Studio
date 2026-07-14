using System.Windows.Controls;
using ALE.ACS.Presentation.ViewModels.Projects;

namespace ALE.ACS.Presentation.Shell.Panes;

public partial class ProjectExplorer : UserControl
{
    public ProjectExplorer(ProjectExplorerViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}