using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Interfaces;

/// <summary>
/// Serviço responsável pelas operações relacionadas a projetos.
/// </summary>
public interface IProjectService
{
    Result<Project> Create(string projectName);
}