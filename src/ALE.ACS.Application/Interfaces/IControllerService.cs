using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Controllers;

namespace ALE.ACS.Application.Interfaces;

/// <summary>
/// Serviço responsável pelas operações de controladores dentro do projeto ativo.
/// </summary>
public interface IControllerService
{
    /// <summary>Lista os controladores do projeto ativo.</summary>
    IReadOnlyList<Controller> GetControllers();

    /// <summary>Obtém um controlador pelo id.</summary>
    Controller? GetController(Guid id);

    /// <summary>Obtém um controlador pelo nome (case-insensitive).</summary>
    Controller? GetControllerByName(string name);

    /// <summary>Cria e adiciona um controlador ao projeto ativo.</summary>
    Result<Controller> Create(string name, ControllerType type);

    /// <summary>Renomeia um controlador.</summary>
    Result Rename(Guid id, string newName);

    /// <summary>Define as variáveis (SP/MV/CV) de um controlador.</summary>
    Result SetVariables(Guid id, string setpoint, string manipulated, string controlled);

    /// <summary>Define o período de varredura.</summary>
    Result SetScanTime(Guid id, TimeSpan scanTime);

    /// <summary>Armazena a especificação serializada do controlador.</summary>
    Result SetSerializedSpec(Guid id, string serializedSpec);

    /// <summary>Marca o design como válido.</summary>
    Result MarkDesignValid(Guid id, string designSummary);

    /// <summary>Remove um controlador do projeto ativo.</summary>
    Result Remove(Guid id);
}
