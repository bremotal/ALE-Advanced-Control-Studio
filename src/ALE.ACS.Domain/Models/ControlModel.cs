using System.Collections.ObjectModel;

namespace ALE.ACS.Domain.Models;

public sealed class ControlModel
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ObservableCollection<ModelVariable> Variables { get; } = [];
}