using System.Collections.ObjectModel;
using ALE.ACS.Domain.Common.Base;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Controllers;
using ALE.ACS.Domain.Models;

namespace ALE.ACS.Domain.Projects;

/// <summary>
/// Representa um projeto de engenharia do ALE Advanced Control Studio.
/// </summary>
/// <remarks>
/// Aggregate root principal. Carrega os filhos:
/// <list type="bullet">
///   <item><description><see cref="Models"/>: modelos de processo.</description></item>
///   <item><description><see cref="Controllers"/>: controladores de processo.</description></item>
/// </list>
/// </remarks>
public sealed class Project : AggregateRoot
{
    private readonly ObservableCollection<ControlModel> _models = [];
    private readonly ObservableCollection<Controller> _controllers = [];

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
    /// Modelos de processo do projeto.
    /// </summary>
    public IReadOnlyCollection<ControlModel> Models => _models;

    /// <summary>
    /// Controladores do projeto.
    /// </summary>
    public IReadOnlyCollection<Controller> Controllers => _controllers;

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

    // ===== Modelos =====

    /// <summary>
    /// Adiciona um modelo de processo ao projeto.
    /// </summary>
    public Result<ControlModel> AddModel(string name, string description = "")
    {
        if (_models.Any(m => string.Equals(
            m.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ControlModel>.Failure(
                $"Já existe um modelo chamado '{name}' no projeto.");
        }

        var result = ControlModel.Create(name, description);

        if (!result.IsSuccess || result.Value is null)
        {
            return result;
        }

        _models.Add(result.Value);
        MarkAsModified();

        return result;
    }

    /// <summary>
    /// Remove um modelo do projeto pelo id.
    /// </summary>
    public bool RemoveModel(Guid modelId)
    {
        var model = _models.FirstOrDefault(m => m.Id == modelId);

        if (model is null)
        {
            return false;
        }

        _models.Remove(model);
        MarkAsModified();

        return true;
    }

    /// <summary>
    /// Localiza um modelo pelo nome (case-insensitive).
    /// </summary>
    public ControlModel? FindModel(string name)
        => _models.FirstOrDefault(m => string.Equals(
            m.Name, name, StringComparison.OrdinalIgnoreCase));

    // ===== Controladores =====

    /// <summary>
    /// Adiciona um controlador ao projeto.
    /// </summary>
    public Result<Controller> AddController(string name, ControllerType type)
    {
        if (_controllers.Any(c => string.Equals(
            c.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Controller>.Failure(
                $"Já existe um controlador chamado '{name}' no projeto.");
        }

        var result = Controller.Create(name, type);

        if (!result.IsSuccess || result.Value is null)
        {
            return result;
        }

        _controllers.Add(result.Value);
        MarkAsModified();

        return result;
    }

    /// <summary>
    /// Remove um controlador do projeto pelo id.
    /// </summary>
    public bool RemoveController(Guid controllerId)
    {
        var controller = _controllers.FirstOrDefault(c => c.Id == controllerId);

        if (controller is null)
        {
            return false;
        }

        _controllers.Remove(controller);
        MarkAsModified();

        return true;
    }

    /// <summary>
    /// Localiza um controlador pelo nome (case-insensitive).
    /// </summary>
    public Controller? FindController(string name)
        => _controllers.FirstOrDefault(c => string.Equals(
            c.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Enumera os controladores de um determinado tipo.
    /// </summary>
    public IEnumerable<Controller> ControllersOfType(ControllerType type)
        => _controllers.Where(c => c.Type == type);
}
