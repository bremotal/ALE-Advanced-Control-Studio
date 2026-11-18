using System.Text.Json;
using System.Text.Json.Serialization;
using ALE.ACS.Engine.Core.Projects;

namespace ALE.ACS.Infrastructure.Persistence;

/// <summary>
/// Implementação concreta de <see cref="IPersistenceProvider"/> que grava
/// <see cref="ProjectSnapshot"/>s como arquivos JSON em disco.
/// </summary>
/// <remarks>
/// Formato: um arquivo por snapshot, nomeado <c>{Id}.aleacs</c> dentro do
/// diretório base informado no construtor. O payload (string JSON fornecida
/// pelo repositório) é armazenado como conteúdo do arquivo.
/// </remarks>
public sealed class FilePersistenceProvider : IPersistenceProvider
{
    private readonly FileSystem.IFileSystem _fileSystem;
    private readonly string _baseDirectory;

    public FilePersistenceProvider(FileSystem.IFileSystem fileSystem, string baseDirectory)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);
        _baseDirectory = baseDirectory;
        _fileSystem.EnsureDirectory(_baseDirectory);
    }

    /// <inheritdoc/>
    public RepositoryResult Save(ProjectSnapshot snapshot, SaveOptions options)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        try
        {
            var path = GetPath(snapshot.Id);
            var envelope = new SnapshotEnvelope
            {
                Id = snapshot.Id,
                Name = snapshot.Name,
                Version = snapshot.Version,
                CreatedAt = snapshot.CreatedAt,
                UpdatedAt = snapshot.UpdatedAt,
                Payload = snapshot.Payload,
                Metadata = new Dictionary<string, string>(snapshot.Metadata)
            };

            var json = JsonSerializer.Serialize(
                envelope,
                SnapshotEnvelopeJsonContext.Default.SnapshotEnvelope);

            _fileSystem.WriteAllText(path, json);

            return RepositoryResult.CreateSuccess(snapshot);
        }
        catch (Exception ex)
        {
            return RepositoryResult.CreateFailure(
                $"Falha ao salvar snapshot '{snapshot.Id}': {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public RepositoryResult Load(string id, LoadOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        try
        {
            var path = GetPath(id);

            if (!_fileSystem.FileExists(path))
            {
                return RepositoryResult.CreateFailure(
                    $"Snapshot '{id}' não encontrado em '{path}'.");
            }

            var json = _fileSystem.ReadAllText(path);
            var envelope = JsonSerializer.Deserialize(
                json,
                SnapshotEnvelopeJsonContext.Default.SnapshotEnvelope);

            if (envelope is null)
            {
                return RepositoryResult.CreateFailure(
                    $"Falha ao deserializar snapshot '{id}'.");
            }

            var snapshot = new ProjectSnapshot(
                envelope.Id,
                envelope.Name,
                envelope.Version,
                envelope.CreatedAt,
                envelope.UpdatedAt,
                envelope.Payload,
                envelope.Metadata ?? new Dictionary<string, string>());

            return RepositoryResult.CreateSuccess(snapshot);
        }
        catch (Exception ex)
        {
            return RepositoryResult.CreateFailure(
                $"Falha ao carregar snapshot '{id}': {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public bool Exists(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _fileSystem.FileExists(GetPath(id));
    }

    /// <inheritdoc/>
    public RepositoryResult Delete(string id, DeleteOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        try
        {
            _fileSystem.DeleteFile(GetPath(id));
            return RepositoryResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return RepositoryResult.CreateFailure(
                $"Falha ao excluir snapshot '{id}': {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public RepositoryResult Validate(ProjectSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (string.IsNullOrWhiteSpace(snapshot.Id))
        {
            return RepositoryResult.CreateFailure("Snapshot sem Id.");
        }

        if (string.IsNullOrWhiteSpace(snapshot.Name))
        {
            return RepositoryResult.CreateFailure("Snapshot sem Name.");
        }

        if (string.IsNullOrWhiteSpace(snapshot.Payload))
        {
            return RepositoryResult.CreateFailure("Snapshot sem Payload.");
        }

        return RepositoryResult.CreateSuccess(snapshot);
    }

    private string GetPath(string id)
    {
        var safeId = string.Concat(id.Select(c => char.IsLetterOrDigit(c) || c == '-' ? c : '_'));
        return _fileSystem.Combine(_baseDirectory, $"{safeId}.aleacs");
    }
}

/// <summary>
/// Envelope serializável do snapshot, compatível com JSON.
/// </summary>
internal sealed class SnapshotEnvelope
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Payload { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Contexto JSON para serialização com source-gen (performance + AOT-friendly).
/// </summary>
[JsonSerializable(typeof(SnapshotEnvelope))]
internal sealed partial class SnapshotEnvelopeJsonContext : JsonSerializerContext
{
}
