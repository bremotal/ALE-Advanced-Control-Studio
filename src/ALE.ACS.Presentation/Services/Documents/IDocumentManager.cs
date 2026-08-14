using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Documents;

/// <summary>
/// Manages opened documents in the IDE workspace.
/// </summary>
public interface IDocumentManager
{
    /// <summary>
    /// Gets the currently opened documents.
    /// </summary>
    ObservableCollection<EngineeringDocument> OpenDocuments { get; }

    /// <summary>
    /// Opens a document for a controller.
    /// </summary>
    /// <param name="controller">Controller descriptor.</param>
    void OpenDocument(ControllerDocument controller);

    /// <summary>
    /// Closes an opened document by id.
    /// </summary>
    /// <param name="id">Document id.</param>
    void CloseDocument(Guid id);
}
