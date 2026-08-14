using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Tests.Fuzzy.MembershipFunctions;

public class TrapezoidalMembershipFunctionTests
{
    private static TrapezoidalMembershipFunction CreateDefault()
    {
        return new(0.0, 25.0, 75.0, 100.0);
    }

    [Theory]
    [InlineData(-10.0)]
    [InlineData(110.0)]
    public void Evaluate_Should_Return_Zero_When_X_Is_Outside_Domain(double x)
    {
        var function = CreateDefault();

        var result = function.Evaluate(x);

        Assert.Equal(0.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Rising_Value_On_Ascending_Edge()
    {
        var function = CreateDefault();

        var result = function.Evaluate(12.5);

        Assert.Equal(0.5, result, 6);
    }

    [Theory]
    [InlineData(25.0)]
    [InlineData(50.0)]
    [InlineData(75.0)]
    public void Evaluate_Should_Return_One_On_Plateau(double x)
    {
        var function = CreateDefault();

        var result = function.Evaluate(x);

        Assert.Equal(1.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Falling_Value_On_Descending_Edge()
    {
        var function = CreateDefault();

        var result = function.Evaluate(87.5);

        Assert.Equal(0.5, result, 6);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void Evaluate_Should_Return_Zero_On_Extremes(double x)
    {
        var function = CreateDefault();

        var result = function.Evaluate(x);

        Assert.Equal(0.0, result, 6);
    }

    [Fact]
    public void GetSupportBounds_Should_Return_The_Base_Interval()
    {
        var function = CreateDefault();

        var supportBounds = function.GetSupportBounds();

        Assert.Equal((0.0, 100.0), supportBounds);
    }

    [Theory]
    [InlineData(10.0, 5.0, 20.0, 30.0)]
    [InlineData(0.0, 20.0, 10.0, 30.0)]
    [InlineData(0.0, 20.0, 30.0, 25.0)]
    [InlineData(0.0, 10.0, 9.0, 20.0)]
    public void Constructor_Should_Throw_When_Parameters_Are_Invalid(
        double a,
        double b,
        double c,
        double d)
    {
        Assert.Throws<ArgumentException>(() => new TrapezoidalMembershipFunction(a, b, c, d));
    }
}