using ALE.ACS.Presentation.Controllers.Fuzzy.Model;
using ALE.ACS.Presentation.Engineering.Base;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.Controllers.Fuzzy.ViewModels;

public partial class FuzzyEditorViewModel : EngineeringEditorViewModel
{
    [ObservableProperty]
    private FuzzyControllerModel controller;

    public FuzzyEditorViewModel()
    {
        Controller = CreateDefault();
    }

    public void LoadController(FuzzyControllerModel model)
    {
        Controller = model;
    }

    private static FuzzyControllerModel CreateDefault()
    {
        return new FuzzyControllerModel(
            "Fuzzy Controller",
            "Fuzzy controller engineering draft.",
            ["Process Variable", "Setpoint"],
            ["Control Output"],
            ["Low", "Medium", "High"],
            ["IF error is High THEN output is High"]);
    }
}
