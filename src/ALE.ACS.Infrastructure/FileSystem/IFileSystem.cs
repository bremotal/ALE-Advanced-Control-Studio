namespace ALE.ACS.Infrastructure.FileSystem;

/// <summary>
/// Abstração sobre o sistema de arquivos para permitir testes com mock.
/// </summary>
public interface IFileSystem
{
    /// <summary>Lê todo o conteúdo de um arquivo de texto.</summary>
    string ReadAllText(string path);

    /// <summary>Escreve conteúdo num arquivo de texto (sobrescreve).</summary>
    void WriteAllText(string path, string content);

    /// <summary>Verifica se um arquivo existe.</summary>
    bool FileExists(string path);

    /// <summary>Cria o diretório se não existir.</summary>
    void EnsureDirectory(string path);

    /// <summary>Exclui um arquivo (silencioso se não existir).</summary>
    void DeleteFile(string path);

    /// <summary>Combina caminhos.</summary>
    string Combine(params string[] paths);
}

/// <summary>
/// Implementação real de <see cref="IFileSystem"/> usando <see cref="System.IO.File"/> e <see cref="System.IO.Directory"/>.
/// </summary>
public sealed class PhysicalFileSystem : IFileSystem
{
    /// <inheritdoc/>
    public string ReadAllText(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.ReadAllText(path);
    }

    /// <inheritdoc/>
    public void WriteAllText(string path, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);
        EnsureDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    /// <inheritdoc/>
    public bool FileExists(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return File.Exists(path);
    }

    /// <inheritdoc/>
    public void EnsureDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        Directory.CreateDirectory(path);
    }

    /// <inheritdoc/>
    public void DeleteFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <inheritdoc/>
    public string Combine(params string[] paths)
        => Path.Combine(paths);
}
