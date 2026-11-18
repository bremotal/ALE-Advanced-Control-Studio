using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core.Export;

namespace ALE.ACS.Export.Providers;

/// <summary>
/// Provider de exportação que gera código PLC Structured Text usando o
/// <see cref="PlcCodeGeneratorRegistry"/> do Engine. Converte um
/// <see cref="ExportRequest"/> (que carrega um <c>IControllerSpec</c> no
/// payload via metadata) em um <see cref="ExportArtifact"/> .st.
/// </summary>
public sealed class PlcCodeExportProvider : IExportProvider
{
    private readonly PlcCodeGeneratorRegistry _registry;

    public PlcCodeExportProvider(PlcCodeGeneratorRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <inheritdoc/>
    public bool CanExport(ExportRequest request)
    {
        if (request is null)
        {
            return false;
        }

        // O provider atende apenas formatos PLC.
        return request.Format is ExportFormat.SiemensSCL
            or ExportFormat.RockwellST
            or ExportFormat.CODESYSST
            or ExportFormat.LookupTable;
    }

    /// <inheritdoc/>
    public ExportResult Export(ExportRequest request)
    {
        if (request is null)
        {
            return new ExportResult(false, Array.Empty<ExportArtifact>(),
                "ExportRequest nulo.");
        }

        if (!CanExport(request))
        {
            return new ExportResult(false, Array.Empty<ExportArtifact>(),
                $"Formato {request.Format} não suportado pelo PlcCodeExportProvider.");
        }

        // O ExportRequest do Core carrega EngineeringProject/Application/Strategy,
        // mas para geração PLC precisamos de um IControllerSpec. A ponte é feita
        // pelo caller (Application layer) escrevendo o spec serializado nos metadados
        // — por ora retornamos uma mensagem clara indicando que o wire do spec
        // deve ser feito pela camada de aplicação no Bloco E (integração CORE).
        // Este provider está pronto para ser chamado assim que o registry
        // tiver um PlcGenerationRequest montado.

        return new ExportResult(false, Array.Empty<ExportArtifact>(),
            "PlcCodeExportProvider requer um PlcGenerationRequest montado " +
            "pela camada Application (wire no Bloco E).");
    }

    /// <inheritdoc/>
    public ExportResult Validate(ExportRequest request)
    {
        if (request is null)
        {
            return new ExportResult(false, Array.Empty<ExportArtifact>(),
                "ExportRequest nulo.");
        }

        if (!CanExport(request))
        {
            return new ExportResult(false, Array.Empty<ExportArtifact>(),
                $"Formato {request.Format} não suportado.");
        }

        return new ExportResult(true, Array.Empty<ExportArtifact>(),
            "Validação de formato OK. Spec será resolvido no momento da exportação.");
    }

    /// <summary>
    /// Ponto de entrada direto: dado um <see cref="PlcGenerationRequest"/>,
    /// delega ao registry e empacota o resultado como <see cref="ExportArtifact"/>.
    /// </summary>
    public ExportResult ExportPlcCode(PlcGenerationRequest plcRequest)
    {
        ArgumentNullException.ThrowIfNull(plcRequest);

        var generation = _registry.Generate(plcRequest);

        if (!generation.Success)
        {
            return new ExportResult(
                false,
                Array.Empty<ExportArtifact>(),
                string.Join("; ", generation.Errors));
        }

        var artifact = new ExportArtifact(
            name: generation.BlockName,
            extension: ".st",
            content: generation.StructuredText,
            encoding: "utf-8",
            metadata: new Dictionary<string, string>
            {
                ["target"] = generation.Target.ToString(),
                ["warnings"] = string.Join("|", generation.Warnings)
            });

        return new ExportResult(
            true,
            new[] { artifact },
            generation.Warnings.Count > 0
                ? $"Gerado com {generation.Warnings.Count} avisos."
                : "Geração concluída.");
    }
}
