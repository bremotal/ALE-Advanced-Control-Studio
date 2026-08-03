using System;
using System.Collections.ObjectModel;
using ALE.ACS.Presentation.Models;
using ALE.ACS.Presentation.Services.Documents;
using ALE.ACS.Presentation.Services.Navigation;
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
    private readonly INavigationService _navigationService;
    private readonly IProjectSession _projectSession;

    private bool _isSyncingNavigation;
    private bool _isRefreshing;

    public WorkspaceViewModel(
        IDocumentService documentService,
        IProjectSession projectSession)
    {
        _documentService = documentService;
        _navigationService = NavigationService.Current;
        _projectSession = projectSession;

        _navigationService.Initialize(_documentService);
        _navigationService.NavigationChanged += OnNavigationChanged;

        Documents = new ObservableCollection<EngineeringDocument>(_documentService.Documents);

        ActiveDocument = _navigationService.ActiveDocument
                         ?? _documentService.ActiveDocument;

        _projectSession.CurrentProjectChanged += OnCurrentProjectChanged;
    }

    public ObservableCollection<EngineeringDocument> Documents { get; }

    public bool HasDocuments => Documents.Count > 0;

    [ObservableProperty]
    private EngineeringDocument? activeDocument;

    [ObservableProperty]
    private object? activeEngineeringEditorViewModel;

    partial void OnActiveDocumentChanged(EngineeringDocument? value)
    {
        if (_isSyncingNavigation)
            return;

        if (value is null)
            return;

        _navigationService.ActivateDocument(value.Id);

        OnPropertyChanged(nameof(HasDocuments));
    }

    public string WorkspaceTitle =>
        _projectSession.CurrentProject is null
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

        var closedTitle = ActiveDocument.Title;

        _documentService.CloseDocument(ActiveDocument.Id);
        _navigationService.NotifyDocumentClosed(closedTitle);

        RefreshDocuments();
    }

    [RelayCommand]
    private void CloseDocument(EngineeringDocument? document)
    {
        if (document is null)
            return;

        _documentService.CloseDocument(document.Id);
        _navigationService.NotifyDocumentClosed(document.Title);

        RefreshDocuments();
    }

    public void RefreshDocuments()
    {
        if (_isRefreshing)
            return;

        _isRefreshing = true;

        try
        {
            Documents.Clear();

            foreach (var document in _documentService.Documents)
            {
                Documents.Add(document);
            }

            _isSyncingNavigation = true;

            ActiveDocument =
                _navigationService.ActiveDocument
                ?? _documentService.ActiveDocument;

            ActiveEngineeringEditorViewModel = _navigationService.ActiveEditorViewModel;

            _isSyncingNavigation = false;

            OnPropertyChanged(nameof(WorkspaceTitle));
            OnPropertyChanged(nameof(HasDocuments));
        }
        finally
        {
            _isSyncingNavigation = false;
            _isRefreshing = false;
        }
    }

    private void OnCurrentProjectChanged(
        object? sender,
        ALE.ACS.Domain.Projects.Project? project)
    {
        OnPropertyChanged(nameof(WorkspaceTitle));
    }

    private void OnNavigationChanged(object? sender, EventArgs e)
    {
        if (_isRefreshing)
            return;

        _isSyncingNavigation = true;

        ActiveDocument =
            _navigationService.ActiveDocument
            ?? _documentService.ActiveDocument;

        ActiveEngineeringEditorViewModel = _navigationService.ActiveEditorViewModel;

        _isSyncingNavigation = false;

        OnPropertyChanged(nameof(HasDocuments));
    }
}