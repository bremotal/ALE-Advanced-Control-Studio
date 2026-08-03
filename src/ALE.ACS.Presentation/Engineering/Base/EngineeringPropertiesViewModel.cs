using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.Engineering.Base;

public abstract partial class EngineeringPropertiesViewModel : ViewModels.ViewModelBase
{
    [ObservableProperty]
    private string title = "Properties";

    [ObservableProperty]
    private string summary = "No engineering document selected.";
}
