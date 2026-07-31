using ALE.ACS.Presentation.Models.Fuzzy;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.ViewModels.Fuzzy;

public partial class FuzzyEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private FuzzyControllerModel controller;

    public FuzzyEditorViewModel()
    {
        Controller = CreateDefaultController();
    }

    public void LoadController(FuzzyControllerModel controller)
    {
        Controller = controller;
    }

    private static FuzzyControllerModel CreateDefaultController()
    {
        return new FuzzyControllerModel(
            "Fuzzy Controller",
            "Foundational editor model for a fuzzy controller module.",
            ["Process Variable", "Setpoint"],
            ["Control Output"],
            ["Low", "Medium", "High"],
            ["IF error is High THEN output is High"]);
    }
}
