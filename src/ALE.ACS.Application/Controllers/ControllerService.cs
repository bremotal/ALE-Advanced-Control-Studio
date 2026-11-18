using ALE.ACS.Application.EventBus;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Controllers;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Controllers;

/// <summary>
/// Implementação de <see cref="IControllerService"/> operando sobre o
/// projeto ativo mantido por <see cref="IProjectService"/>.
/// </summary>
public sealed class ControllerService : IControllerService
{
    private readonly IProjectService _projectService;
    private readonly IEventBus _eventBus;

    public ControllerService(IProjectService projectService, IEventBus eventBus)
    {
        _projectService = projectService;
        _eventBus = eventBus;
    }

    /// <inheritdoc/>
    public IReadOnlyList<Controller> GetControllers()
        => _projectService.CurrentProject?.Controllers.ToList()
           ?? [];

    /// <inheritdoc/>
    public Controller? GetController(Guid id)
        => _projectService.CurrentProject?.Controllers
            .FirstOrDefault(c => c.Id == id);

    /// <inheritdoc/>
    public Controller? GetControllerByName(string name)
        => _projectService.CurrentProject?.FindController(name);

    /// <inheritdoc/>
    public Result<Controller> Create(string name, ControllerType type)
    {
        if (_projectService.CurrentProject is null)
        {
            return Result<Controller>.Failure(
                "Nenhum projeto ativo.");
        }

        return _projectService.AddController(name, type);
    }

    /// <inheritdoc/>
    public Result Rename(Guid id, string newName)
    {
        var controller = GetController(id);

        if (controller is null)
        {
            return Result.Failure(
                "Controlador não encontrado.");
        }

        var previous = controller.Name;

        try
        {
            controller.Rename(newName);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        _eventBus.Publish(new ControllerRenamedEvent(previous, controller.Name));

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result SetVariables(Guid id, string setpoint, string manipulated, string controlled)
    {
        var controller = GetController(id);

        if (controller is null)
        {
            return Result.Failure(
                "Controlador não encontrado.");
        }

        try
        {
            controller.SetVariables(setpoint, manipulated, controlled);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result SetScanTime(Guid id, TimeSpan scanTime)
    {
        var controller = GetController(id);

        if (controller is null)
        {
            return Result.Failure(
                "Controlador não encontrado.");
        }

        try
        {
            controller.SetScanTime(scanTime);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result SetSerializedSpec(Guid id, string serializedSpec)
    {
        var controller = GetController(id);

        if (controller is null)
        {
            return Result.Failure(
                "Controlador não encontrado.");
        }

        controller.SetSerializedSpec(serializedSpec);

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result MarkDesignValid(Guid id, string designSummary)
    {
        var controller = GetController(id);

        if (controller is null)
        {
            return Result.Failure(
                "Controlador não encontrado.");
        }

        controller.MarkDesignValid(designSummary);

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result Remove(Guid id)
    {
        if (_projectService.CurrentProject is null)
        {
            return Result.Failure(
                "Nenhum projeto ativo.");
        }

        var removed = _projectService.CurrentProject.RemoveController(id);

        return removed
            ? Result.Success()
            : Result.Failure("Controlador não encontrado.");
    }
}
