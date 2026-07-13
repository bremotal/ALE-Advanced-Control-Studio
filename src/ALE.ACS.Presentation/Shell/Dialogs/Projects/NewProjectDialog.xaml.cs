using System.Windows;
using ALE.ACS.Presentation.Events;
using ALE.ACS.Presentation.ViewModels.Projects;

namespace ALE.ACS.Presentation.Shell.Dialogs.Projects;

/// <summary>
/// Diálogo para criação de um novo projeto.
/// </summary>
public partial class NewProjectDialog : Window
{
    public NewProjectViewModel ViewModel { get; }

    public NewProjectDialog(NewProjectViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;

        DataContext = ViewModel;

        ViewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(
        object? sender,
        DialogCloseRequestedEventArgs e)
    {
        DialogResult = e.DialogResult;
    }
}