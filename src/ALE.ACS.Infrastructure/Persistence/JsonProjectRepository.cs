using System.Text.Json;
using System.Text.Json.Serialization;
using ALE.ACS.Application.Interfaces;
using ALE.ACS.Domain.Common.Results;
using ALE.ACS.Domain.Controllers;
using ALE.ACS.Domain.Models;
using ALE.ACS.Domain.Projects;
using ALE.ACS.Engine.Core.Projects;
using ALE.ACS.Infrastructure.FileSystem;

namespace ALE.ACS.Infrastructure.Persistence;

/// <summary>
/// Repositório de projetos que serializa o aggregate <see cref="Project"/>
/// (com seus filhos Models e Controllers) para JSON via <see cref="IPersistenceProvider"/>.
/// Implementa <see cref="IProjectRepository"/> definida na camada Application.
/// </summary>
/// <remarks>
/// Faz a ponte entre o aggregate do Domain (com comportamento) e o snapshot
/// do Core/Projects (que carrega apenas payload opaco). O payload é a serialização
/// JSON do <see cref="ProjectDto"/>.
/// </remarks>
public sealed class JsonProjectRepository : IProjectRepository
{
    private readonly IPersistenceProvider _persistenceProvider;
    private readonly IFileSystem _fileSystem;
    private const string ProviderName = "file";

    public JsonProjectRepository(IPersistenceProvider persistenceProvider, IFileSystem fileSystem)
    {
        _persistenceProvider = persistenceProvider ?? throw new ArgumentNullException(nameof(persistenceProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>Salva um projeto num arquivo .aleacs.</summary>
    public Result Save(Project project)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (string.IsNullOrWhiteSpace(project.FileName))
        {
            return Result.Failure(
                "O projeto não tem caminho de arquivo. Use SetFileName/SaveAs primeiro.");
        }

        try
        {
            var dto = ProjectDto.FromDomain(project);
            var payload = JsonSerializer.Serialize(dto, ProjectDtoJsonContext.Default.ProjectDto);

            var snapshot = new ProjectSnapshot(
                id: project.Id.ToString(),
                name: project.Name,
                version: "1.0",
                createdAt: project.CreatedAt,
                updatedAt: project.UpdatedAt,
                payload: payload,
                metadata: new Dictionary<string, string>
                {
                    ["format"] = "json",
                    ["fileName"] = project.FileName
                });

            var saveResult = _persistenceProvider.Save(snapshot, new SaveOptions(ProviderName));

            if (!saveResult.Success)
            {
                return Result.Failure(saveResult.Message ?? "Falha ao salvar projeto.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Falha ao serializar projeto: {ex.Message}");
        }
    }

    /// <summary>Carrega um projeto a partir do caminho do arquivo.</summary>
    public Result<Project> Load(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            if (!_fileSystem.FileExists(fileName))
            {
                return Result<Project>.Failure(
                    $"Arquivo não encontrado: {fileName}");
            }

            var id = Path.GetFileNameWithoutExtension(fileName);
            var loadResult = _persistenceProvider.Load(id, new LoadOptions(ProviderName));

            if (!loadResult.Success || loadResult.Snapshot is null)
            {
                return Result<Project>.Failure(
                    loadResult.Message ?? "Falha ao carregar projeto.");
            }

            var dto = JsonSerializer.Deserialize(
                loadResult.Snapshot.Payload,
                ProjectDtoJsonContext.Default.ProjectDto);

            if (dto is null)
            {
                return Result<Project>.Failure("Falha ao deserializar projeto.");
            }

            var project = dto.ToDomain();
            project.SetFileName(fileName);

            return Result<Project>.Success(project);
        }
        catch (Exception ex)
        {
            return Result<Project>.Failure($"Falha ao carregar projeto: {ex.Message}");
        }
    }
}

// ===== DTOs de serialização =====

internal sealed class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ControlModelDto> Models { get; set; } = [];
    public List<ControllerDto> Controllers { get; set; } = [];

    public static ProjectDto FromDomain(Project project)
    {
        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            DisplayName = project.DisplayName,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };

        foreach (var model in project.Models)
        {
            dto.Models.Add(ControlModelDto.FromDomain(model));
        }

        foreach (var controller in project.Controllers)
        {
            dto.Controllers.Add(ControllerDto.FromDomain(controller));
        }

        return dto;
    }

