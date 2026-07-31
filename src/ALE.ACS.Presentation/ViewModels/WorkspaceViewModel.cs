using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Documents;
using ALE.ACS.Presentation.Services.Session;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ALE.ACS.Presentation.ViewModels;

/// <summary>
/// ViewModel da workspace que expõe documentos abertos e o documento ativo.
/// </summary>
public partial class WorkspaceViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IProjectSession _projectSession;

    public WorkspaceViewModel(IDocumentService documentService, IProjectSession projectSession)
    {
        _documentService = documentService;
        _projectSession = projectSession;

        Documents = new ObservableCollection<EngineeringDocument>(_documentService.Documents);
        ActiveDocument = _documentService.ActiveDocument;

        _projectSession.CurrentProjectChanged += OnCurrentProjectChanged;
    }

    public ObservableCollection<EngineeringDocument> Documents { get; }

    [ObservableProperty]
    private EngineeringDocument? activeDocument;

    public string WorkspaceTitle => _projectSession.CurrentProject is null
        ? "Workspace"
        : $"Workspace - {_projectSession.CurrentProject.Name}";

    [RelayCommand]
    private void OpenSampleDocument()
    {
        _documentService.OpenDocument(
            "Engineering Notes",
            "This is a lightweight engineering document placeholder for the SPR-004 workspace infrastructure.",
            "Engineering");

        RefreshDocuments();
    }

    [RelayCommand]
    private void CloseActiveDocument()
    {
        if (ActiveDocument is null)
            return;

        _documentService.CloseDocument(ActiveDocument.Id);
        RefreshDocuments();
    }

    public void RefreshDocuments()
    {
        Documents.Clear();

        foreach (var document in _documentService.Documents)
        {
            Documents.Add(document);
        }

        ActiveDocument = _documentService.ActiveDocument;
        OnPropertyChanged(nameof(WorkspaceTitle));
    }

    private void OnCurrentProjectChanged(object? sender, ALE.ACS.Domain.Projects.Project? project)
    {
        OnPropertyChanged(nameof(WorkspaceTitle));
    }
}
