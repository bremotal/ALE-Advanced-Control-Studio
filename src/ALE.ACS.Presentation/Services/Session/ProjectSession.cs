using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Presentation.Services.Session;

/// <summary>
/// Mantém o estado da sessão da IDE.
/// </summary>
public sealed class ProjectSession : IProjectSession
{
    private Project? _currentProject;

    public Project? CurrentProject => _currentProject;

    public event EventHandler<Project?>? CurrentProjectChanged;

    public void SetCurrentProject(Project project)
    {
        if (_currentProject == project)
            return;

        _currentProject = project;
        CurrentProjectChanged?.Invoke(this, _currentProject);
    }

    public void CloseProject()
    {
        if (_currentProject is null)
            return;

        _currentProject = null;
        CurrentProjectChanged?.Invoke(this, null);
    }
}