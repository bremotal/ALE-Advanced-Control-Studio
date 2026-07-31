using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Models.Fuzzy;
using ALE.ACS.Presentation.Services.Documents;
using ALE.ACS.Presentation.ViewModels.Fuzzy;
using ALE.ACS.Presentation.Views.Fuzzy;

namespace ALE.ACS.Presentation.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    private IDocumentService? _documentService;
    private readonly Dictionary<string, FuzzyEditorViewModel> _fuzzyEditors = new(StringComparer.OrdinalIgnoreCase);

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

        if (IsFuzzyController(node))
        {
            OpenFuzzyEditor(node.Name);
            RaiseNavigationChanged();
            return;
        }

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

    private static bool IsFuzzyController(TreeNode node)
    {
        return string.Equals(node.Name, "Fuzzy Controller", StringComparison.OrdinalIgnoreCase);
    }

    private void OpenFuzzyEditor(string editorName)
    {
        if (_documentService is null)
            return;

        var model = CreateFuzzyControllerModel(editorName);

        if (!_fuzzyEditors.ContainsKey(editorName))
        {
            var viewModel = new FuzzyEditorViewModel();
            viewModel.LoadController(model);

            // Keep the view-model instance available for future editor integration.
            _ = new FuzzyEditorView { DataContext = viewModel };
            _fuzzyEditors[editorName] = viewModel;
        }
        else
        {
            _fuzzyEditors[editorName].LoadController(model);
        }

        _documentService.OpenDocument(
            editorName,
            BuildFuzzyDocumentContent(model),
            "Fuzzy");

        ActiveDocument = _documentService.ActiveDocument;
        StatusMessage = $"Fuzzy editor: {editorName}";
        PropertiesMessage = $"Name: {model.Name}\nType: Fuzzy Controller\nInputs: {model.Inputs.Count}\nOutputs: {model.Outputs.Count}";
        OutputMessage = $"Fuzzy editor opened for {editorName}.";
    }

    private static FuzzyControllerModel CreateFuzzyControllerModel(string name)
    {
        return new FuzzyControllerModel(
            name,
            "Foundational Fuzzy module structure. Algorithmic behavior is intentionally not implemented.",
            ["Process Variable", "Setpoint"],
            ["Control Output"],
            ["Low", "Medium", "High"],
            ["IF error is High THEN output is High"]);
    }

    private static string BuildFuzzyDocumentContent(FuzzyControllerModel model)
    {
        return $"Fuzzy Editor\n"
             + $"Name: {model.Name}\n"
             + $"Description: {model.Description}\n\n"
             + $"Inputs: {string.Join(", ", model.Inputs)}\n"
             + $"Outputs: {string.Join(", ", model.Outputs)}\n"
             + $"Membership Functions: {string.Join(", ", model.MembershipFunctions)}\n"
             + $"Rule Base: {string.Join(" | ", model.RuleBase)}";
    }
}