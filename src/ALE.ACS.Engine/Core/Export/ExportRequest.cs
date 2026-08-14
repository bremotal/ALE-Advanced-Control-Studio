using ALE.ACS.Engine.Core.Applications;

namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Represents a request to export an engineering project.
/// </summary>
public sealed class ExportRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportRequest"/> class.
    /// </summary>
    /// <param name="project">The project to export.</param>
    /// <param name="application">The application to export.</param>
    /// <param name="strategy">The strategy to export.</param>
    /// <param name="format">The requested export format.</param>
    public ExportRequest(EngineeringProject? project, ControlApplication? application, EngineeringStrategy? strategy, ExportFormat format)
    {
        Project = project;
        Application = application;
        Strategy = strategy;
        Format = format;
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    public EngineeringProject? Project { get; }

    /// <summary>
    /// Gets the application.
    /// </summary>
    public ControlApplication? Application { get; }

    /// <summary>
    /// Gets the strategy.
    /// </summary>
    public EngineeringStrategy? Strategy { get; }

    /// <summary>
    /// Gets the export format.
    /// </summary>
    public ExportFormat Format { get; }
}
