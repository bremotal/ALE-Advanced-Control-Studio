using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Models;

namespace ALE.ACS.Application.Interfaces;

/// <summary>
/// Serviço responsável pelas operações de modelos de processo dentro do projeto ativo.
/// </summary>
public interface IModelService
{
    /// <summary>Lista os modelos do projeto ativo.</summary>
    IReadOnlyList<ControlModel> GetModels();

    /// <summary>Obtém um modelo pelo id.</summary>
    ControlModel? GetModel(Guid id);

    /// <summary>Obtém um modelo pelo nome (case-insensitive).</summary>
    ControlModel? GetModelByName(string name);

    /// <summary>Cria e adiciona um modelo ao projeto ativo.</summary>
    Result<ControlModel> Create(string name, string description = "");

    /// <summary>Renomeia um modelo.</summary>
    Result Rename(Guid id, string newName);

    /// <summary>Adiciona uma variável a um modelo.</summary>
    Result<ModelVariable> AddVariable(
        Guid modelId,
        string name,
        ModelVariableType type,
        string unit = "",
        double minimum = 0,
        double maximum = 0,
        double nominal = 0);

    /// <summary>Remove uma variável de um modelo.</summary>
    Result RemoveVariable(Guid modelId, Guid variableId);

    /// <summary>Define a dinâmica dominante do processo.</summary>
    Result SetDynamics(Guid modelId, TimeSpan timeConstant, TimeSpan deadTime);

    /// <summary>Marca o modelo como validado.</summary>
    Result MarkValidated(Guid modelId);

    /// <summary>Remove um modelo do projeto ativo.</summary>
    Result Remove(Guid id);
}
