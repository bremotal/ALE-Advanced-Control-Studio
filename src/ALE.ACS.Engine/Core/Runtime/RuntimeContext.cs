using ALE.ACS.Engine.Core.Applications;

namespace ALE.ACS.Engine.Core.Runtime;

/// <summary>
/// Provides the environment context for a runtime session.
/// </summary>
public sealed class RuntimeContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeContext"/> class.
    /// </summary>
    /// <param name="project">The active project.</param>
    /// <param name="application">The active application.</param>
    /// <param name="strategy">The active strategy.</param>
    /// <param name="variables">The variables available to runtime.</param>
    public RuntimeContext(EngineeringProject? project, ControlApplication? application, EngineeringStrategy? strategy, IReadOnlyCollection<EngineeringVariable>? variables)
    {
        Project = project;
        Application = application;
        Strategy = strategy;
        Variables = variables ?? [];
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
    /// Gets the variables available to runtime.
    /// </summary>
    public IReadOnlyCollection<EngineeringVariable> Variables { get; }
}
