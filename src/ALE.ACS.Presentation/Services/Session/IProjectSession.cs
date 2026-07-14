using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Presentation.Services.Session;

/// <summary>
/// Mantém o estado do projeto atualmente aberto na IDE.
/// </summary>
public interface IProjectSession
{
    /// <summary>
    /// Projeto atualmente aberto.
    /// </summary>
    Project? CurrentProject { get; }

    /// <summary>
    /// Define o projeto ativo.
    /// </summary>
    void SetCurrentProject(Project project);

    /// <summary>
    /// Fecha o projeto atual.
    /// </summary>
    void CloseProject();
}