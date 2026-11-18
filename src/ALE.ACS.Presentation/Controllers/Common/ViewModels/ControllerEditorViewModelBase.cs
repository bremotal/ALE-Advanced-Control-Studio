using ALE.ACS.Presentation.Controllers.Common.Model;
using ALE.ACS.Presentation.Engineering.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ALE.ACS.Presentation.Controllers.Common.ViewModels;

/// <summary>
/// Base para todos os editores de controlador que usam o modelo genérico
/// <see cref="ControllerEditorModel"/> com seções de texto.
/// </summary>
/// <remarks>
/// Fornece commands Add/Remove por seção, com CanExecute vinculado ao campo de
/// texto e à seleção. Os editores concretos (PID, MPC, etc.) só precisam criar
/// o <see cref="ControllerEditorModel"/> com as seções apropriadas no construtor.
/// </remarks>
public abstract partial class ControllerEditorViewModelBase : EngineeringEditorViewModel
{
    [ObservableProperty]
    private ControllerEditorModel _controller;

    // Item selecionado na seção ativa (a UI seleciona via tab/card ativo).
    [ObservableProperty]
    private ControllerSection? _selectedSection;

    [ObservableProperty]
    private string? _selectedItem;

    [ObservableProperty]
    private string _newItemText = string.Empty;

    protected ControllerEditorViewModelBase(ControllerEditorModel controller)
    {
        _controller = controller;
        SelectedSection = controller.Sections.FirstOrDefault();
    }

    /// <summary>Adiciona um item à seção ativa.</summary>
    [RelayCommand(CanExecute = nameof(CanAddItem))]
    private void AddItem()
    {
        if (SelectedSection is null)
        {
            return;
        }

        var value = NewItemText?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (SelectedSection.Items.Contains(value))
        {
            return;
        }

        SelectedSection.Items.Add(value);
        NewItemText = string.Empty;
    }

    private bool CanAddItem()
        => SelectedSection is not null && !string.IsNullOrWhiteSpace(NewItemText);

    /// <summary>Remove o item selecionado da seção ativa.</summary>
    [RelayCommand(CanExecute = nameof(CanRemoveItem))]
    private void RemoveItem()
    {
        if (SelectedSection is null || SelectedItem is null)
        {
            return;
        }

        SelectedSection.Items.Remove(SelectedItem);
        SelectedItem = null;
    }

    private bool CanRemoveItem()
        => SelectedSection is not null && SelectedItem is not null;

    partial void OnNewItemTextChanged(string value)
        => AddItemCommand.NotifyCanExecuteChanged();

    partial void OnSelectedItemChanged(string? value)
        => RemoveItemCommand.NotifyCanExecuteChanged();

    partial void OnSelectedSectionChanged(ControllerSection? value)
    {
        SelectedItem = null;
        AddItemCommand.NotifyCanExecuteChanged();
        RemoveItemCommand.NotifyCanExecuteChanged();
    }
}
