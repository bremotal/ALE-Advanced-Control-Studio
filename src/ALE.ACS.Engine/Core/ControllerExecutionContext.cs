namespace ALE.ACS.Engine.Core;

public sealed class ControllerExecutionContext
{
    public ControllerExecutionContext(ControllerInput input, DateTime startTime)
    {
        Input = input;
        StartTime = startTime;
    }

    public ControllerInput Input { get; }

    public DateTime StartTime { get; }
}