using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Projects;

/// <summary>
/// Implementação em memória do gerenciamento de projetos.
/// </summary>
public sealed class ProjectService : IProjectService
{
    public EngineeringProject? CurrentProject { get; private set; }

    public bool HasProject => CurrentProject is not null;

    public EngineeringProject NewProject()
    {
        var now = DateTime.Now;

        CurrentProject = new EngineeringProject(
            Guid.NewGuid().ToString("N"),
            "Untitled Project",
            string.Empty,
            now,
            now,
            true);

        return CurrentProject;
    }

    public void CloseProject()
    {
        CurrentProject = null;
    }

    public void SaveProject()
    {
        if (CurrentProject is null)
            return;

        CurrentProject.MarkSaved();
    }
}