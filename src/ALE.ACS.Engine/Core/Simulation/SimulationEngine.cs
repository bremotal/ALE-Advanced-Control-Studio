using ALE.ACS.Engine.Core.Execution;
using EngineExecutionContext = ALE.ACS.Engine.Core.Execution.ExecutionContext;

namespace ALE.ACS.Engine.Core.Simulation;

/// <summary>
/// Coordinates the lifecycle of a simulation run.
/// </summary>
public sealed class SimulationEngine
{
    /// <summary>
    /// Runs a simulation context through the execution pipeline.
    /// </summary>
    /// <param name="context">The simulation context.</param>
    /// <returns>The simulation result.</returns>
    public SimulationResult Run(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var executionContext = new EngineExecutionContext(
            context.Project,
            context.Application,
            context.Strategy,
            context.Variables,
            new Dictionary<string, object?>());

        var pipeline = new ExecutionPipeline();

        var pipelineResult = pipeline.Execute(executionContext);

        return new SimulationResult(
            pipelineResult.Success,
            pipelineResult.Outputs,
            pipelineResult.Diagnostics,
            pipelineResult.Statistics,
            context.Time);
    }
}