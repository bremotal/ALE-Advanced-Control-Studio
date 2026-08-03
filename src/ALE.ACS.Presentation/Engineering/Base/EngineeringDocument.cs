using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.Engineering.Base;

public partial class EngineeringDocument : ObservableObject
{
    public EngineeringDocument(string name, string description)
    {
        Id = Guid.NewGuid().ToString("N");
        Name = name;
        Description = description;
        UpdatedAt = DateTime.Now;
    }

    public string Id { get; }

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private bool isDirty;

    [ObservableProperty]
    private DateTime updatedAt;

    protected void MarkUpdated()
    {
        IsDirty = true;
        UpdatedAt = DateTime.Now;
    }
}
