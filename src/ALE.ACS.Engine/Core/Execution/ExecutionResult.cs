using ALE.ACS.Engine.Core.Diagnostics;

namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Represents the outcome of an execution pipeline.
/// </summary>
public sealed class ExecutionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionResult"/> class.
    /// </summary>
    /// <param name="success">Whether the pipeline executed successfully.</param>
    /// <param name="outputs">The output map produced by the pipeline.</param>
    /// <param name="diagnostics">The emitted diagnostics.</param>
    /// <param name="statistics">The execution statistics.</param>
    public ExecutionResult(bool success, IReadOnlyDictionary<string, object?> outputs, IReadOnlyCollection<DiagnosticMessage> diagnostics, ExecutionStatistics statistics)
    {
        Success = success;
        Outputs = outputs;
        Diagnostics = diagnostics;
        Statistics = statistics;
    }

    /// <summary>
    /// Gets a value indicating whether the pipeline succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the outputs produced by the pipeline.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Outputs { get; }

    /// <summary>
    /// Gets the diagnostics emitted during execution.
    /// </summary>
    public IReadOnlyCollection<DiagnosticMessage> Diagnostics { get; }

    /// <summary>
    /// Gets the statistics produced by the pipeline.
    /// </summary>
    public ExecutionStatistics Statistics { get; }
}
