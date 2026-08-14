using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Documents;

/// <summary>
/// Implementação simples do serviço de documentos para a workspace.
/// </summary>
public sealed class DocumentService : IDocumentService, IDocumentManager
{
    private readonly ObservableCollection<EngineeringDocument> _documents = [];

    public IReadOnlyList<EngineeringDocument> Documents => _documents;

    public ObservableCollection<EngineeringDocument> OpenDocuments => _documents;

    public EngineeringDocument? ActiveDocument { get; private set; }

    public void OpenDocument(ControllerDocument controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var existingDocument = _documents.FirstOrDefault(document => document.Id == controller.Id.ToString("N"));

        if (existingDocument is not null)
        {
            SetActiveDocument(existingDocument.Id);
            return;
        }

        var document = new EngineeringDocument(
            controller.Id.ToString("N"),
            controller.Name,
            $"{controller.Type} design workspace",
            controller.Type);

        _documents.Add(document);
        SetActiveDocument(document.Id);
    }

    public void OpenDocument(string title, string content, string kind = "Document")
    {
        var existingDocument = _documents.FirstOrDefault(document => string.Equals(document.Title, title, StringComparison.OrdinalIgnoreCase));

        if (existingDocument is not null)
        {
            SetActiveDocument(existingDocument.Id);
            return;
        }

        var document = new EngineeringDocument(
            Guid.NewGuid().ToString("N"),
            title,
            content,
            kind);

        _documents.Add(document);
        SetActiveDocument(document.Id);
    }

    public void SetActiveDocument(string id)
    {
        ActiveDocument = _documents.FirstOrDefault(document => document.Id == id);
    }

    public void CloseDocument(string id)
    {
        var document = _documents.FirstOrDefault(item => item.Id == id);

        if (document is null)
            return;

        _documents.Remove(document);

        if (ActiveDocument?.Id == id)
        {
            ActiveDocument = _documents.LastOrDefault();
        }
    }

    public void CloseDocument(Guid id)
    {
        CloseDocument(id.ToString("N"));
    }
}
