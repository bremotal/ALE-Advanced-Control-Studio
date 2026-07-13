using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Presentation.Services.Dialogs;
using ALE.ACS.Presentation.Shell.Dialogs.Projects;

namespace ALE.ACS.Presentation.ViewModels;

/// <summary>
/// ViewModel principal da aplicação.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IProjectService _projectService;

    [ObservableProperty]
    private Project? currentProject;

    public MainWindowViewModel(
        IDialogService dialogService,
        IProjectService projectService)
    {
        _dialogService = dialogService;
        _projectService = projectService;
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