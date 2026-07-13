using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ALE.ACS.Presentation.Events;

namespace ALE.ACS.Presentation.ViewModels.Projects;

/// <summary>
/// ViewModel responsável pelo diálogo de criação de projeto.
/// Apenas coleta os dados informados pelo usuário.
/// </summary>
public partial class NewProjectViewModel : ViewModelBase
{
    [ObservableProperty]
    private string projectName = string.Empty;

    public bool CanCreate =>
        !string.IsNullOrWhiteSpace(ProjectName);

    public event EventHandler<DialogCloseRequestedEventArgs>? RequestClose;

    partial void OnProjectNameChanged(string value)
    {
        CreateCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create()
    {
        RequestClose?.Invoke(
            this,
            new DialogCloseRequestedEventArgs(true));
    }
}