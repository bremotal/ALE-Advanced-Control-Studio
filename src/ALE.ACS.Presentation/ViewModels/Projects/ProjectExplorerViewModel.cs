using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Projects;

namespace ALE.ACS.Presentation.ViewModels.Projects;

public class ProjectExplorerViewModel : ViewModelBase
{
    public ObservableCollection<TreeNode> ProjectItems { get; }

    public ProjectExplorerViewModel(
        FakeProjectService projectService)
    {
        ProjectItems = projectService.LoadProject();
    }
}