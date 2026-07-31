using System.Collections.ObjectModel;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Explorer;
using ALE.ACS.Presentation.Services.Navigation;
using ALE.ACS.Presentation.Services.Session;
using CommunityToolkit.Mvvm.Input;

namespace ALE.ACS.Presentation.ViewModels.Projects;

public partial class ProjectExplorerViewModel : ViewModelBase
{
    private readonly IProjectExplorerService _projectExplorerService;
    private readonly INavigationService _navigationService;
    private readonly IProjectSession _projectSession;
    private TreeNode? _selectedNode;

    public ObservableCollection<TreeNode> ProjectItems { get; }

    public TreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (ReferenceEquals(_selectedNode, value))
                return;

            _selectedNode = value;
            _navigationService.SelectNode(value);
            OnPropertyChanged();
        }
    }

    public ProjectExplorerViewModel(
        IProjectExplorerService projectExplorerService,
        IProjectSession projectSession)
    {
        _projectExplorerService = projectExplorerService;
        _navigationService = NavigationService.Current;
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

        var projectNodes = _projectExplorerService.LoadProjectTree();
        var rootNode = projectNodes.FirstOrDefault();

        if (rootNode is null)
            return;

        rootNode.Name = _projectSession.CurrentProject?.Name ?? "ALE Demo Project";

        foreach (var item in projectNodes)
        {
            ProjectItems.Add(item);
        }

        SelectedNode = rootNode;
    }

    [RelayCommand]
    private void OpenSelectedNode()
    {
        if (SelectedNode is null)
            return;

        _navigationService.SelectNode(SelectedNode);
    }

    [RelayCommand]
    private void RenameSelectedNode()
    {
        if (SelectedNode is null)
            return;

        SelectedNode.Name = $"{SelectedNode.Name} (Renamed)";
        _navigationService.SelectNode(SelectedNode);
        OnPropertyChanged(nameof(ProjectItems));
    }

    [RelayCommand]
    private void ExpandSelectedNode()
    {
        if (SelectedNode is null)
            return;

        SetExpanded(SelectedNode, true);
        OnPropertyChanged(nameof(ProjectItems));
    }

    [RelayCommand]
    private void CollapseSelectedNode()
    {
        if (SelectedNode is null)
            return;

        SetExpanded(SelectedNode, false);
        OnPropertyChanged(nameof(ProjectItems));
    }

    private static void SetExpanded(TreeNode node, bool isExpanded)
    {
        node.IsExpanded = isExpanded;

        foreach (var child in node.Children)
        {
            SetExpanded(child, isExpanded);
        }
    }
}