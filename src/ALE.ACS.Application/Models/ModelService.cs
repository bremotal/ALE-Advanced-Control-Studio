using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Models;
using ALE.ACS.Domain.Projects;

namespace ALE.ACS.Application.Models;

/// <summary>
/// Implementação de <see cref="IModelService"/> operando sobre o
/// projeto ativo mantido por <see cref="IProjectService"/>.
/// </summary>
public sealed class ModelService : IModelService
{
    private readonly IProjectService _projectService;

    public ModelService(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ControlModel> GetModels()
        => _projectService.CurrentProject?.Models.ToList() ?? [];

    /// <inheritdoc/>
    public ControlModel? GetModel(Guid id)
        => _projectService.CurrentProject?.Models
            .FirstOrDefault(m => m.Id == id);

    /// <inheritdoc/>
    public ControlModel? GetModelByName(string name)
        => _projectService.CurrentProject?.FindModel(name);

    /// <inheritdoc/>
    public Result<ControlModel> Create(string name, string description = "")
    {
        if (_projectService.CurrentProject is null)
        {
            return Result<ControlModel>.Failure(
                "Nenhum projeto ativo.");
        }

        return _projectService.AddModel(name, description);
    }

    /// <inheritdoc/>
    public Result Rename(Guid id, string newName)
    {
        var model = GetModel(id);

        if (model is null)
        {
            return Result.Failure("Modelo não encontrado.");
        }

        try
        {
            model.Rename(newName);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result<ModelVariable> AddVariable(
        Guid modelId,
        string name,
        ModelVariableType type,
        string unit = "",
        double minimum = 0,
        double maximum = 0,
        double nominal = 0)
    {
        var model = GetModel(modelId);

        if (model is null)
        {
            return Result<ModelVariable>.Failure("Modelo não encontrado.");
        }

        return model.AddVariable(name, type, unit, minimum, maximum, nominal);
    }

    /// <inheritdoc/>
    public Result RemoveVariable(Guid modelId, Guid variableId)
    {
        var model = GetModel(modelId);

        if (model is null)
        {
            return Result.Failure("Modelo não encontrado.");
        }

        var removed = model.RemoveVariable(variableId);

        return removed
            ? Result.Success()
            : Result.Failure("Variável não encontrada no modelo.");
    }

    /// <inheritdoc/>
    public Result SetDynamics(Guid modelId, TimeSpan timeConstant, TimeSpan deadTime)
    {
        var model = GetModel(modelId);

        if (model is null)
        {
            return Result.Failure("Modelo não encontrado.");
        }

        try
        {
            model.SetDynamics(timeConstant, deadTime);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result MarkValidated(Guid modelId)
    {
        var model = GetModel(modelId);

        if (model is null)
        {
            return Result.Failure("Modelo não encontrado.");
        }

        model.MarkValidated();

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result Remove(Guid id)
    {
        if (_projectService.CurrentProject is null)
        {
            return Result.Failure("Nenhum projeto ativo.");
        }

        var removed = _projectService.CurrentProject.RemoveModel(id);

        return removed
            ? Result.Success()
            : Result.Failure("Modelo não encontrado.");
    }
}
