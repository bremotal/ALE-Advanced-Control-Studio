using ALE.ACS.Application.Projects.CreateProject;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Projects;

/// <summary>
/// Coordinates IDE project lifecycle operations.
/// </summary>
public sealed class ProjectManager
{
    private readonly CreateProjectHandler _createProjectHandler = new();

    /// <summary>
    /// Creates a project using the application create-project use case.
    /// </summary>
    /// <param name="name">Project name.</param>
    /// <returns>Creation result.</returns>
    public Result<Project> CreateProject(string name)
    {
        var command = new CreateProjectCommand(name);
        return _createProjectHandler.Handle(command);
    }
}
