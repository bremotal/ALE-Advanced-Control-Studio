using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.Engineering.Base;

public abstract partial class EngineeringEditorViewModel : ViewModels.ViewModelBase
{
    [ObservableProperty]
    private EngineeringDocument? document;

    public virtual void Load(EngineeringDocument document)
    {
        Document = document;
    }
}
