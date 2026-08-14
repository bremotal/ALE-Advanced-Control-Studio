namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Coordinates export providers and consolidates results.
/// </summary>
public sealed class ExportManager
{
    private readonly IReadOnlyCollection<IExportProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportManager"/> class.
    /// </summary>
    /// <param name="providers">The export providers.</param>
    public ExportManager(IReadOnlyCollection<IExportProvider>? providers)
    {
        _providers = providers ?? Array.Empty<IExportProvider>();
    }

    /// <summary>
    /// Exports the supplied request.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result.</returns>
    public ExportResult Export(ExportRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var provider = _providers.FirstOrDefault(candidate => candidate.CanExport(request));
        return provider?.Export(request) ?? new ExportResult(false, Array.Empty<ExportArtifact>(), "No export provider available.");
    }
}
