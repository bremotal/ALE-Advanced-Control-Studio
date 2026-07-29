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
    /// Caminho do arquivo do projeto.
    /// </summary>
    public string? FileName { get; private set; }

    /// <summary>
    /// Indica se existem alterações pendentes de gravação.
    /// </summary>
    public bool IsDirty { get; private set; }

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

        var project = new Project
        {
            IsDirty = true
        };

        project.Rename(name);

        return Result<Project>.Success(project);
    }

    /// <summary>
    /// Define o caminho do arquivo do projeto.
    /// </summary>
    /// <param name="fileName">Caminho do arquivo.</param>
    public void SetFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        FileName = fileName;
    }

    /// <summary>
    /// Marca o projeto como salvo.
    /// </summary>
    public void MarkAsSaved()
    {
        IsDirty = false;
    }

    /// <summary>
    /// Marca o projeto como alterado.
    /// </summary>
    public void MarkAsModified()
    {
        IsDirty = true;
    }
}