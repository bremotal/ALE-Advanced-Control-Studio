using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Documents;
using ALE.ACS.Presentation.Services.Explorer;
using ALE.ACS.Presentation.Services.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ALE.ACS.Presentation.ViewModels.Projects;

public partial class ProjectExplorerViewModel : ViewModelBase
{
    private readonly IProjectExplorerService _projectExplorerService;
    private readonly IDocumentService _documentService;
    private readonly INavigationService _navigationService;

    private TreeNode? _selectedNode;

    public ObservableCollection<TreeNode> ProjectItems =>
        _projectExplorerService.ProjectItems;

    public TreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (ReferenceEquals(_selectedNode, value))
            {
                return;
            }

            _selectedNode = value;

            _navigationService.SelectNode(value);

            OnPropertyChanged();
            OnPropertyChanged(nameof(CanCreateModel));
            OnPropertyChanged(nameof(CanCreateController));
        }
    }

    public bool CanCreateModel =>
        SelectedNode?.Name == "Models";

    public bool CanCreateController =>
        SelectedNode?.Name == "Controllers";

    public ProjectExplorerViewModel(
        IProjectExplorerService projectExplorerService,
        IDocumentService documentService)
    {
        _projectExplorerService = projectExplorerService;
        _documentService = documentService;
        _navigationService = NavigationService.Current;

        SelectedNode = ProjectItems.FirstOrDefault();
    }

    [RelayCommand]
    private void NewModel()
    {
        if (!CanCreateModel)
        {
            return;
        }

        var modelName = $"Modelo_{DateTime.Now:HHmmss}";

        _projectExplorerService.AddModel(modelName);

        _documentService.OpenDocument(
            modelName,
            string.Empty,
            "Model");

        _navigationService.SelectNode(
            new TreeNode
            {
                Name = modelName,
                Type = ProjectItemType.Model
            });

        LogInfo($"Model created: {modelName}");

        OnPropertyChanged(nameof(ProjectItems));
    }

    [RelayCommand]
    private void NewController()
    {
        if (!CanCreateController)
        {
            return;
        }

        var controllerName = $"Controller_{DateTime.Now:HHmmss}";

        _projectExplorerService.AddController(controllerName);

        _documentService.OpenDocument(
            controllerName,
            string.Empty,
            "Controller");

        _navigationService.SelectNode(
            new TreeNode
            {
                Name = controllerName,
                Type = ProjectItemType.Controller
            });

        LogInfo($"Controller created: {controllerName}");

        OnPropertyChanged(nameof(ProjectItems));
    }

    [RelayCommand]
    private void OpenSelectedNode()
    {
        if (SelectedNode is null)
        {
            return;
        }

        _navigationService.SelectNode(SelectedNode);
    }

    [RelayCommand]
    private void RenameSelectedNode()
    {
        if (SelectedNode is null)
        {
            return;
        }

        // REMED-001: rename de verdade via diálogo de prompt.
        // Antes era apenas: SelectedNode.Name = $"{SelectedNode.Name} (Renamed)";
        // que acumulava "(Renamed)" a cada clique.
        var newName = Services.Dialogs.PromptDialog.Show(
            title: "Renomear",
            message: "Novo nome:",
            defaultValue: SelectedNode.Name);

        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        newName = newName.Trim();

        if (string.Equals(newName, SelectedNode.Name, StringComparison.Ordinal))
        {
            return;
        }

        var oldName = SelectedNode.Name;
        SelectedNode.Name = newName;

        // Renomeia o documento correspondente na workspace, se existir.
        var doc = _documentService.Documents.FirstOrDefault(d =>
            string.Equals(d.Title, oldName, StringComparison.OrdinalIgnoreCase));

        if (doc is not null)
        {
            doc.Title = newName;
        }

        _navigationService.SelectNode(SelectedNode);

        OnPropertyChanged(nameof(ProjectItems));
    }

    [RelayCommand]
    private void ExpandSelectedNode()
    {
        if (SelectedNode is null)
        {
            return;
        }

        SetExpanded(SelectedNode, true);

        OnPropertyChanged(nameof(ProjectItems));
    }

    [RelayCommand]
    private void CollapseSelectedNode()
    {
        if (SelectedNode is null)
        {
            return;
        }

        SetExpanded(SelectedNode, false);

        OnPropertyChanged(nameof(ProjectItems));
    }

    private static void SetExpanded(
        TreeNode node,
        bool isExpanded)
    {
        node.IsExpanded = isExpanded;

        foreach (var child in node.Children)
        {
            SetExpanded(child, isExpanded);
        }
    }

    private void LogInfo(string message)
    {
        // Mantém o Explorer independente do Output da MainWindow.
        System.Diagnostics.Debug.WriteLine(message);
    }
}