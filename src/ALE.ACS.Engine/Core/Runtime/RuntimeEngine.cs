using ALE.ACS.Engine.Core.Execution;
using EngineExecutionContext = ALE.ACS.Engine.Core.Execution.ExecutionContext;

namespace ALE.ACS.Engine.Core.Runtime;

/// <summary>
/// Coordinates the lifecycle of runtime sessions.
/// </summary>
public sealed class RuntimeEngine
{
    /// <summary>
    /// Executes the supplied runtime context.
    /// </summary>
    /// <param name="context">The runtime context.</param>
    /// <returns>The runtime result.</returns>
    public RuntimeResult Execute(RuntimeContext context)
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

        return new RuntimeResult(
            pipelineResult.Success,
            pipelineResult.Diagnostics,
            pipelineResult.Statistics,
            TimeSpan.Zero);
    }
}