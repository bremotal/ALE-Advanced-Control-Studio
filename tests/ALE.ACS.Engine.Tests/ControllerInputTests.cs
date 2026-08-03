using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Tests.Core;

public class ControllerInputTests
{
    [Fact]
    public void Should_Create_Empty_Input()
    {
        var input = new ControllerInput();

        Assert.NotNull(input.Variables);
        Assert.Equal(0, input.Variables.Count);
    }

    [Fact]
    public void Should_Accept_Any_Number_Of_Variables()
    {
        var variables = Enumerable.Range(1, 100)
            .Select(i => new ControllerVariable($"Var{i}", i, DateTime.UtcNow, "Good"));

        var input = new ControllerInput(variables);

        Assert.Equal(100, input.Variables.Count);
    }

    [Fact]
    public void Should_Retrieve_Variable_By_Name_Case_Insensitive()
    {
        var input = new ControllerInput();
        input.AddVariable(new ControllerVariable("SetPoint", 10.0, DateTime.UtcNow, "Good"));

        var found = input.TryGetVariable("setpoint", out var variable);

        Assert.True(found);
        Assert.NotNull(variable);
        Assert.Equal("SetPoint", variable.Name);
    }
}
