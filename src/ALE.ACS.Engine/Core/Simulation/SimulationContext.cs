using ALE.ACS.Engine.Core.Applications;

namespace ALE.ACS.Engine.Core.Simulation;

/// <summary>
/// Provides state for a simulation run.
/// </summary>
public sealed class SimulationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationContext"/> class.
    /// </summary>
    /// <param name="project">The active project.</param>
    /// <param name="application">The active application.</param>
    /// <param name="strategy">The active strategy.</param>
    /// <param name="variables">The variables available to the simulation.</param>
    /// <param name="time">The current simulation time.</param>
    public SimulationContext(EngineeringProject? project, ControlApplication? application, EngineeringStrategy? strategy, IReadOnlyCollection<EngineeringVariable>? variables, TimeSpan time)
    {
        Project = project;
        Application = application;
        Strategy = strategy;
        Variables = variables ?? [];
        Time = time;
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
    /// Gets the variables in scope.
    /// </summary>
    public IReadOnlyCollection<EngineeringVariable> Variables { get; }

    /// <summary>
    /// Gets the current simulation time.
    /// </summary>
    public TimeSpan Time { get; }
}
