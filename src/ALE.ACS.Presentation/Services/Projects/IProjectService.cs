using ALE.ACS.Presentation.Models;

namespace ALE.ACS.Presentation.Services.Projects;

/// <summary>
/// Serviço de gerenciamento de projetos em memória.
/// </summary>
public interface IProjectService
{
    EngineeringProject? CurrentProject { get; }

    bool HasProject { get; }

    EngineeringProject NewProject();

    void CloseProject();

    void SaveProject();
}