using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Documents;

namespace ALE.ACS.Presentation.Services.Navigation;

public interface INavigationService
{
    TreeNode? SelectedNode { get; }

    EngineeringDocument? ActiveDocument { get; }

    object? ActiveEditorViewModel { get; }

    string StatusMessage { get; }

    string PropertiesMessage { get; }

    string OutputMessage { get; }

    event EventHandler? NavigationChanged;

    void Initialize(IDocumentService documentService);

    void SelectNode(TreeNode? node);

    void ActivateDocument(string documentId);

    void NotifyDocumentClosed(string documentTitle);

    void NotifyValidation(string source, bool isValid, string message);
}