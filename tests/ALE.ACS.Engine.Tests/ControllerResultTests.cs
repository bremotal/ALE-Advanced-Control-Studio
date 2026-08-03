using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Tests.Core;

public class ControllerResultTests
{
    [Fact]
    public void Should_Expose_Result_Data()
    {
        var outputs = new ControllerVariableCollection
        {
            new ControllerVariable("MV", 2.0, DateTime.UtcNow, "Good")
        };

        var messages = new List<ControllerMessage>
        {
            new(ControllerMessageSeverity.Info, "CTRL_OK", "Execution completed.", DateTime.UtcNow)
        };

        var executionTime = TimeSpan.FromMilliseconds(12);

        var result = new ControllerResult(
            outputs,
            ControllerStatus.Success,
            messages,
            executionTime,
            true);

        Assert.Equal(outputs, result.OutputVariables);
        Assert.Equal(ControllerStatus.Success, result.Status);
        Assert.Single(result.Messages);
        Assert.Equal(executionTime, result.ExecutionTime);
        Assert.True(result.Success);
    }
}
