using ALE.ACS.Engine.Core.Diagnostics;
using ALE.ACS.Engine.Core.Execution;

namespace ALE.ACS.Engine.Core.Simulation;

/// <summary>
/// Represents the result of a simulation run.
/// </summary>
public sealed class SimulationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationResult"/> class.
    /// </summary>
    /// <param name="success">Whether the simulation succeeded.</param>
    /// <param name="outputs">The outputs produced by the simulation.</param>
    /// <param name="diagnostics">The diagnostics emitted during the run.</param>
    /// <param name="statistics">The execution statistics.</param>
    /// <param name="simulationTime">The logical simulation time.</param>
    public SimulationResult(bool success, IReadOnlyDictionary<string, object?> outputs, IReadOnlyCollection<DiagnosticMessage> diagnostics, ExecutionStatistics statistics, TimeSpan simulationTime)
    {
        Success = success;
        Outputs = outputs;
        Diagnostics = diagnostics;
        Statistics = statistics;
        SimulationTime = simulationTime;
    }

    /// <summary>
    /// Gets a value indicating whether the simulation succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the outputs produced by the simulation.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Outputs { get; }

    /// <summary>
    /// Gets the diagnostics from the simulation.
    /// </summary>
    public IReadOnlyCollection<DiagnosticMessage> Diagnostics { get; }

    /// <summary>
    /// Gets the execution statistics.
    /// </summary>
    public ExecutionStatistics Statistics { get; }

    /// <summary>
    /// Gets the logical simulation time.
    /// </summary>
    public TimeSpan SimulationTime { get; }
}
