namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Represents the current state of an execution pipeline.
/// </summary>
public enum ExecutionState
{
    /// <summary>
    /// The execution has not started.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The execution is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The execution completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The execution failed.
    /// </summary>
    Failed
}
