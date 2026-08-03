namespace ALE.ACS.Engine.Core;

public sealed class ControllerResult
{
    public ControllerResult(
        ControllerVariableCollection outputVariables,
        ControllerStatus status,
        IReadOnlyList<ControllerMessage> messages,
        TimeSpan executionTime,
        bool success)
    {
        OutputVariables = outputVariables;
        Status = status;
        Messages = messages;
        ExecutionTime = executionTime;
        Success = success;
    }

    public ControllerVariableCollection OutputVariables { get; }

    public ControllerStatus Status { get; }

    public IReadOnlyList<ControllerMessage> Messages { get; }

    public TimeSpan ExecutionTime { get; }

    public bool Success { get; }
}