namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Contains coarse execution metrics for the pipeline.
/// </summary>
public sealed class ExecutionStatistics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionStatistics"/> class.
    /// </summary>
    /// <param name="blockCount">The number of executed blocks.</param>
    /// <param name="variableCount">The number of variables in scope.</param>
    /// <param name="warningCount">The number of warnings emitted.</param>
    /// <param name="errorCount">The number of errors emitted.</param>
    /// <param name="executionTime">The elapsed execution time.</param>
    public ExecutionStatistics(int blockCount, int variableCount, int warningCount, int errorCount, TimeSpan executionTime)
    {
        BlockCount = blockCount;
        VariableCount = variableCount;
        WarningCount = warningCount;
        ErrorCount = errorCount;
        ExecutionTime = executionTime;
    }

    /// <summary>
    /// Gets the number of blocks in the strategy.
    /// </summary>
    public int BlockCount { get; }

    /// <summary>
    /// Gets the number of variables in scope.
    /// </summary>
    public int VariableCount { get; }

    /// <summary>
    /// Gets the number of warnings emitted.
    /// </summary>
    public int WarningCount { get; }

    /// <summary>
    /// Gets the number of errors emitted.
    /// </summary>
    public int ErrorCount { get; }

    /// <summary>
    /// Gets the elapsed execution time.
    /// </summary>
    public TimeSpan ExecutionTime { get; }
}
