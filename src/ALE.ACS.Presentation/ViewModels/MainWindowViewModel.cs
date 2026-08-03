using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Presentation.Engineering.Base;
using ALE.ACS.Presentation.Models.Output;
using ALE.ACS.Presentation.Services.Dialogs;
using ALE.ACS.Presentation.Services.Navigation;
using ALE.ACS.Presentation.Services.Session;
using ALE.ACS.Presentation.Shell.Dialogs.Projects;
using ALE.ACS.Presentation.Views;

namespace ALE.ACS.Presentation.ViewModels;

/// <summary>
/// ViewModel principal da aplicação.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IProjectService _projectService;
    private readonly IProjectSession _projectSession;
    private readonly WorkspaceViewModel _workspaceViewModel;
    private readonly EngineeringValidationService _engineeringValidationService = new();
    private readonly Stack<(string Title, string Kind, string Content)> _undoStack = new();
    private readonly Stack<(string Title, string Kind, string Content)> _redoStack = new();
    private bool _isApplyingProperties;

    private const string DefaultWindowTitle = "ALE Advanced Control Studio";

    [ObservableProperty]
    private Project? currentProject;

    [ObservableProperty]
    private string selectedName = string.Empty;

    [ObservableProperty]
    private string selectedType = string.Empty;

    [ObservableProperty]
    private string selectedDescription = string.Empty;

    [ObservableProperty]
    private bool isExplorerVisible = true;

    [ObservableProperty]
    private bool isWorkspaceVisible = true;

    [ObservableProperty]
    private bool isPropertiesVisible = true;

    [ObservableProperty]
    private bool isOutputVisible = true;

    [ObservableProperty]
    private bool isStatusBarVisible = true;

    public WorkspaceView WorkspaceView { get; }

    public ObservableCollection<string> RecentProjects { get; } = [];

    public ObservableCollection<OutputLogEntry> OutputEntries { get; } = [];

    /// <summary>
    /// Título exibido na janela principal.
    /// </summary>
    public string WindowTitle =>
        CurrentProject is null
            ? DefaultWindowTitle
            : $"{DefaultWindowTitle} - {CurrentProject.Name}";

    /// <summary>
    /// Texto exibido na barra de status da IDE.
    /// </summary>
    public string StatusText =>
        !string.IsNullOrWhiteSpace(_navigationService.StatusMessage)
            ? _navigationService.StatusMessage
            : CurrentProject is null
                ? "Ready"
                : $"Project: {CurrentProject.Name}";

    public string ProjectStatus => CurrentProject is null ? "No Project" : CurrentProject.Name;

    public string DocumentStatus => _navigationService.ActiveDocument?.Title ?? "No Document";

    public string StateStatus => _navigationService.ActiveDocument?.IsDirty == true ? "Modified" : "Saved";

    /// <summary>
    /// Conteúdo exibido na área de trabalho da IDE.
    /// </summary>
    public string WorkspaceContent =>
        CurrentProject is null
            ? "Create or open a project to begin designing control solutions."
            : $"Project: {CurrentProject.Name}\nStatus: {(CurrentProject.IsDirty ? "Modified" : "Saved")}";

    /// <summary>
    /// Conteúdo exibido no painel de propriedades.
    /// </summary>
    public string PropertiesContent =>
        !string.IsNullOrWhiteSpace(_navigationService.PropertiesMessage)
            ? _navigationService.PropertiesMessage
            : CurrentProject is null
                ? "No project selected."
                : $"Name: {CurrentProject.Name}\nFile: {CurrentProject.FileName ?? "Not saved"}\nDirty: {(CurrentProject.IsDirty ? "Yes" : "No")}";

    public string OutputContent => _navigationService.OutputMessage;

    public MainWindowViewModel(
        IDialogService dialogService,
        IProjectService projectService,
        IProjectSession projectSession,
        WorkspaceViewModel workspaceViewModel)
    {
        _dialogService = dialogService;
        _navigationService = NavigationService.Current;
        _projectService = projectService;
        _projectSession = projectSession;
        _workspaceViewModel = workspaceViewModel;

        _navigationService.NavigationChanged += OnNavigationChanged;

        WorkspaceView = new WorkspaceView
        {
            DataContext = workspaceViewModel
        };

        LogInfo("IDE initialized.");
    }

    partial void OnCurrentProjectChanged(Project? value)
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(ProjectStatus));
        OnPropertyChanged(nameof(DocumentStatus));
        OnPropertyChanged(nameof(StateStatus));
        OnPropertyChanged(nameof(WorkspaceContent));
        OnPropertyChanged(nameof(PropertiesContent));
        OnPropertyChanged(nameof(OutputContent));

        if (value is null)
        {
            _projectSession.CloseProject();
            LogInfo("Project closed.");
            return;
        }

        _projectSession.SetCurrentProject(value);
        LogInfo($"Project selected: {value.Name}");
    }

    private void OnNavigationChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(DocumentStatus));
        OnPropertyChanged(nameof(StateStatus));
        OnPropertyChanged(nameof(PropertiesContent));
        OnPropertyChanged(nameof(OutputContent));

        SyncEditablePropertiesFromActiveDocument();
        LogInfo(_navigationService.OutputMessage);
    }

    [RelayCommand]
    private void NewProject()
    {
        var dialog = _dialogService.ShowDialog<NewProjectDialog>();

        if (dialog.DialogResult != true)
            return;

        var projectName = dialog.ViewModel.ProjectName;

        var result = _projectService.Create(projectName);

        if (!result.IsSuccess || result.Value is null)
        {
            System.Windows.MessageBox.Show(
                result.Error!,
                "ALE ACS",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            LogError(result.Error ?? "Unable to create project.");
            return;
        }

        var createdProject = result.Value;

        CurrentProject = createdProject;
        AddRecentProject(createdProject.FileName ?? $"{createdProject.Name}.aleacs");
    }

    [RelayCommand]
    private void OpenProject()
    {
        var name = $"Opened Project {DateTime.Now:HHmmss}";
        var result = _projectService.Create(name);

        if (!result.IsSuccess || result.Value is null)
        {
            LogError(result.Error ?? "Unable to open project.");
            return;
        }

        var openedProject = result.Value;

        openedProject.SetFileName($"C:/Projects/{name}.aleacs");
        openedProject.MarkAsSaved();
        CurrentProject = openedProject;
        AddRecentProject(openedProject.FileName!);
        LogInfo($"Project opened: {name}");
    }

    [RelayCommand]
    private void SaveProject()
    {
        if (CurrentProject is null)
        {
            LogWarning("No active project to save.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CurrentProject.FileName))
        {
            CurrentProject.SetFileName($"C:/Projects/{CurrentProject.Name}.aleacs");
        }

        CurrentProject.MarkAsSaved();
        AddRecentProject(CurrentProject.FileName!);
        OnPropertyChanged(nameof(PropertiesContent));
        OnPropertyChanged(nameof(StateStatus));
        LogInfo($"Project saved: {CurrentProject.Name}");
    }

    [RelayCommand]
    private void SaveProjectAs()
    {
        if (CurrentProject is null)
        {
            LogWarning("No active project to save.");
            return;
        }

        var fileName = $"C:/Projects/{CurrentProject.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.aleacs";
        CurrentProject.SetFileName(fileName);
        CurrentProject.MarkAsSaved();
        AddRecentProject(fileName);
        OnPropertyChanged(nameof(PropertiesContent));
        OnPropertyChanged(nameof(StateStatus));
        LogInfo($"Project saved as: {fileName}");
    }

    [RelayCommand]
    private void CloseProject()
    {
        CurrentProject = null;
        _undoStack.Clear();
        _redoStack.Clear();
        ApplyPropertiesCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void ExitApplication()
    {
        LogInfo("Exit command executed.");
        System.Windows.Application.Current.Shutdown();
    }

    [RelayCommand]
    private void OpenRecentProject(string? projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
        {
            LogWarning("Recent project action requested without a valid path.");
            return;
        }

        var name = Path.GetFileNameWithoutExtension(projectPath);
        var result = _projectService.Create(name);

        if (!result.IsSuccess || result.Value is null)
        {
            LogError(result.Error ?? "Unable to open recent project.");
            return;
        }

        var recentProject = result.Value;

        recentProject.SetFileName(projectPath);
        recentProject.MarkAsSaved();
        CurrentProject = recentProject;
        AddRecentProject(projectPath);
        LogInfo($"Recent project opened: {projectPath}");
    }

    [RelayCommand]
    private void NewFuzzyController()
    {
        OpenController("Fuzzy Controller");
    }

    [RelayCommand]
    private void NewMpcController()
    {
        OpenController("MPC Controller");
    }

    [RelayCommand]
    private void NewAdaptiveController()
    {
        OpenController("Adaptive Controller");
    }

    [RelayCommand]
    private void NewNeuralController()
    {
        OpenController("Neural Controller");
    }

    [RelayCommand]
    private void NewIdentificationController()
    {
        OpenController("Identification Controller");
    }

    [RelayCommand]
    private void NewOptimizationController()
    {
        OpenController("Optimization Controller");
    }

    [RelayCommand]
    private void NewPidController()
    {
        OpenController("PID Controller");
    }

    [RelayCommand]
    private void RunValidation()
    {
        var activeDocument = _navigationService.ActiveDocument;

        if (activeDocument is null)
        {
            _navigationService.NotifyValidation("Validation", false, "No active document.");
            LogWarning("Validation requested without active document.");
            return;
        }

        var document = new EngineeringDocument(activeDocument.Title, activeDocument.Content)
        {
            IsDirty = activeDocument.IsDirty
        };

        var results = _engineeringValidationService.Validate(document);
        var isValid = results.Count == 0;
        var message = isValid ? "Document is valid." : string.Join("; ", results);

        _navigationService.NotifyValidation("Validation", isValid, message);
        LogInfo($"Validation executed for: {activeDocument.Title}");
    }

    [RelayCommand]
    private void RunSimulation()
    {
        var activeDocument = _navigationService.ActiveDocument;

        if (activeDocument is null)
        {
            LogWarning("Simulation requested without active document.");
            return;
        }

        var simulation = new EngineeringSimulationService();
        var message = simulation.CreateSimulationDraft(new EngineeringDocument(activeDocument.Title, activeDocument.Content));
        LogInfo($"Simulation: {message}");
    }

    [RelayCommand]
    private void OpenOptions()
    {
        LogInfo("Options requested. Feature pending full implementation.");

        System.Windows.MessageBox.Show(
            "Options will be available in a future release.",
            "ALE ACS - Options",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ToggleExplorer()
    {
        IsExplorerVisible = !IsExplorerVisible;
        LogInfo($"Explorer visibility: {(IsExplorerVisible ? "Visible" : "Hidden")}");
    }

    [RelayCommand]
    private void ToggleWorkspace()
    {
        IsWorkspaceVisible = !IsWorkspaceVisible;
        LogInfo($"Workspace visibility: {(IsWorkspaceVisible ? "Visible" : "Hidden")}");
    }

    [RelayCommand]
    private void ToggleProperties()
    {
        IsPropertiesVisible = !IsPropertiesVisible;
        LogInfo($"Properties visibility: {(IsPropertiesVisible ? "Visible" : "Hidden")}");
    }

    [RelayCommand]
    private void ToggleOutput()
    {
        IsOutputVisible = !IsOutputVisible;
        LogInfo($"Output visibility: {(IsOutputVisible ? "Visible" : "Hidden")}");
    }

    [RelayCommand]
    private void ToggleStatusBar()
    {
        IsStatusBarVisible = !IsStatusBarVisible;
        LogInfo($"Status Bar visibility: {(IsStatusBarVisible ? "Visible" : "Hidden")}");
    }

    [RelayCommand]
    private void ShowAbout()
    {
        LogInfo("About dialog opened.");

        System.Windows.MessageBox.Show(
            "ALE Advanced Control Studio\nRelease 0.3.1\nEngineering IDE Framework",
            "About ALE ACS",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    [RelayCommand]
    private void OpenDocumentation()
    {
        LogInfo("Documentation action executed.");

        var docsPath = Path.Combine(AppContext.BaseDirectory, "docs", "README.md");

        if (!File.Exists(docsPath))
        {
            LogWarning("Documentation file not found. Action recorded only.");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = docsPath,
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void OpenReleaseNotes()
    {
        LogInfo("Release notes action executed.");

        var notesPath = Path.Combine(AppContext.BaseDirectory, "docs", "releases", "Release-0.2.md");

        if (!File.Exists(notesPath))
        {
            LogWarning("Release notes file not found. Action recorded only.");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = notesPath,
            UseShellExecute = true
        });
    }

    [RelayCommand(CanExecute = nameof(CanApplyProperties))]
    private void ApplyProperties()
    {
        var activeDocument = _navigationService.ActiveDocument;

        if (activeDocument is null)
            return;

        _undoStack.Push((activeDocument.Title, activeDocument.Kind, activeDocument.Content));
        _redoStack.Clear();

        _isApplyingProperties = true;
        activeDocument.Title = SelectedName;
        activeDocument.Kind = SelectedType;
        activeDocument.Content = SelectedDescription;
        _isApplyingProperties = false;

        var validationDocument = new EngineeringDocument(SelectedName, SelectedDescription)
        {
            IsDirty = activeDocument.IsDirty
        };

        var validationMessages = _engineeringValidationService.Validate(validationDocument);
        var isValid = validationMessages.Count == 0;
        var validationMessage = isValid
            ? "Document properties are valid."
            : string.Join("; ", validationMessages);

        _navigationService.NotifyValidation("Properties", isValid, validationMessage);

        _navigationService.ActivateDocument(activeDocument.Id);
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        LogInfo($"Properties applied to document: {activeDocument.Title}");
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        var activeDocument = _navigationService.ActiveDocument;

        if (activeDocument is null || _undoStack.Count == 0)
            return;

        _redoStack.Push((activeDocument.Title, activeDocument.Kind, activeDocument.Content));
        var previous = _undoStack.Pop();

        _isApplyingProperties = true;
        activeDocument.Title = previous.Title;
        activeDocument.Kind = previous.Kind;
        activeDocument.Content = previous.Content;
        _isApplyingProperties = false;

        _navigationService.ActivateDocument(activeDocument.Id);
        SyncEditablePropertiesFromActiveDocument();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        LogInfo("Undo applied.");
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        var activeDocument = _navigationService.ActiveDocument;

        if (activeDocument is null || _redoStack.Count == 0)
            return;

        _undoStack.Push((activeDocument.Title, activeDocument.Kind, activeDocument.Content));
        var next = _redoStack.Pop();

        _isApplyingProperties = true;
        activeDocument.Title = next.Title;
        activeDocument.Kind = next.Kind;
        activeDocument.Content = next.Content;
        _isApplyingProperties = false;

        _navigationService.ActivateDocument(activeDocument.Id);
        SyncEditablePropertiesFromActiveDocument();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        LogInfo("Redo applied.");
    }

    private bool CanApplyProperties()
    {
        return _navigationService.ActiveDocument is not null;
    }

    private bool CanUndo()
    {
        return _undoStack.Count > 0;
    }

    private bool CanRedo()
    {
        return _redoStack.Count > 0;
    }

    private void SyncEditablePropertiesFromActiveDocument()
    {
        if (_isApplyingProperties)
            return;

        var activeDocument = _navigationService.ActiveDocument;

        SelectedName = activeDocument?.Title ?? string.Empty;
        SelectedType = activeDocument?.Kind ?? string.Empty;
        SelectedDescription = activeDocument?.Content ?? string.Empty;

        ApplyPropertiesCommand.NotifyCanExecuteChanged();
    }

    private void AddRecentProject(string projectPath)
    {
        var existing = RecentProjects.FirstOrDefault(item => string.Equals(item, projectPath, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            RecentProjects.Remove(existing);
        }

        RecentProjects.Insert(0, projectPath);

        while (RecentProjects.Count > 10)
        {
            RecentProjects.RemoveAt(RecentProjects.Count - 1);
        }
    }

    private void LogInfo(string message)
    {
        AddLog(OutputLogLevel.Info, message);
    }

    private void LogWarning(string message)
    {
        AddLog(OutputLogLevel.Warning, message);
    }

    private void LogError(string message)
    {
        AddLog(OutputLogLevel.Error, message);
    }

    private void AddLog(OutputLogLevel level, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        OutputEntries.Add(new OutputLogEntry(level, message));
    }

    [RelayCommand]
    private void NewController()
    {
        NewFuzzyController();
        LogInfo("Toolbar new controller command executed.");
    }

    [RelayCommand]
    private void CloseActiveDocument()
    {
        _workspaceViewModel.CloseActiveDocumentCommand.Execute(null);
        LogInfo("Toolbar close document command executed.");
    }

    private void OpenController(string controllerName)
    {
        _navigationService.SelectNode(new Models.TreeNode
        {
            Name = controllerName,
            Type = Models.ProjectItemType.Controller
        });

        LogInfo($"Controller created/opened: {controllerName}");
        LogInfo("Explorer refresh requested through navigation state update.");
    }
}