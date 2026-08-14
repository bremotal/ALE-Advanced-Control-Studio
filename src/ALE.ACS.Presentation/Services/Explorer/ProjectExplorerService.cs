using System.Collections.ObjectModel;
using ALE.ACS.Application.EventBus;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Explorer;

public sealed class ProjectExplorerService : IProjectExplorerService
{
    private readonly TreeNode _projectRoot;
    private readonly TreeNode _controllersNode;
    private readonly TreeNode _modelsNode;

    public ProjectExplorerService(IEventBus eventBus)
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        _controllersNode = new TreeNode
        {
            Name = "Controllers",
            Type = ProjectItemType.Folder,
            IsExpanded = true
        };

        _modelsNode = new TreeNode
        {
            Name = "Models",
            Type = ProjectItemType.Folder,
            IsExpanded = true
        };

        _projectRoot = new TreeNode
        {
            Name = "No Project",
            Type = ProjectItemType.Project,
            IsExpanded = true,
            Children =
            {
                _controllersNode,
                _modelsNode,
                CreateFolder("IO", ProjectItemType.IO),
                CreateFolder("Tags", ProjectItemType.Tag),
                CreateFolder("Scripts", ProjectItemType.Script),
                CreateFolder("Trends", ProjectItemType.Trend),
                CreateFolder("Historian", ProjectItemType.Historian),
                CreateFolder("Reports", ProjectItemType.Report)
            }
        };

        ProjectItems = [_projectRoot];

        eventBus.Subscribe<ProjectCreatedEvent>(OnProjectCreated);
        eventBus.Subscribe<ProjectClosedEvent>(OnProjectClosed);

        eventBus.Subscribe<ControllerCreatedEvent>(OnControllerCreated);
        eventBus.Subscribe<ControllerRenamedEvent>(OnControllerRenamed);

        eventBus.Subscribe<ModelCreatedEvent>(OnModelCreated);
    }

    public ObservableCollection<TreeNode> ProjectItems { get; }

    private void OnProjectCreated(ProjectCreatedEvent projectEvent)
    {
        if (projectEvent is null)
        {
            return;
        }

        _projectRoot.Name = projectEvent.ProjectName;

        _controllersNode.Children.Clear();
        _modelsNode.Children.Clear();
    }

    private void OnProjectClosed(ProjectClosedEvent projectEvent)
    {
        _projectRoot.Name = "No Project";

        _controllersNode.Children.Clear();
        _modelsNode.Children.Clear();
    }

    private void OnControllerCreated(ControllerCreatedEvent controllerEvent)
    {
        if (controllerEvent is null)
        {
            return;
        }

        if (_controllersNode.Children.Any(
                item => string.Equals(
                    item.Name,
                    controllerEvent.ControllerName,
                    StringComparison.Ordinal)))
        {
            return;
        }

        _controllersNode.Children.Add(new TreeNode
        {
            Name = controllerEvent.ControllerName,
            Type = ProjectItemType.Controller
        });

        _controllersNode.IsExpanded = true;
    }

    private void OnControllerRenamed(ControllerRenamedEvent controllerEvent)
    {
        if (controllerEvent is null ||
            string.IsNullOrWhiteSpace(controllerEvent.PreviousName) ||
            string.IsNullOrWhiteSpace(controllerEvent.ControllerName))
        {
            return;
        }

        foreach (var controllerNode in _controllersNode.Children)
        {
            if (controllerNode.Type == ProjectItemType.Controller &&
                string.Equals(
                    controllerNode.Name,
                    controllerEvent.PreviousName,
                    StringComparison.Ordinal))
            {
                controllerNode.Name = controllerEvent.ControllerName;
                return;
            }
        }
    }

    private void OnModelCreated(ModelCreatedEvent modelEvent)
    {
        if (modelEvent is null ||
            string.IsNullOrWhiteSpace(modelEvent.ModelName))
        {
            return;
        }

        if (_modelsNode.Children.Any(
                item => string.Equals(
                    item.Name,
                    modelEvent.ModelName,
                    StringComparison.Ordinal)))
        {
            return;
        }

        _modelsNode.Children.Add(new TreeNode
        {
            Name = modelEvent.ModelName,
            Type = ProjectItemType.Model
        });

        _modelsNode.IsExpanded = true;
    }
    public void AddModel(string modelName)
    {
        OnModelCreated(
            new ModelCreatedEvent(modelName));
    }

    public void AddController(string controllerName)
    {
        OnControllerCreated(
            new ControllerCreatedEvent(controllerName));
    }

    private static TreeNode CreateFolder(
        string name,
        ProjectItemType type)
    {
        return new TreeNode
        {
            Name = name,
            Type = type
        };
    }
}