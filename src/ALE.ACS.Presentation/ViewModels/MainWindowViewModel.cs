using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Projects;
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

    private const string DefaultWindowTitle = "ALE Advanced Control Studio";

    [ObservableProperty]
    private Project? currentProject;

    public WorkspaceView WorkspaceView { get; }

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

        _navigationService.NavigationChanged += OnNavigationChanged;

        WorkspaceView = new WorkspaceView
        {
            DataContext = workspaceViewModel
        };
    }

    partial void OnCurrentProjectChanged(Project? value)
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(WorkspaceContent));
        OnPropertyChanged(nameof(PropertiesContent));
        OnPropertyChanged(nameof(OutputContent));

        if (value is null)
        {
            _projectSession.CloseProject();
            return;
        }

        _projectSession.SetCurrentProject(value);
    }

    private void OnNavigationChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(PropertiesContent));
        OnPropertyChanged(nameof(OutputContent));
    }

    [RelayCommand]
    private void NewProject()
    {
        var dialog = _dialogService.ShowDialog<NewProjectDialog>();

        if (dialog.DialogResult != true)
            return;

        var projectName = dialog.ViewModel.ProjectName;

        var result = _projectService.Create(projectName);

        if (!result.IsSuccess)
        {
            System.Windows.MessageBox.Show(
                result.Error!,
                "ALE ACS",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);

            return;
        }

        CurrentProject = result.Value;
    }
}