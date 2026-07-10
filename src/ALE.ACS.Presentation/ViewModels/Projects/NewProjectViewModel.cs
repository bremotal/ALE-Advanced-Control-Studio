using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.ViewModels.Projects;

/// <summary>
/// ViewModel responsável pela criação de um novo projeto.
/// </summary>
public partial class NewProjectViewModel : ObservableObject
{
    [ObservableProperty]
    private string projectName = string.Empty;
}