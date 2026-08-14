namespace ALE.ACS.Domain.Models;

public sealed class ModelVariable
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public ModelVariableType Type { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double Minimum { get; set; }

    public double Maximum { get; set; }
}