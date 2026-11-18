using ALE.ACS.Application.EventBus;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Application.Projects.CreateProject;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Controllers;
using ALE.ACS.Domain.Models;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Projects;

/// <summary>
/// Implementação única dos serviços relacionados a projetos.
/// </summary>
/// <remarks>
/// Centraliza o ciclo de vida do projeto ativo, delegando a criação ao
/// <see cref="CreateProjectHandler"/> e mantendo o <see cref="CurrentProject"/>
/// em memória. As operações <c>Open</c>/<c>Save</c> delegam para o
/// <see cref="IProjectRepository"/> injetado (implementação concreta chega
/// via Infrastructure).
/// </remarks>
public sealed class ProjectService : IProjectService
{
    private readonly CreateProjectHandler _createProjectHandler;
    private readonly IEventBus _eventBus;
    private readonly IProjectRepository _repository;

    private Project? _currentProject;
    private readonly List<string> _recentProjects = [];

    public ProjectService(IEventBus eventBus, IProjectRepository repository)
    {
        _createProjectHandler = new CreateProjectHandler();
        _eventBus = eventBus;
        _repository = repository;
    }

    /// <inheritdoc/>
    public Project? CurrentProject => _currentProject;

    /// <inheritdoc/>
    public Result<Project> Create(string projectName)
    {
        var command = new CreateProjectCommand(projectName);
        var result = _createProjectHandler.Handle(command);

        if (!result.IsSuccess || result.Value is null)
        {
            return result;
        }

        _currentProject = result.Value;

        _eventBus.Publish(new ProjectCreatedEvent(result.Value.Name));

        return result;
    }

    /// <inheritdoc/>
    public Result<Project> Open(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result<Project>.Failure(
                "O caminho do arquivo é obrigatório.");
        }

        if (!Path.GetExtension(fileName).Equals(
            ".aleacs", StringComparison.OrdinalIgnoreCase))
        {
            return Result<Project>.Failure(
                "O arquivo deve ter extensão .aleacs.");
        }

        var loadResult = _repository.Load(fileName);

        if (!loadResult.IsSuccess || loadResult.Value is null)
        {
            return loadResult;
        }

        _currentProject = loadResult.Value;

        TrackRecent(fileName);

        _eventBus.Publish(new ProjectCreatedEvent(_currentProject.Name));

        return loadResult;
    }

    /// <inheritdoc/>
    public Result Save()
    {
        if (_currentProject is null)
        {
            return Result.Failure(
                "Nenhum projeto ativo para salvar.");
        }

        if (string.IsNullOrWhiteSpace(_currentProject.FileName))
        {
            return Result.Failure(
                "O projeto não tem caminho de arquivo. Use SaveAs.");
        }

        var result = _repository.Save(_currentProject);

        if (result.IsSuccess)
        {
            _currentProject.MarkAsSaved();
        }

        return result;
    }

    /// <inheritdoc/>
    public Result SaveAs(string fileName)
    {
        if (_currentProject is null)
        {
            return Result.Failure(
                "Nenhum projeto ativo para salvar.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        _currentProject.SetFileName(fileName);

        var result = _repository.Save(_currentProject);

        if (result.IsSuccess)
        {
            _currentProject.MarkAsSaved();
            TrackRecent(fileName);
        }

        return result;
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (_currentProject is null)
        {
            return;
        }

        _currentProject = null;

        _eventBus.Publish(new ProjectClosedEvent());
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetRecentProjects()
        => _recentProjects.AsReadOnly();

    /// <inheritdoc/>
    public Result<ControlModel> AddModel(string name, string description = "")
    {
        if (_currentProject is null)
        {
            return Result<ControlModel>.Failure(
                "Nenhum projeto ativo.");
        }

        var result = _currentProject.AddModel(name, description);

        if (result.IsSuccess && result.Value is not null)
        {
            _eventBus.Publish(new ModelCreatedEvent(result.Value.Name));
        }

        return result;
    }

    /// <inheritdoc/>
    public Result<Controller> AddController(string name, ControllerType type)
    {
        if (_currentProject is null)
        {
            return Result<Controller>.Failure(
                "Nenhum projeto ativo.");
        }

        var result = _currentProject.AddController(name, type);

        if (result.IsSuccess && result.Value is not null)
        {
            _eventBus.Publish(new ControllerCreatedEvent(result.Value.Name));
        }

        return result;
    }

    private void TrackRecent(string fileName)
    {
        _recentProjects.Remove(fileName);
        _recentProjects.Insert(0, fileName);

        if (_recentProjects.Count > 10)
        {
            _recentProjects.RemoveAt(_recentProjects.Count - 1);
        }
    }
}
