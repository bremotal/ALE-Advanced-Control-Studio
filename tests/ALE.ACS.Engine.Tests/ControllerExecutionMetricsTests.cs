using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Tests.Core;

public class ControllerExecutionMetricsTests
{
    [Fact]
    public void Should_Calculate_ElapsedTime_From_Start_And_End()
    {
        var start = DateTime.UtcNow;
        var end = start.AddMilliseconds(250);

        var metrics = new ControllerExecutionMetrics(start, end);

        Assert.Equal(start, metrics.StartTime);
        Assert.Equal(end, metrics.EndTime);
        Assert.Equal(TimeSpan.FromMilliseconds(250), metrics.ElapsedTime);
    }

    [Fact]
    public void Should_Throw_When_EndTime_Is_Earlier_Than_StartTime()
    {
        var start = DateTime.UtcNow;
        var end = start.AddMilliseconds(-1);

        Assert.Throws<ArgumentException>(() =>
            new ControllerExecutionMetrics(start, end));
    }
}
