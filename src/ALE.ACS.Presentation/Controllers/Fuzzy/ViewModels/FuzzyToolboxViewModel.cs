using ALE.ACS.Presentation.Engineering.Base;

namespace ALE.ACS.Presentation.Controllers.Fuzzy.ViewModels;

public sealed class FuzzyToolboxViewModel : EngineeringToolboxViewModel
{
    public FuzzyToolboxViewModel()
    {
        Tools.Add("New Membership Function");
        Tools.Add("Add Rule");
        Tools.Add("Validate");
    }
}
