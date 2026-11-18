using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Controllers;
using ALE.ACS.Domain.Models;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Interfaces;

/// <summary>
/// Serviço responsável pelas operações relacionadas a projetos.
/// </summary>
/// <remarks>
/// Centraliza o ciclo de vida do projeto ativo e suas operações de filhos
/// (modelos e controladores). Operações mais específicas de controllers
/// e models ficam em <see cref="IControllerService"/> e <see cref="IModelService"/>.
/// </remarks>
public interface IProjectService
{
    /// <summary>Projeto atualmente ativo (null se nenhum).</summary>
    Project? CurrentProject { get; }

    /// <summary>Cria um novo projeto e o define como ativo.</summary>
    Result<Project> Create(string projectName);

    /// <summary>Abre um projeto a partir do caminho do arquivo.</summary>
    Result<Project> Open(string fileName);

    /// <summary>Salva o projeto ativo no caminho atual.</summary>
    Result Save();

    /// <summary>Salva o projeto ativo num novo caminho.</summary>
    Result SaveAs(string fileName);

    /// <summary>Fecha o projeto ativo.</summary>
    void Close();

    /// <summary>Lista os projetos recentes (caminhos).</summary>
    IReadOnlyList<string> GetRecentProjects();

    /// <summary>Adiciona um modelo ao projeto ativo.</summary>
    Result<ControlModel> AddModel(string name, string description = "");

    /// <summary>Adiciona um controlador ao projeto ativo.</summary>
    Result<Controller> AddController(string name, ControllerType type);
}
