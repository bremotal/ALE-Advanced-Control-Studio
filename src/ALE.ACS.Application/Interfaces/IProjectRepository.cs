using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Interfaces;

/// <summary>
/// Repositório de projetos — abstração sobre a camada de persistência.
/// A Application programa contra esta interface; a Infrastructure fornece
/// a implementação concreta (arquivo JSON, banco, etc.).
/// </summary>
public interface IProjectRepository
{
    /// <summary>Salva um projeto no caminho armazenado em <see cref="Project.FileName"/>.</summary>
    Result Save(Project project);

    /// <summary>Carrega um projeto a partir de um caminho de arquivo.</summary>
    Result<Project> Load(string fileName);
}
