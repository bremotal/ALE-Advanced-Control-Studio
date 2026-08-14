using ALE.ACS.Engine.Core.Diagnostics;
using ALE.ACS.Engine.Core.Execution;

namespace ALE.ACS.Engine.Core.Runtime;

/// <summary>
/// Represents the outcome of a runtime execution.
/// </summary>
public sealed class RuntimeResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeResult"/> class.
    /// </summary>
    /// <param name="success">Whether the runtime execution succeeded.</param>
    /// <param name="diagnostics">The emitted diagnostics.</param>
    /// <param name="statistics">The execution statistics.</param>
    /// <param name="totalTime">The total execution time.</param>
    public RuntimeResult(bool success, IReadOnlyCollection<DiagnosticMessage> diagnostics, ExecutionStatistics statistics, TimeSpan totalTime)
    {
        Success = success;
        Diagnostics = diagnostics;
        Statistics = statistics;
        TotalTime = totalTime;
    }

    /// <summary>
    /// Gets a value indicating whether the runtime execution succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the emitted diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticMessage> Diagnostics { get; }

    /// <summary>
    /// Gets the execution statistics.
    /// </summary>
    public ExecutionStatistics Statistics { get; }

    /// <summary>
    /// Gets the total runtime duration.
    /// </summary>
    public TimeSpan TotalTime { get; }
}
