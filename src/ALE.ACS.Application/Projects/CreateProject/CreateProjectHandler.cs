using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Projects.CreateProject;

/// <summary>
/// Manipula a solicitação de criação de um novo projeto.
/// </summary>
public sealed class CreateProjectHandler
{
    /// <summary>
    /// Executa o caso de uso.
    /// </summary>
    /// <param name="command">Comando recebido da interface.</param>
    /// <returns>Resultado da criação do projeto.</returns>
    public Result<Project> Handle(CreateProjectCommand command)
    {
        return Project.Create(command.Name);
    }
}