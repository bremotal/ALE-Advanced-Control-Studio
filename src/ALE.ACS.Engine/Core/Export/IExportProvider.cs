namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Defines a provider that can generate export artifacts.
/// </summary>
public interface IExportProvider
{
    /// <summary>
    /// Determines whether the provider can export the supplied request.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>True when this provider can export the request.</returns>
    bool CanExport(ExportRequest request);

    /// <summary>
    /// Exports the supplied request.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result.</returns>
    ExportResult Export(ExportRequest request);

    /// <summary>
    /// Validates the supplied request.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <returns>The export result.</returns>
    ExportResult Validate(ExportRequest request);
}
