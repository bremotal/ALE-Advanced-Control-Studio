using ALE.ACS.Presentation.Controllers.Fuzzy.Model;
using ALE.ACS.Presentation.Engineering.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ALE.ACS.Presentation.Controllers.Fuzzy.ViewModels;

/// <summary>
/// Editor do controlador Fuzzy.
/// Expõe o <see cref="Controller"/> ao XAML e commands de Add/Remove para cada
/// coleção (Inputs, Outputs, MembershipFunctions, RuleBase), com CanExecute
/// vinculado aos campos de texto/seleção.
/// </summary>
public partial class FuzzyEditorViewModel : EngineeringEditorViewModel
{
    [ObservableProperty]
    private FuzzyControllerModel _controller;

    // ===== Campos de texto para adicionar novos itens =====
    [ObservableProperty]
    private string _newInput = string.Empty;

    [ObservableProperty]
    private string _newOutput = string.Empty;

    [ObservableProperty]
    private string _newMembershipFunction = string.Empty;

    [ObservableProperty]
    private string _newRule = string.Empty;

    // ===== Itens selecionados (para Remove) =====
    [ObservableProperty]
    private string? _selectedInput;

    [ObservableProperty]
    private string? _selectedOutput;

    [ObservableProperty]
    private string? _selectedMembershipFunction;

    [ObservableProperty]
    private string? _selectedRule;

    public FuzzyEditorViewModel()
    {
        _controller = CreateDefault();
    }

    public void LoadController(FuzzyControllerModel model)
    {
        Controller = model;
    }

    // ===== Inputs =====

    [RelayCommand(CanExecute = nameof(CanAddInput))]
    private void AddInput()
    {
        var value = NewInput?.Trim();
        if (string.IsNullOrEmpty(value)) return;
        if (Controller.Inputs.Contains(value)) return;
        Controller.Inputs.Add(value);
        NewInput = string.Empty;
    }

    private bool CanAddInput() => !string.IsNullOrWhiteSpace(NewInput);

    [RelayCommand(CanExecute = nameof(CanRemoveInput))]
    private void RemoveInput()
    {
        if (SelectedInput is null) return;
        Controller.Inputs.Remove(SelectedInput);
        SelectedInput = null;
    }

    private bool CanRemoveInput() => SelectedInput is not null;

    // ===== Outputs =====

    [RelayCommand(CanExecute = nameof(CanAddOutput))]
    private void AddOutput()
    {
        var value = NewOutput?.Trim();
        if (string.IsNullOrEmpty(value)) return;
        if (Controller.Outputs.Contains(value)) return;
        Controller.Outputs.Add(value);
        NewOutput = string.Empty;
    }

    private bool CanAddOutput() => !string.IsNullOrWhiteSpace(NewOutput);

    [RelayCommand(CanExecute = nameof(CanRemoveOutput))]
    private void RemoveOutput()
    {
        if (SelectedOutput is null) return;
        Controller.Outputs.Remove(SelectedOutput);
        SelectedOutput = null;
    }

    private bool CanRemoveOutput() => SelectedOutput is not null;

    // ===== Membership Functions =====

    [RelayCommand(CanExecute = nameof(CanAddMembershipFunction))]
    private void AddMembershipFunction()
    {
        var value = NewMembershipFunction?.Trim();
        if (string.IsNullOrEmpty(value)) return;
        if (Controller.MembershipFunctions.Contains(value)) return;
        Controller.MembershipFunctions.Add(value);
        NewMembershipFunction = string.Empty;
    }

    private bool CanAddMembershipFunction() => !string.IsNullOrWhiteSpace(NewMembershipFunction);

    [RelayCommand(CanExecute = nameof(CanRemoveMembershipFunction))]
    private void RemoveMembershipFunction()
    {
        if (SelectedMembershipFunction is null) return;
        Controller.MembershipFunctions.Remove(SelectedMembershipFunction);
        SelectedMembershipFunction = null;
    }

    private bool CanRemoveMembershipFunction() => SelectedMembershipFunction is not null;

    // ===== Rule Base =====

    [RelayCommand(CanExecute = nameof(CanAddRule))]
    private void AddRule()
    {
        var value = NewRule?.Trim();
        if (string.IsNullOrEmpty(value)) return;
        if (Controller.RuleBase.Contains(value)) return;
        Controller.RuleBase.Add(value);
        NewRule = string.Empty;
    }

    private bool CanAddRule() => !string.IsNullOrWhiteSpace(NewRule);

    [RelayCommand(CanExecute = nameof(CanRemoveRule))]
    private void RemoveRule()
    {
        if (SelectedRule is null) return;
        Controller.RuleBase.Remove(SelectedRule);
        SelectedRule = null;
    }

    private bool CanRemoveRule() => SelectedRule is not null;

    // ===== Reavalia CanExecute dos commands quando os campos mudam =====
    // O CommunityToolkit.Mvvm já notifica CommandManager para [ObservableProperty],
    // mas chamamos NotifyCanExecuteChanged explicitamente para forçar
    // reavaliação imediata dos botões na UI.

    partial void OnNewInputChanged(string value) => AddInputCommand.NotifyCanExecuteChanged();
    partial void OnNewOutputChanged(string value) => AddOutputCommand.NotifyCanExecuteChanged();
    partial void OnNewMembershipFunctionChanged(string value) => AddMembershipFunctionCommand.NotifyCanExecuteChanged();
    partial void OnNewRuleChanged(string value) => AddRuleCommand.NotifyCanExecuteChanged();

    partial void OnSelectedInputChanged(string? value) => RemoveInputCommand.NotifyCanExecuteChanged();
    partial void OnSelectedOutputChanged(string? value) => RemoveOutputCommand.NotifyCanExecuteChanged();
    partial void OnSelectedMembershipFunctionChanged(string? value) => RemoveMembershipFunctionCommand.NotifyCanExecuteChanged();
    partial void OnSelectedRuleChanged(string? value) => RemoveRuleCommand.NotifyCanExecuteChanged();

    // ===== Default =====

    private static FuzzyControllerModel CreateDefault()
    {
        return new FuzzyControllerModel(
            "Fuzzy Controller",
            "Fuzzy controller engineering draft.",
            ["Process Variable", "Setpoint"],
            ["Control Output"],
            ["Low", "Medium", "High"],
            ["IF error is High THEN output is High"]);
    }
}
