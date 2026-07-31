using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.Models;

/// <summary>
/// Representa um projeto de engenharia mantido em memória pela Presentation.
/// </summary>
public sealed partial class EngineeringProject : ObservableObject
{
    public EngineeringProject(
        string id,
        string name,
        string location,
        DateTime createdAt,
        DateTime lastModified,
        bool isDirty,
        IEnumerable<EngineeringDocument>? documents = null)
    {
        Id = id;
        Name = name;
        Location = location;
        CreatedAt = createdAt;
        LastModified = lastModified;
        IsDirty = isDirty;
        Documents = new ObservableCollection<EngineeringDocument>(documents ?? []);
    }

    public string Id { get; }

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string location;

    [ObservableProperty]
    private DateTime createdAt;

    [ObservableProperty]
    private DateTime lastModified;

    [ObservableProperty]
    private bool isDirty;

    public ObservableCollection<EngineeringDocument> Documents { get; }

    partial void OnNameChanged(string value)
    {
        MarkModified();
    }

    partial void OnLocationChanged(string value)
    {
        MarkModified();
    }

    public void MarkSaved()
    {
        IsDirty = false;
        LastModified = DateTime.Now;
    }

    public void MarkModified()
    {
        IsDirty = true;
        LastModified = DateTime.Now;
    }
}