using System.Collections.ObjectModel;

namespace ALE.ACS.Presentation.Controllers.MPC.Model;

public sealed class MpcControllerModel
{
    public string Name { get; set; } = "MPC Controller";

    public ObservableCollection<string> Inputs { get; } = [];

    public ObservableCollection<string> Outputs { get; } = [];

    public int PredictionHorizon { get; set; } = 10;

    public int ControlHorizon { get; set; } = 5;

    public ObservableCollection<string> Constraints { get; } = [];
}
