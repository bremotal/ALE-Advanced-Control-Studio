using System.Collections.ObjectModel;
using ALE.ACS.Domain.Common.Base;
using ALE.ACS.Domain.Common.Results;

namespace ALE.ACS.Domain.Models;

/// <summary>
/// Aggregate root representando o modelo de engenharia de um processo.
/// </summary>
/// <remarks>
/// Carrega a coleção de variáveis de processo (CVs, MVs, DVs, parâmetros)
/// e ganhos/dinâmica resumidos que alimentam o design dos controladores.
/// É o filho direto de <see cref="ALE.ACS.Domain.Projects.Project"/>.
/// </remarks>
public sealed class ControlModel : AggregateRoot
{
    private readonly ObservableCollection<ModelVariable> _variables = [];

    private ControlModel()
    {
    }

    /// <summary>
    /// Variáveis de processo do modelo.
    /// </summary>
    public IReadOnlyCollection<ModelVariable> Variables => _variables;

    /// <summary>
    /// Indica se o modelo foi validado.
    /// </summary>
    public bool IsValidated { get; private set; }

    /// <summary>
    /// Tempo morto dominante do processo (segundos).
    /// Usado como referência para sintonia inicial de controladores.
    /// </summary>
    public TimeSpan DeadTime { get; private set; }

    /// <summary>
    /// Constante de tempo dominante do processo (segundos).
    /// </summary>
    public TimeSpan TimeConstant { get; private set; }

    /// <summary>
    /// Cria um novo modelo de engenharia.
    /// </summary>
    public static Result<ControlModel> Create(string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ControlModel>.Failure(
                "O nome do modelo é obrigatório.");
        }

        var model = new ControlModel();

        model.Rename(name);
        model.SetDescription(description);

        return Result<ControlModel>.Success(model);
    }

    /// <summary>
    /// Adiciona uma variável ao modelo.
    /// </summary>
    public Result<ModelVariable> AddVariable(
        string name,
        ModelVariableType type,
        string unit = "",
        double minimum = 0,
        double maximum = 0,
        double nominal = 0)
    {
        if (_variables.Any(v => string.Equals(
            v.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ModelVariable>.Failure(
                $"Já existe uma variável chamada '{name}' no modelo.");
        }

        var result = ModelVariable.Create(name, type, unit, minimum, maximum, nominal);

        if (!result.IsSuccess || result.Value is null)
        {
            return result;
        }

        _variables.Add(result.Value);

        MarkAsModified();

        return result;
    }

    /// <summary>
    /// Remove uma variável do modelo pelo id.
    /// </summary>
    public bool RemoveVariable(Guid variableId)
    {
        var variable = _variables.FirstOrDefault(v => v.Id == variableId);

        if (variable is null)
        {
            return false;
        }

        _variables.Remove(variable);

        MarkAsModified();

        return true;
    }

    /// <summary>
    /// Define os parâmetros dinâmicos dominantes do processo.
    /// </summary>
    public void SetDynamics(TimeSpan timeConstant, TimeSpan deadTime)
    {
        if (timeConstant <= TimeSpan.Zero)
        {
            throw new ArgumentException(
                "A constante de tempo deve ser positiva.",
                nameof(timeConstant));
        }

        if (deadTime < TimeSpan.Zero)
        {
            throw new ArgumentException(
                "O tempo morto não pode ser negativo.",
                nameof(deadTime));
        }

        TimeConstant = timeConstant;
        DeadTime = deadTime;

        Touch();
    }

    /// <summary>
    /// Marca o modelo como validado.
    /// </summary>
    public void MarkValidated()
    {
        IsValidated = true;

        Touch();
    }

    /// <summary>
    /// Marca o modelo como alterado.
    /// </summary>
    public void MarkAsModified()
    {
        Touch();
    }

    /// <summary>
    /// Enumera as variáveis de um determinado tipo.
    /// </summary>
    public IEnumerable<ModelVariable> VariablesOfType(ModelVariableType type)
        => _variables.Where(v => v.Type == type);
}
