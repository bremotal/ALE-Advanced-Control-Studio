using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Documents;

namespace ALE.ACS.Presentation.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    private IDocumentService? _documentService;

    public static INavigationService Current { get; } = new NavigationService();

    public TreeNode? SelectedNode { get; private set; }

    public EngineeringDocument? ActiveDocument { get; private set; }

    public string StatusMessage { get; private set; } = "Ready";

    public string PropertiesMessage { get; private set; } = "No selection.";

    public string OutputMessage { get; private set; } = "Output";

    public event EventHandler? NavigationChanged;

    public void Initialize(IDocumentService documentService)
    {
        _documentService ??= documentService;
        ActiveDocument = _documentService.ActiveDocument;
    }

    public void SelectNode(TreeNode? node)
    {
        SelectedNode = node;

        if (node is null)
        {
            StatusMessage = "Ready";
            PropertiesMessage = "No selection.";
            OutputMessage = "Selection cleared.";
            RaiseNavigationChanged();
            return;
        }

        StatusMessage = $"Selected: {node.Name}";
        PropertiesMessage = $"Name: {node.Name}\nType: {node.Type}\nChildren: {node.Children.Count}";

        if (_documentService is not null)
        {
            _documentService.OpenDocument(
                node.Name,
                $"Engineering workspace document for {node.Name} ({node.Type}).",
                node.Type.ToString());

            ActiveDocument = _documentService.ActiveDocument;
            OutputMessage = $"Opened document: {node.Name}";
        }
        else
        {
            OutputMessage = $"Selected node: {node.Name}";
        }

        RaiseNavigationChanged();
    }

    public void ActivateDocument(string documentId)
    {
        if (_documentService is null)
            return;

        _documentService.SetActiveDocument(documentId);
        ActiveDocument = _documentService.ActiveDocument;

        if (ActiveDocument is null)
        {
            StatusMessage = "No active document.";
            OutputMessage = "Active document cleared.";
            RaiseNavigationChanged();
            return;
        }

        StatusMessage = $"Document: {ActiveDocument.Title}";
        PropertiesMessage = $"Name: {ActiveDocument.Title}\nType: {ActiveDocument.Kind}\nDirty: {(ActiveDocument.IsDirty ? "Yes" : "No")}";
        OutputMessage = $"Activated document: {ActiveDocument.Title}";

        RaiseNavigationChanged();
    }

    private void RaiseNavigationChanged()
    {
        NavigationChanged?.Invoke(this, EventArgs.Empty);
    }
}