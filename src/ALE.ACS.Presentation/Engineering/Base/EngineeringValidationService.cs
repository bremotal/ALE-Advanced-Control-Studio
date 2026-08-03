namespace ALE.ACS.Presentation.Engineering.Base;

public sealed class EngineeringValidationService
{
    public IReadOnlyList<string> Validate(EngineeringDocument? document)
    {
        if (document is null)
            return ["No engineering document loaded."];

        var messages = new List<string>();

        if (string.IsNullOrWhiteSpace(document.Name))
            messages.Add("Name is required.");

        if (string.IsNullOrWhiteSpace(document.Description))
            messages.Add("Description is required.");

        return messages;
    }
}
