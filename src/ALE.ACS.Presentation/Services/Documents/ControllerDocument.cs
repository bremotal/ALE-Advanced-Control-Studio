namespace ALE.ACS.Presentation.Services.Documents;

/// <summary>
/// Represents a controller document descriptor for workspace tabs.
/// </summary>
/// <param name="Id">Controller identifier.</param>
/// <param name="Name">Controller display name.</param>
/// <param name="Type">Controller type name.</param>
public sealed record ControllerDocument(Guid Id, string Name, string Type);
