using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Presentation.Services.Session;

/// <summary>
/// Mantém o estado da sessão da IDE.
/// </summary>
public sealed class ProjectSession : IProjectSession
{
    public Project? CurrentProject { get; private set; }

    public void SetCurrentProject(Project project)
    {
        CurrentProject = project;
    }

    public void CloseProject()
    {
        CurrentProject = null;
    }
}