using System.Collections.ObjectModel;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Projects;
using ALE.ACS.Presentation.Services.Session;

namespace ALE.ACS.Presentation.ViewModels.Projects;

public class ProjectExplorerViewModel : ViewModelBase
{
    private readonly FakeProjectService _projectService;
    private readonly IProjectSession _projectSession;

    public ObservableCollection<TreeNode> ProjectItems { get; }

    public ProjectExplorerViewModel(
        FakeProjectService projectService,
        IProjectSession projectSession)
    {
        _projectService = projectService;
        _projectSession = projectSession;
        ProjectItems = new ObservableCollection<TreeNode>();

        _projectSession.CurrentProjectChanged += OnCurrentProjectChanged;
        RefreshProjectItems();
    }

    private void OnCurrentProjectChanged(object? sender, Project? project)
    {
        RefreshProjectItems();
    }

    private void RefreshProjectItems()
    {
        ProjectItems.Clear();

        var projectNodes = _projectService.LoadProject();
        var rootNode = projectNodes.FirstOrDefault();

        if (rootNode is null)
            return;

        rootNode.Name = _projectSession.CurrentProject?.Name ?? "ALE Demo Project";

        foreach (var item in projectNodes)
        {
            ProjectItems.Add(item);
        }
    }
}