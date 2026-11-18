using System.Text.Json;
using System.Text.Json.Serialization;
using ALE.ACS.Engine.Core.Export;

namespace ALE.ACS.Export.Providers;

/// <summary>
/// Provider de exportação que serializa um <see cref="ExportRequest"/> para JSON.
/// Útil para snapshots de engenharia, troca entre instâncias do ACS e backup.
/// </summary>
public sealed class JsonExportProvider : IExportProvider
{
    /// <inheritdoc/>
    public bool CanExport(ExportRequest request)
        => request is not null && request.Format == ExportFormat.Json;

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
                $"Formato {request.Format} não suportado pelo JsonExportProvider.");
        }

        try
        {
            var payload = new ExportPayload
            {
                ProjectName = request.Project?.Name,
                ApplicationName = request.Application?.Name,
                StrategyName = request.Strategy?.Name,
                Format = request.Format.ToString(),
                ExportedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(
                payload,
                ExportPayloadJsonContext.Default.ExportPayload);

            var artifact = new ExportArtifact(
                name: $"{payload.ProjectName ?? "project"}_{payload.ExportedAt:yyyyMMdd_HHmmss}",
                extension: ".json",
                content: json,
                encoding: "utf-8",
                metadata: new Dictionary<string, string>
                {
                    ["format"] = "json",
                    ["version"] = "1.0"
                });

            return new ExportResult(
                true,
                new[] { artifact },
                "Exportação JSON concluída.");
        }
        catch (Exception ex)
        {
            return new ExportResult(
                false,
                Array.Empty<ExportArtifact>(),
                $"Falha ao serializar: {ex.Message}");
        }
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
            "Validação OK.");
    }
}

internal sealed class ExportPayload
{
    public string? ProjectName { get; set; }
    public string? ApplicationName { get; set; }
    public string? StrategyName { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
}

[JsonSerializable(typeof(ExportPayload))]
[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ExportPayloadJsonContext : JsonSerializerContext
{
}
