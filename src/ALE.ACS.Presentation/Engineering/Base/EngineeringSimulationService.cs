namespace ALE.ACS.Presentation.Engineering.Base;

public sealed class EngineeringSimulationService
{
    public string CreateSimulationDraft(EngineeringDocument? document)
    {
        if (document is null)
            return "No engineering document loaded.";

        return $"Simulation draft prepared for '{document.Name}'.";
    }
}
