using ALE.ACS.Domain.Common.Base;
using ALE.ACS.Domain.Common.Results;

namespace ALE.ACS.Domain.Projects;

/// <summary>
/// Representa um projeto de engenharia do ALE Advanced Control Studio.
/// </summary>
public sealed class Project : AggregateRoot
{
    private Project()
    {
    }

    /// <summary>
    /// Cria um novo projeto.
    /// </summary>
    /// <param name="name">Nome do projeto.</param>
    /// <returns>Resultado da criação.</returns>
    public static Result<Project> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Project>.Failure(
                "O nome do projeto é obrigatório.");
        }

        var project = new Project();

        project.Rename(name);

        return Result<Project>.Success(project);
    }
}