    public Project ToDomain()
    {
        var createResult = Project.Create(Name);
        if (!createResult.IsSuccess || createResult.Value is null)
        {
            // Não deveria acontecer (validamos em FromDomain), mas como fallback:
            createResult = Project.Create($"Project_{Id:N}");
        }

        var project = createResult.Value!;

        if (!string.IsNullOrEmpty(DisplayName))
        {
            project.SetDisplayName(DisplayName);
        }

        project.SetDescription(Description);

        foreach (var modelDto in Models)
        {
            var model = modelDto.ToDomain();
            // Re-id para casar: precisamos recriar via reflexão ou aceitar novo id.
            // Para manter simples, adicionamos via AddModel e copiamos variáveis.
            var addResult = project.AddModel(model.Name, model.Description);
            if (addResult.IsSuccess && addResult.Value is not null)
            {
                var target = addResult.Value;
                foreach (var v in model.Variables)
                {
                    target.AddVariable(
                        v.Name,
                        v.Type,
                        v.Unit,
                        v.Minimum,
                        v.Maximum,
                        v.Nominal);
                }
                if (model.TimeConstant > TimeSpan.Zero)
                {
                    target.SetDynamics(model.TimeConstant, model.DeadTime);
                }
                if (model.IsValidated)
                {
                    target.MarkValidated();
                }
            }
        }

        foreach (var controllerDto in Controllers)
        {
            var controller = controllerDto.ToDomain();
            var addResult = project.AddController(controller.Name, controller.Type);
            if (addResult.IsSuccess && addResult.Value is not null)
            {
                var target = addResult.Value;
                if (!string.IsNullOrEmpty(controller.Setpoint))
                {
                    target.SetVariables(
                        controller.Setpoint,
                        controller.Manipulated,
                        controller.Controlled);
                }
                target.SetScanTime(controller.ScanTime);
                if (!string.IsNullOrEmpty(controller.SerializedSpec))
                {
                    target.SetSerializedSpec(controller.SerializedSpec);
                }
                if (controller.IsDesignValid)
                {
                    target.MarkDesignValid(controller.DesignSummary);
                }
            }
        }

        return project;
    }
}

internal sealed class ControlModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ModelVariableDto> Variables { get; set; } = [];
    public bool IsValidated { get; set; }
    public double TimeConstantSeconds { get; set; }
    public double DeadTimeSeconds { get; set; }

    public static ControlModelDto FromDomain(ControlModel model)
    {
        var dto = new ControlModelDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsValidated = model.IsValidated,
            TimeConstantSeconds = model.TimeConstant.TotalSeconds,
            DeadTimeSeconds = model.DeadTime.TotalSeconds
        };

        foreach (var v in model.Variables)
        {
            dto.Variables.Add(ModelVariableDto.FromDomain(v));
        }

        return dto;
    }

    public ControlModel ToDomain()
    {
        var createResult = ControlModel.Create(Name, Description);
        if (!createResult.IsSuccess || createResult.Value is null)
        {
            createResult = ControlModel.Create($"Model_{Id:N}");
        }

        var model = createResult.Value!;

        foreach (var v in Variables)
        {
            model.AddVariable(v.Name, v.Type, v.Unit, v.Minimum, v.Maximum, v.Nominal);
        }

        if (TimeConstantSeconds > 0)
        {
            model.SetDynamics(
                TimeSpan.FromSeconds(TimeConstantSeconds),
                TimeSpan.FromSeconds(DeadTimeSeconds));
        }

        if (IsValidated)
        {
            model.MarkValidated();
        }

        return model;
    }
}

internal sealed class ModelVariableDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ModelVariableType Type { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double Nominal { get; set; }

    public static ModelVariableDto FromDomain(ModelVariable v)
        => new()
        {
            Id = v.Id,
            Name = v.Name,
            Description = v.Description,
            Type = v.Type,
            Unit = v.Unit,
            Minimum = v.Minimum,
            Maximum = v.Maximum,
            Nominal = v.Nominal
        };
}

internal sealed class ControllerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ControllerType Type { get; set; }
    public ControllerLifecycle Lifecycle { get; set; }
    public string Setpoint { get; set; } = string.Empty;
    public string Manipulated { get; set; } = string.Empty;
    public string Controlled { get; set; } = string.Empty;
    public double ScanTimeSeconds { get; set; } = 0.1;
    public string SerializedSpec { get; set; } = string.Empty;
    public bool IsDesignValid { get; set; }
    public string DesignSummary { get; set; } = string.Empty;

    public static ControllerDto FromDomain(Controller c)
        => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Type = c.Type,
            Lifecycle = c.Lifecycle,
            Setpoint = c.Setpoint,
            Manipulated = c.Manipulated,
            Controlled = c.Controlled,
            ScanTimeSeconds = c.ScanTime.TotalSeconds,
            SerializedSpec = c.SerializedSpec,
            IsDesignValid = c.IsDesignValid,
            DesignSummary = c.DesignSummary
        };

    public Controller ToDomain()
    {
        var createResult = Controller.Create(Name, Type);
        if (!createResult.IsSuccess || createResult.Value is null)
        {
            createResult = Controller.Create($"Controller_{Id:N}", Type);
        }

        var controller = createResult.Value!;

        if (!string.IsNullOrEmpty(Setpoint))
        {
            controller.SetVariables(Setpoint, Manipulated, Controlled);
        }

        if (ScanTimeSeconds > 0)
        {
            controller.SetScanTime(TimeSpan.FromSeconds(ScanTimeSeconds));
        }

        if (!string.IsNullOrEmpty(SerializedSpec))
        {
            controller.SetSerializedSpec(SerializedSpec);
        }

        if (IsDesignValid)
        {
            controller.MarkDesignValid(DesignSummary);
        }

        return controller;
    }
}

[JsonSerializable(typeof(ProjectDto))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ProjectDtoJsonContext : JsonSerializerContext
{
}
