namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Provides contextual metadata for export operations.
/// </summary>
public sealed class ExportContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportContext"/> class.
    /// </summary>
    /// <param name="metadata">The metadata associated with the export.</param>
    public ExportContext(IReadOnlyDictionary<string, object?>? metadata)
    {
        Metadata = metadata ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the metadata associated with the export operation.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; }
}
