namespace ALE.ACS.Engine.Core;

public sealed class ControllerExecutionMetrics
{
    public ControllerExecutionMetrics(DateTime startTime, DateTime endTime)
    {
        if (endTime < startTime)
            throw new ArgumentException("EndTime cannot be earlier than StartTime.", nameof(endTime));

        StartTime = startTime;
        EndTime = endTime;
    }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }

    public TimeSpan ElapsedTime => EndTime - StartTime;
}