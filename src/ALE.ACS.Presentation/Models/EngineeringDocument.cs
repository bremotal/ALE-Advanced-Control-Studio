using CommunityToolkit.Mvvm.ComponentModel;

namespace ALE.ACS.Presentation.Models;

/// <summary>
/// Documento genérico utilizado pela área de trabalho da IDE.
/// </summary>
public sealed partial class EngineeringDocument : ObservableObject
{
    public EngineeringDocument(string id, string title, string content, string kind)
    {
        Id = id;
        Title = title;
        Content = content;
        Kind = kind;
        LastModified = DateTime.Now;
    }

    public string Id { get; }

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string content;

    [ObservableProperty]
    private string kind;

    [ObservableProperty]
    private bool isDirty;

    [ObservableProperty]
    private DateTime lastModified;

    partial void OnTitleChanged(string value)
    {
        MarkDirty();
    }

    partial void OnContentChanged(string value)
    {
        MarkDirty();
    }

    private void MarkDirty()
    {
        if (IsDirty)
            return;

        IsDirty = true;
        LastModified = DateTime.Now;
    }
}
