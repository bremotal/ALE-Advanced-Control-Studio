using System.Collections.ObjectModel;

namespace ALE.ACS.Presentation.Engineering.Base;

public abstract class EngineeringToolboxViewModel : ViewModels.ViewModelBase
{
    public ObservableCollection<string> Tools { get; } = [];
}
