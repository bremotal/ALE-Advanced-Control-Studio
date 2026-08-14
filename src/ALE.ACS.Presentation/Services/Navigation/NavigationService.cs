using ALE.ACS.Presentation.Controllers.Fuzzy.Model;
using ALE.ACS.Presentation.Controllers.Fuzzy.ViewModels;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Documents;

namespace ALE.ACS.Presentation.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    private IDocumentService? _documentService;

    private readonly Dictionary<string, FuzzyEditorViewModel> _fuzzyEditors =
        new(StringComparer.OrdinalIgnoreCase);

    public static INavigationService Current { get; } = new NavigationService();

    public TreeNode? SelectedNode { get; private set; }

    public EngineeringDocument? ActiveDocument { get; private set; }

    public object? ActiveEditorViewModel { get; private set; }

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
            ActiveEditorViewModel = null;

            RaiseNavigationChanged();
            return;
        }

        StatusMessage = $"Selected: {node.Name}";

        PropertiesMessage =
            $"Name: {node.Name}\n" +
            $"Type: {node.Type}\n" +
            $"Children: {node.Children.Count}";

        // ============================================================
        // FUZZY CONTROLLER
        // ============================================================
        // Fuzzy possui um editor especializado.
        if (IsFuzzyController(node))
        {
            OpenFuzzyEditor(node.Name);

            OutputMessage =
                $"Fuzzy Controller changed: {node.Name}. " +
                $"Document opened: {node.Name}.";

            RaiseNavigationChanged();
            return;
        }

        // ============================================================
        // MODEL
        // ============================================================
        // Model já possui um EngineeringDocument próprio.
        //
        // NÃO devemos chamar OpenDocument() aqui, pois isso criaria
        // outro documento genérico e perderia a referência ao
        // ControlModel associado ao documento original.
        if (node.Type == ProjectItemType.Model)
        {
            if (_documentService is not null)
            {
                var modelDocument = _documentService.Documents
                    .FirstOrDefault(document =>
                        document.Kind == "Model" &&
                        string.Equals(
                            document.Title,
                            node.Name,
                            StringComparison.OrdinalIgnoreCase));

                if (modelDocument is not null)
                {
                    ActivateDocument(modelDocument.Id);
                    return;
                }
            }

            ActiveDocument = null;
            ActiveEditorViewModel = null;

            OutputMessage = $"Model selected: {node.Name}.";

            RaiseNavigationChanged();
            return;
        }

        // ============================================================
        // DEMAIS NÓS
        // ============================================================
        // Para os demais tipos ainda utilizamos o documento genérico.
        if (_documentService is not null)
        {
            _documentService.OpenDocument(
                node.Name,
                $"Engineering workspace document for {node.Name} ({node.Type}).",
                node.Type.ToString());

            ActiveDocument = _documentService.ActiveDocument;
            ActiveEditorViewModel = null;

            OutputMessage =
                $"Node selected: {node.Name}. " +
                $"Document opened: {node.Name}.";
        }
        else
        {
            ActiveEditorViewModel = null;
            OutputMessage = $"Selected node: {node.Name}";
        }

        RaiseNavigationChanged();
    }

    public void ActivateDocument(string documentId)
    {
        if (_documentService is null)
        {
            return;
        }

        _documentService.SetActiveDocument(documentId);

        ActiveDocument = _documentService.ActiveDocument;

        if (ActiveDocument is null)
        {
            StatusMessage = "No active document.";
            OutputMessage = "Active document cleared.";
            ActiveEditorViewModel = null;

            RaiseNavigationChanged();
            return;
        }

        ActiveEditorViewModel = ResolveEditorViewModel(ActiveDocument);

        StatusMessage = $"Document: {ActiveDocument.Title}";

        PropertiesMessage =
            $"Name: {ActiveDocument.Title}\n" +
            $"Type: {ActiveDocument.Kind}\n" +
            $"Dirty: {(ActiveDocument.IsDirty ? "Yes" : "No")}";

        OutputMessage = $"Activated document: {ActiveDocument.Title}";

        RaiseNavigationChanged();
    }

    public void NotifyDocumentClosed(string documentTitle)
    {
        if (string.IsNullOrWhiteSpace(documentTitle))
        {
            return;
        }

        OutputMessage = $"Document closed: {documentTitle}";

        RaiseNavigationChanged();
    }

    public void NotifyValidation(
        string source,
        bool isValid,
        string message)
    {
        var status = isValid ? "OK" : "FAILED";

        OutputMessage =
            $"Validation [{source}] {status}: {message}";

        RaiseNavigationChanged();
    }

    private void RaiseNavigationChanged()
    {
        NavigationChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool IsFuzzyController(TreeNode node)
    {
        return string.Equals(
            node.Name,
            "Fuzzy Controller",
            StringComparison.OrdinalIgnoreCase);
    }

    private void OpenFuzzyEditor(string editorName)
    {
        if (_documentService is null)
        {
            return;
        }

        var model = CreateFuzzyControllerModel(editorName);

        if (!_fuzzyEditors.ContainsKey(editorName))
        {
            var viewModel = new FuzzyEditorViewModel();

            viewModel.LoadController(model);

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

        ActiveEditorViewModel = _fuzzyEditors[editorName];

        StatusMessage = $"Fuzzy editor: {editorName}";

        PropertiesMessage =
            $"Name: {model.Name}\n" +
            $"Type: Fuzzy Controller\n" +
            $"Inputs: {model.Inputs.Count}\n" +
            $"Outputs: {model.Outputs.Count}";

        OutputMessage = $"Document opened: {editorName}";
    }

    private object? ResolveEditorViewModel(
        EngineeringDocument document)
    {
        if (string.Equals(
                document.Kind,
                "Fuzzy",
                StringComparison.OrdinalIgnoreCase)
            && _fuzzyEditors.TryGetValue(
                document.Title,
                out var fuzzyViewModel))
        {
            return fuzzyViewModel;
        }

        return null;
    }

    private static FuzzyControllerModel CreateFuzzyControllerModel(
        string name)
    {
        return new FuzzyControllerModel(
            name,
            "Foundational Fuzzy module structure. Algorithmic behavior is intentionally not implemented.",
            ["Process Variable", "Setpoint"],
            ["Control Output"],
            ["Low", "Medium", "High"],
            ["IF error is High THEN output is High"]);
    }

    private static string BuildFuzzyDocumentContent(
        FuzzyControllerModel model)
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