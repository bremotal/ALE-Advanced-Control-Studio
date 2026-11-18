using ALE.ACS.Domain.Common.Base;
using ALE.ACS.Domain.Common.Results;

namespace ALE.ACS.Domain.Models;

/// <summary>
/// Tipo de variável de processo dentro de um modelo de engenharia.
/// </summary>

/// <summary>
/// Variável de processo dentro de um modelo de engenharia.
/// Herda de <see cref="EntityBase"/> para ter identidade, auditoria
/// e métodos de renomeação consistentes com o resto do domínio.
/// </summary>
public sealed class ModelVariable : EntityBase
{
    private ModelVariable()
    {
    }

    /// <summary>
    /// Tipo da variável (CV/MV/DV/Parameter).
    /// </summary>
    public ModelVariableType Type { get; private set; }

    /// <summary>
    /// Unidade de engenharia (°C, bar, %, kg/h, ...).
    /// </summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>
    /// Valor mínimo operacional.
    /// </summary>
    public double Minimum { get; private set; }

    /// <summary>
    /// Valor máximo operacional.
    /// </summary>
    public double Maximum { get; private set; }

    /// <summary>
    /// Valor nominal (ponto de operação).
    /// </summary>
    public double Nominal { get; private set; }

    /// <summary>
    /// Cria uma nova variável de modelo.
    /// </summary>
    public static Result<ModelVariable> Create(
        string name,
        ModelVariableType type,
        string unit = "",
        double minimum = 0,
        double maximum = 0,
        double nominal = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ModelVariable>.Failure(
                "O nome da variável é obrigatório.");
        }

        if (maximum < minimum)
        {
            return Result<ModelVariable>.Failure(
                "O valor máximo não pode ser inferior ao mínimo.");
        }

        var variable = new ModelVariable
        {
            Type = type,
            Unit = unit?.Trim() ?? string.Empty,
            Minimum = minimum,
            Maximum = maximum,
            Nominal = nominal
        };

        variable.Rename(name);

        return Result<ModelVariable>.Success(variable);
    }

    /// <summary>
    /// Define os limites operacionais e o ponto nominal.
    /// </summary>
    public void SetRange(double minimum, double maximum, double nominal)
    {
        if (maximum < minimum)
        {
            throw new ArgumentException(
                "O valor máximo não pode ser inferior ao mínimo.");
        }

        Minimum = minimum;
        Maximum = maximum;
        Nominal = nominal;

        Touch();
    }

    /// <summary>
    /// Define a unidade de engenharia.
    /// </summary>
    public void SetUnit(string unit)
    {
        Unit = unit?.Trim() ?? string.Empty;

        Touch();
    }
}
