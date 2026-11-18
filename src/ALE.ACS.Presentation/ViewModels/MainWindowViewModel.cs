using ALE.ACS.Application.DigitalTwin;
using ALE.ACS.Application.EventBus;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Models;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Engine.Cascade;
using ALE.ACS.Engine.ProcessVariables;
using ALE.ACS.Engine.SmithPredictor;
using ALE.ACS.Presentation.Engineering.Base;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Models.Output;
using ALE.ACS.Presentation.Services.Dialogs;
using ALE.ACS.Presentation.Services.Documents;
using ALE.ACS.Presentation.Services.Navigation;
using ALE.ACS.Presentation.Services.Session;
using ALE.ACS.Presentation.Shell.Dialogs.Projects;
using ALE.ACS.Presentation.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ALE.ACS.Presentation.ViewModels;

/// <summary>
/// ViewModel principal da aplicação.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEventBus _eventBus;
    private readonly IDocumentManager _documentManager;
    private readonly INavigationService _navigationService;
    private readonly IProjectService _projectService;
    private readonly IProjectSession _projectSession;
    private readonly WorkspaceViewModel _workspaceViewModel;
    private readonly IDigitalTwinService _digitalTwinService;
    private readonly EngineeringValidationService _engineeringValidationService = new();
    private readonly Stack<(string Title, string Kind, string Content)> _undoStack = new();
    private readonly Stack<(string Title, string Kind, string Content)> _redoStack = new();
    private bool _isApplyingProperties;

    private const string DefaultWindowTitle = "ALE Advanced Control Studio";

    [ObservableProperty]
    private ALE.ACS.Presentation.Models.TreeNode? _selectedNode;

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

    /// <summary>
    /// Quando true, o painel de Output aparece como aba no content;
    /// quando false, aparece como painel inferior (padrão).
    /// </summary>
    [ObservableProperty]
    private bool outputModeIsTab = false;

    public WorkspaceView WorkspaceView { get; }

    public ObservableCollection<string> RecentProjects { get; } = [];

    public ObservableCollection<OutputLogEntry> OutputEntries { get; } = [];

    public string OutputText =>
    string.Join(
        Environment.NewLine,
        OutputEntries.Select(entry =>
            $"{entry.Timestamp:HH:mm:ss} | {entry.Level} | {entry.Message}"));

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
        IEventBus eventBus,
        IDocumentManager documentManager,
        IProjectService projectService,
        IProjectSession projectSession,
        WorkspaceViewModel workspaceViewModel,
        IDigitalTwinService digitalTwinService)
    {
        _dialogService = dialogService;
        _eventBus = eventBus;
        _documentManager = documentManager;
        _navigationService = NavigationService.Current;
        _projectService = projectService;
        _projectSession = projectSession;
        _workspaceViewModel = workspaceViewModel;
        _digitalTwinService = digitalTwinService;

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
        {
            return;
        }

        var projectName = dialog.ViewModel.ProjectName;

        var result = _projectService.Create(projectName);

        if (!result.IsSuccess || result.Value is null)
        {
            MessageBox.Show(
                result.Error!,
                "ALE ACS",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            LogError(result.Error ?? "Unable to create project.");
            return;
        }

        var createdProject = result.Value;

        CurrentProject = createdProject;
        _eventBus.Publish(new ProjectCreatedEvent(createdProject.Name));
        AddRecentProject(createdProject.FileName ?? $"{createdProject.Name}.aleacs");
    }

    [RelayCommand]
    private void OpenProject()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Abrir projeto ALE ACS",
            Filter = "Projeto ALE ACS (*.aleacs)|*.aleacs|Todos os arquivos (*.*)|*.*",
            DefaultExt = ".aleacs",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        OpenProjectFile(dialog.FileName);
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
            SaveProjectAs();
            return;
        }

        SaveProjectFile(CurrentProject.FileName);
    }

    [RelayCommand]
    private void SaveProjectAs()
    {
        if (CurrentProject is null)
        {
            LogWarning("No active project to save.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Salvar projeto ALE ACS",
            Filter = "Projeto ALE ACS (*.aleacs)|*.aleacs|Todos os arquivos (*.*)|*.*",
            DefaultExt = ".aleacs",
            AddExtension = true,
            FileName = $"{CurrentProject.Name}.aleacs",
            InitialDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        SaveProjectFile(dialog.FileName);
    }

    [RelayCommand]
    private void CloseProject()
    {
        CurrentProject = null;
        _undoStack.Clear();
        _redoStack.Clear();

        // LIMPA AS ABAS DO WORKSPACE
        _documentManager.OpenDocuments.Clear();

        // AVISA A ÁRVORE PARA LIMPAR
        _eventBus.Publish(new ProjectClosedEvent());

        ApplyPropertiesCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void NewModel()
    {
        if (SelectedNode is null ||
            SelectedNode.Name != "Models")
        {
            return;
        }

        var modelName = $"Modelo_{DateTime.Now:HHmmss}";

        var createResult = ControlModel.Create(modelName, string.Empty);
        if (!createResult.IsSuccess || createResult.Value is null)
        {
            LogInfo($"Falha ao criar modelo: {createResult.Error}");
            return;
        }
        var controlModel = createResult.Value;

        var doc = new ALE.ACS.Presentation.Models.EngineeringDocument(
            Guid.NewGuid().ToString("N"),
            modelName,
            "",
            "Model")
        {
            IsDirty = false,
            ControlModel = controlModel
        };

        _documentManager.OpenDocuments.Add(doc);

        _eventBus.Publish(
            new ModelCreatedEvent(modelName));

        _workspaceViewModel.RefreshDocuments();

        _navigationService.ActivateDocument(doc.Id);

        LogInfo($"Model created: {modelName}");
    }
    [RelayCommand]
    private void NewControllerFromTree()
    {
        if (SelectedNode is null || SelectedNode.Name != "Controllers") return;

        var controllerName = $"Controller_{DateTime.Now:HHmmss}";

        // Adiciona na árvore
        SelectedNode.Children.Add(new ALE.ACS.Presentation.Models.TreeNode
        {
            Name = controllerName,
            Type = ALE.ACS.Presentation.Models.ProjectItemType.Controller
        });
        SelectedNode.IsExpanded = true;

        // Abre no Workspace usando o namespace completo para evitar ambiguidade
        var doc = new ALE.ACS.Presentation.Models.EngineeringDocument(
            Guid.NewGuid().ToString(),
            controllerName,
            "",
            "Controller")
        {
            IsDirty = false
        };

        _documentManager.OpenDocuments.Add(doc);
        _workspaceViewModel.RefreshDocuments();

        LogInfo($"Controller created: {controllerName}");
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

        OpenProjectFile(projectPath);
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

        var document = new ALE.ACS.Presentation.Engineering.Base.EngineeringDocument(
            activeDocument.Title,
            activeDocument.Content)
        {
            IsDirty = activeDocument.IsDirty
        };

        var results = _engineeringValidationService.Validate(document); var isValid = results.Count == 0;
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

        // Digital Twin real (Bloco E4): quando o documento ativo é um PID Controller,
        // simulamos um controlador PID contra um processo FOPDT padrão.
        if (string.Equals(activeDocument.Kind, "PID", StringComparison.OrdinalIgnoreCase))
        {
            var pidSpec = CreateDefaultPidSpec();
            var processModel = CreateDefaultFopdtModel();
            var result = _digitalTwinService.SimulatePid(
                pidSpec,
                processModel,
                setpoint: 50.0,
                initialPv: 0.0,
                scanCount: 100);

            if (result.Success)
            {
                LogInfo(
                    $"Digital Twin OK: {result.StepCount} scans, " +
                    $"{result.SimulatedDuration.TotalSeconds:F1}s simulados. " +
                    $"Amostras: {result.Samples.Count}.");
            }
            else
            {
                LogWarning(
                    $"Digital Twin FAILED: {result.ErrorMessage}");
            }

            return;
        }

        // Fallback legado: draft placeholder para outros tipos.
        var simulation = new EngineeringSimulationService();
        var message = simulation.CreateSimulationDraft(new ALE.ACS.Presentation.Engineering.Base.EngineeringDocument(
            activeDocument.Title,
            activeDocument.Content));
        LogInfo($"Simulation: {message}");
    }

    /// <summary>
    /// Cria um PidSpec padrão para a simulação do Digital Twin.
    /// </summary>
    private static PidSpec CreateDefaultPidSpec()
    {
        var sp = new ProcessVariable("Setpoint", ProcessVariableKind.SP,
            EngineeringUnit.Percent, 0, 100, 50);
        var mv = new ProcessVariable("Manipulated", ProcessVariableKind.MV,
            EngineeringUnit.Percent, 0, 100, 0);
        var cv = new ProcessVariable("Controlled", ProcessVariableKind.PV,
            EngineeringUnit.Percent, 0, 100, 0);

        return new PidSpec("DigitalTwin_PID", sp, mv, cv, TimeSpan.FromMilliseconds(100))
        {
            Kp = 2.0,
            Ti = 5.0,
            Td = 0.5,
            N = 8.0,
            OutMin = 0.0,
            OutMax = 100.0
        };
    }

    /// <summary>
    /// Cria um modelo FOPDT padrão para a simulação do Digital Twin.
    /// </summary>
    private static FopdtModel CreateDefaultFopdtModel()
    {
        return new FopdtModel(
            name: "DigitalTwin_Process",
            k: 1.0,
            tau: 10.0,
            theta: 2.0,
            ts: 0.1);
    }

    [RelayCommand]
    private void OpenOptions()
    {
        LogInfo("Options requested. Feature pending full implementation.");

        MessageBox.Show(
            "Options will be available in a future release.",
            "ALE ACS - Options",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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
    private void CycleTheme()
    {
        Shell.Themes.ThemeManager.Cycle();
        var effective = Shell.Themes.ThemeManager.EffectiveIsDark ? "dark" : "light";
        var mode = Shell.Themes.ThemeManager.CurrentMode;
        LogInfo($"Theme cycled to {mode} (effective: {effective}).");
    }

    [RelayCommand]
    private void ToggleOutputMode()
    {
        OutputModeIsTab = !OutputModeIsTab;
        LogInfo($"Output mode: {(OutputModeIsTab ? "Tab in content" : "Bottom panel")}.");
    }

    [RelayCommand]
    private void ShowAbout()
    {
        LogInfo("About dialog opened.");

        MessageBox.Show(
            "ALE Advanced Control Studio\nRelease 0.3.1\nEngineering IDE Framework",
            "About ALE ACS",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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
        {
            return;
        }

        var previousTitle = activeDocument.Title;
        var previousKind = activeDocument.Kind;

        _undoStack.Push((activeDocument.Title, activeDocument.Kind, activeDocument.Content));
        _redoStack.Clear();

        _isApplyingProperties = true;
        activeDocument.Title = SelectedName;
        activeDocument.Kind = SelectedType;
        activeDocument.Content = SelectedDescription;
        _isApplyingProperties = false;

        if (string.Equals(previousKind, "Controller", StringComparison.Ordinal) &&
            !string.Equals(previousTitle, activeDocument.Title, StringComparison.Ordinal))
        {
            _eventBus.Publish(
                new ControllerRenamedEvent(
                    previousTitle,
                    activeDocument.Title));
        }

        var validationDocument = new ALE.ACS.Presentation.Engineering.Base.EngineeringDocument(
            SelectedName,
            SelectedDescription)
        {
            IsDirty = activeDocument.IsDirty
        };

        var validationMessages = _engineeringValidationService.Validate(validationDocument); var isValid = validationMessages.Count == 0;
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
        {
            return;
        }

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
        {
            return;
        }

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
        {
            return;
        }

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
        {
            return;
        }

        OutputEntries.Add(new OutputLogEntry(level, message));
        OnPropertyChanged(nameof(OutputText));
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
        var doc = new ALE.ACS.Presentation.Models.EngineeringDocument(
            Guid.NewGuid().ToString(),
            controllerName,
            "",
            "Controller")
        {
            IsDirty = false
        };

        _documentManager.OpenDocuments.Add(doc);
        _workspaceViewModel.RefreshDocuments();

        _eventBus.Publish(new ControllerCreatedEvent(controllerName));

        _navigationService.SelectNode(new ALE.ACS.Presentation.Models.TreeNode
        {
            Name = controllerName,
            Type = ALE.ACS.Presentation.Models.ProjectItemType.Controller
        });

        LogInfo($"Controller created/opened: {controllerName}");
        LogInfo("Explorer refresh requested through navigation state update.");
    }

    private void SaveProjectFile(string filePath)
    {
        if (CurrentProject is null)
        {
            return;
        }

        try
        {
            var documents = new List<ProjectDocumentFile>();

            foreach (var document in _documentManager.OpenDocuments)
            {
                LogInfo(
                    $"SAVE DEBUG: {document.Title} | {document.Kind} | ControlModel={(document.ControlModel is null ? "NULL" : "OK")}");

                documents.Add(new ProjectDocumentFile(
                    document.Id,
                    document.Title,
                    document.Content,
                    document.Kind,
                    DateTime.Now,
                    document.ControlModel));
            }

            var projectFile = new ProjectFile(
                CurrentProject.Name,
                _navigationService.ActiveDocument?.Id,
                documents);

            var json = JsonSerializer.Serialize(
                projectFile,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(filePath, json);

            CurrentProject.SetFileName(filePath);
            CurrentProject.MarkAsSaved();

            foreach (var document in _documentManager.OpenDocuments)
            {
                document.IsDirty = false;
            }

            AddRecentProject(filePath);
            OnPropertyChanged(nameof(PropertiesContent));
            OnPropertyChanged(nameof(StateStatus));

            LogInfo($"Project saved: {filePath}");
        }
        catch (Exception exception)
        {
            LogError($"Unable to save project: {exception.Message}");

            MessageBox.Show(
                exception.Message,
                "ALE ACS - Save error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OpenProjectFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);

            var projectFile = JsonSerializer.Deserialize<ProjectFile>(json);

            if (projectFile is null ||
                string.IsNullOrWhiteSpace(projectFile.Name))
            {
                throw new InvalidOperationException(
                    "The selected file is not a valid ALE ACS project.");
            }

            var result = _projectService.Create(projectFile.Name);

            if (!result.IsSuccess || result.Value is null)
            {
                throw new InvalidOperationException(
                    result.Error ?? "Unable to create project from file.");
            }

            var loadedProject = result.Value;

            loadedProject.SetFileName(filePath);
            loadedProject.MarkAsSaved();

            CurrentProject = loadedProject;
            _eventBus.Publish(new ProjectCreatedEvent(loadedProject.Name));

            _documentManager.OpenDocuments.Clear();

            foreach (var savedDocument in projectFile.Documents)
            {
                var document = new ALE.ACS.Presentation.Models.EngineeringDocument(
                    savedDocument.Id,
                    savedDocument.Title,
                    savedDocument.Content,
                    savedDocument.Kind)
                {
                    IsDirty = false,
                    ControlModel = savedDocument.ControlModel
                };

                _documentManager.OpenDocuments.Add(document);
            }

            if (!string.IsNullOrWhiteSpace(projectFile.ActiveDocumentId))
            {
                _navigationService.ActivateDocument(projectFile.ActiveDocumentId);
            }

            _workspaceViewModel.RefreshDocuments();
            AddRecentProject(filePath);

            // AVISA O EXPLORER PARA RECONSTRUIR A ÁRVORE COM OS DOCUMENTOS CARREGADOS
            foreach (var doc in _documentManager.OpenDocuments)
            {
                if (doc.Kind == "Controller")
                {
                    _eventBus.Publish(
                        new ControllerCreatedEvent(doc.Title));
                }
                else if (doc.Kind == "Model")
                {
                    _eventBus.Publish(
                        new ModelCreatedEvent(doc.Title));
                }
            }

            LogInfo($"Project opened: {filePath}");
        }
        catch (Exception exception)
        {
            LogError($"Unable to open project: {exception.Message}");

            MessageBox.Show(
                exception.Message,
                "ALE ACS - Open error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private sealed record ProjectFile(
        string Name,
        string? ActiveDocumentId,
        List<ProjectDocumentFile> Documents);

    private sealed record ProjectDocumentFile(
        string Id,
        string Title,
        string Content,
        string Kind,
        DateTime LastModified,
        ControlModel? ControlModel);
}