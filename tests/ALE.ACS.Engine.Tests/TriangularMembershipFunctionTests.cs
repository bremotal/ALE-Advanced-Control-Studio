using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using Xunit;

namespace ALE.ACS.Engine.Tests.Fuzzy.MembershipFunctions;

public sealed class TriangularMembershipFunctionTests
{
    private static TriangularMembershipFunction CreateDefault()
    {
        return new TriangularMembershipFunction(0.0, 5.0, 10.0);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(11.0)]
    public void Evaluate_Should_Return_Zero_When_X_Is_Outside_Interval(double x)
    {
        var function = CreateDefault();

        var result = function.Evaluate(x);

        Assert.Equal(0.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Zero_When_X_Equals_A()
    {
        var function = CreateDefault();

        var result = function.Evaluate(0.0);

        Assert.Equal(0.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Rising_Value_When_A_Is_Less_Than_X_And_X_Is_Less_Than_B()
    {
        var function = CreateDefault();

        var result = function.Evaluate(2.5);

        Assert.Equal(0.5, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_One_When_X_Equals_B()
    {
        var function = CreateDefault();

        var result = function.Evaluate(5.0);

        Assert.Equal(1.0, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Falling_Value_When_B_Is_Less_Than_X_And_X_Is_Less_Than_C()
    {
        var function = CreateDefault();

        var result = function.Evaluate(7.5);

        Assert.Equal(0.5, result, 6);
    }

    [Fact]
    public void Evaluate_Should_Return_Zero_When_X_Equals_C()
    {
        var function = CreateDefault();

        var result = function.Evaluate(10.0);

        Assert.Equal(0.0, result, 6);
    }

    [Fact]
    public void GetSupportBounds_Should_Return_The_Base_Interval()
    {
        var function = CreateDefault();

        var supportBounds = function.GetSupportBounds();

        Assert.Equal((0.0, 10.0), supportBounds);
    }

    [Theory]
    [InlineData(0.0, 0.0, 1.0)]
    [InlineData(0.0, 1.0, 1.0)]
    [InlineData(2.0, 1.0, 3.0)]
    public void Constructor_Should_Throw_When_Points_Do_Not_Satisfy_A_Less_Than_B_Less_Than_C(
        double a,
        double b,
        double c)
    {
        Assert.Throws<ArgumentException>(() => new TriangularMembershipFunction(a, b, c));
    }
}