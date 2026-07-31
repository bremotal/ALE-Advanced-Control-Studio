using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Documents;

/// <summary>
/// Serviço responsável por gerenciar documentos abertos na workspace.
/// </summary>
public interface IDocumentService
{
    IReadOnlyList<EngineeringDocument> Documents { get; }

    EngineeringDocument? ActiveDocument { get; }

    void OpenDocument(string title, string content, string kind = "Document");

    void SetActiveDocument(string id);

    void CloseDocument(string id);
}
