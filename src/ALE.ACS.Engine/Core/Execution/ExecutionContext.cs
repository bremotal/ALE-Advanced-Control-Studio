using ALE.ACS.Engine.Core.Applications;

namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Provides shared state for an execution run.
/// </summary>
public sealed class ExecutionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
    /// </summary>
    /// <param name="project">The active project.</param>
    /// <param name="application">The active application.</param>
    /// <param name="strategy">The active strategy.</param>
    /// <param name="variables">The available variables.</param>
    /// <param name="services">The shared services.</param>
    public ExecutionContext(
        EngineeringProject? project,
        ControlApplication? application,
        EngineeringStrategy? strategy,
        IReadOnlyCollection<EngineeringVariable>? variables,
        IReadOnlyDictionary<string, object?>? services)
    {
        Project = project;
        Application = application;
        Strategy = strategy;
        Variables = variables ?? [];
        Services = services ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the active project.
    /// </summary>
    public EngineeringProject? Project { get; }

    /// <summary>
    /// Gets the active application.
    /// </summary>
    public ControlApplication? Application { get; }

    /// <summary>
    /// Gets the active strategy.
    /// </summary>
    public EngineeringStrategy? Strategy { get; }

    /// <summary>
    /// Gets the available variables.
    /// </summary>
    public IReadOnlyCollection<EngineeringVariable> Variables { get; }

    /// <summary>
    /// Gets the shared services.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Services { get; }
}
