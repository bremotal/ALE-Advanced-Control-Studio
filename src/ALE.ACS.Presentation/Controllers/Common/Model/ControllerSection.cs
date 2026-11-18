using System.Collections.ObjectModel;

namespace ALE.ACS.Presentation.Controllers.Common.Model;

/// <summary>
/// Modelo genérico de uma seção de um editor de controlador.
/// Cada seção tem nome (ex.: "Inputs", "Outputs", "Regras"), uma lista de
/// itens de texto e um placeholder que descreve o propósito da seção.
/// </summary>
/// <remarks>
/// Permite que editores de controladores diferentes (PID, MPC, etc.) reaproveitem
/// o mesmo XAML base, diferindo apenas na configuração das seções.
/// </remarks>
public sealed class ControllerSection
{
    public ControllerSection(
        string title,
        string icon,
        string hint,
        IEnumerable<string>? items = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(icon);

        Title = title;
        Icon = icon;
        Hint = hint;
        Items = new ObservableCollection<string>(items ?? []);
    }

    /// <summary>Título da seção (ex.: "Inputs", "Outputs", "Regras").</summary>
    public string Title { get; }

    /// <summary>Ícone/glyph exibido antes do título.</summary>
    public string Icon { get; }

    /// <summary>Descrição curta exibida abaixo da lista.</summary>
    public string Hint { get; }

    /// <summary>Itens da seção.</summary>
    public ObservableCollection<string> Items { get; }
}

/// <summary>
/// Modelo genérico de um controlador para a UI.
/// Carrega o nome, descrição e uma lista de <see cref="ControllerSection"/>s.
/// </summary>
public sealed class ControllerEditorModel
{
    public ControllerEditorModel(
        string name,
        string description,
        string controllerKind,
        IEnumerable<ControllerSection>? sections = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(controllerKind);

        Name = name;
        Description = description;
        ControllerKind = controllerKind;
        Sections = new ObservableCollection<ControllerSection>(sections ?? []);
    }

    public string Name { get; set; }

    public string Description { get; set; }

    /// <summary>Rótulo legível do tipo de controlador (ex.: "PID", "MPC", "Fuzzy").</summary>
    public string ControllerKind { get; set; }

    public ObservableCollection<ControllerSection> Sections { get; }
}
