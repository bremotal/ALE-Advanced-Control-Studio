using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Presentation.Services.Dialogs;
using ALE.ACS.Presentation.Services.Session;
using ALE.ACS.Presentation.Shell.Dialogs.Projects;

namespace ALE.ACS.Presentation.ViewModels;

/// <summary>
/// ViewModel principal da aplicação.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IProjectService _projectService;
    private readonly IProjectSession _projectSession;

    private const string DefaultWindowTitle = "ALE Advanced Control Studio";

    [ObservableProperty]
    private Project? currentProject;

    /// <summary>
    /// Título exibido na janela principal.
    /// </summary>
    public string WindowTitle =>
        CurrentProject is null
            ? DefaultWindowTitle
            : $"{DefaultWindowTitle} - {CurrentProject.Name}";

    public MainWindowViewModel(
        IDialogService dialogService,
        IProjectService projectService,
        IProjectSession projectSession)
    {
        _dialogService = dialogService;
        _projectService = projectService;
        _projectSession = projectSession;
    }

    partial void OnCurrentProjectChanged(Project? value)
    {
        OnPropertyChanged(nameof(WindowTitle));

        if (value is null)
        {
            _projectSession.CloseProject();
            return;
        }

        _projectSession.SetCurrentProject(value);
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