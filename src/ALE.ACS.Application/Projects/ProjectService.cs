using ALE.ACS.Application.Interfaces;
using ALE.ACS.Application.Projects.CreateProject;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Projects;

/// <summary>
/// Implementação dos serviços relacionados a projetos.
/// </summary>
public sealed class ProjectService : IProjectService
{
    private readonly CreateProjectHandler _createProjectHandler;

    public ProjectService()
    {
        _createProjectHandler = new CreateProjectHandler();
    }

    public Result<Project> Create(string projectName)
    {
        var command = new CreateProjectCommand(projectName);

        return _createProjectHandler.Handle(command);
    }
}