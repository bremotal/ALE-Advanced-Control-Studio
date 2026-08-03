using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Tests.Core;

public class ControllerVariableTests
{
    [Fact]
    public void Should_Create_ControllerVariable_With_Valid_Data()
    {
        var timestamp = DateTime.UtcNow;

        var variable = new ControllerVariable("PV", 12.5, timestamp, "Good");

        Assert.Equal("PV", variable.Name);
        Assert.Equal(12.5, variable.Value);
        Assert.Equal(timestamp, variable.Timestamp);
        Assert.Equal("Good", variable.Quality);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_Invalid(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new ControllerVariable(name, 1.0, DateTime.UtcNow, "Good"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Should_Throw_When_Quality_Is_Invalid(string quality)
    {
        Assert.Throws<ArgumentException>(() =>
            new ControllerVariable("PV", 1.0, DateTime.UtcNow, quality));
    }
}
