namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Represents the result of an export operation.
/// </summary>
public sealed class ExportResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportResult"/> class.
    /// </summary>
    /// <param name="success">Whether the export succeeded.</param>
    /// <param name="artifacts">The generated artifacts.</param>
    /// <param name="message">The result message.</param>
    public ExportResult(bool success, IReadOnlyCollection<ExportArtifact>? artifacts, string? message)
    {
        Success = success;
        Artifacts = artifacts ?? Array.Empty<ExportArtifact>();
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the export succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the generated artifacts.
    /// </summary>
    public IReadOnlyCollection<ExportArtifact> Artifacts { get; }

    /// <summary>
    /// Gets the result message.
    /// </summary>
    public string? Message { get; }
}
