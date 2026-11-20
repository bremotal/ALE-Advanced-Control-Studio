using System.Collections.ObjectModel;
using ALE.ACS.Application.EventBus;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Explorer;

public sealed class ProjectExplorerService : IProjectExplorerService
{
    private readonly TreeNode _projectRoot;
    private readonly TreeNode _controllersNode;
    private readonly TreeNode _modelsNode;
    // REMED-003b: referências a todas as pastas filhas para poder limpar
    // seu conteúdo quando um projeto é criado/fechado. Antes só Controllers
    // e Models eram limpos — pastas IO/Tags/Scripts/Trends/Historian/Reports
    // mantinham conteúdo de projeto anterior, levando a árvore a um estado
    // inconsistente ao criar o 2º projeto.
    private readonly TreeNode _ioNode;
    private readonly TreeNode _tagsNode;
    private readonly TreeNode _scriptsNode;
    private readonly TreeNode _trendsNode;
    private readonly TreeNode _historianNode;
    private readonly TreeNode _reportsNode;

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

        // REMED-003b: criar todas as pastas como fields nomeados para que
        // possam ser limpas em OnProjectCreated/OnProjectClosed.
        _ioNode = new TreeNode
        {
            Name = "IO",
            Type = ProjectItemType.IO,
            IsExpanded = true
        };

        _tagsNode = new TreeNode
        {
            Name = "Tags",
            Type = ProjectItemType.Tag,
            IsExpanded = true
        };

        _scriptsNode = new TreeNode
        {
            Name = "Scripts",
            Type = ProjectItemType.Script,
            IsExpanded = true
        };

        _trendsNode = new TreeNode
        {
            Name = "Trends",
            Type = ProjectItemType.Trend,
            IsExpanded = true
        };

        _historianNode = new TreeNode
        {
            Name = "Historian",
            Type = ProjectItemType.Historian,
            IsExpanded = true
        };

        _reportsNode = new TreeNode
        {
            Name = "Reports",
            Type = ProjectItemType.Report,
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
                _ioNode,
                _tagsNode,
                _scriptsNode,
                _trendsNode,
                _historianNode,
                _reportsNode
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

        // REMED-003b: limpar TODAS as pastas, não só Controllers/Models.
        _controllersNode.Children.Clear();
        _modelsNode.Children.Clear();
        _ioNode.Children.Clear();
        _tagsNode.Children.Clear();
        _scriptsNode.Children.Clear();
        _trendsNode.Children.Clear();
        _historianNode.Children.Clear();
        _reportsNode.Children.Clear();
    }

    private void OnProjectClosed(ProjectClosedEvent projectEvent)
    {
        _projectRoot.Name = "No Project";

        // REMED-003b: limpar TODAS as pastas ao fechar projeto.
        _controllersNode.Children.Clear();
        _modelsNode.Children.Clear();
        _ioNode.Children.Clear();
        _tagsNode.Children.Clear();
        _scriptsNode.Children.Clear();
        _trendsNode.Children.Clear();
        _historianNode.Children.Clear();
        _reportsNode.Children.Clear();
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