using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using Xunit;

namespace ALE.ACS.Engine.Tests.Fuzzy.MembershipFunctions;

public sealed class TriangularMembershipFunctionTests
{
    private static TriangularMembershipFunction CreateDefault()
    {
        return new TriangularMembershipFunction(
            "Triangle",
            0.0,
            5.0,
            10.0);
    }

    #region Construction

    [Fact]
    public void Constructor_Should_Create_Instance()
    {
        var function = CreateDefault();

        Assert.NotNull(function);
        Assert.Equal("Triangle", function.Name);
    }

    [Fact]
    public void Should_Throw_When_Name_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TriangularMembershipFunction(
                null!,
                0.0,
                5.0,
                10.0));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_WhiteSpace(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new TriangularMembershipFunction(
                name,
                0.0,
                5.0,
                10.0));
    }

    [Fact]
    public void Should_Throw_When_Left_Is_Greater_Than_Center()
    {
        Assert.Throws<ArgumentException>(() =>
            new TriangularMembershipFunction(
                "Triangle",
                5.0,
                5.0,
                10.0));
    }

    [Fact]
    public void Should_Throw_When_Center_Is_Greater_Than_Right()
    {
        Assert.Throws<ArgumentException>(() =>
            new TriangularMembershipFunction(
                "Triangle",
                0.0,
                10.0,
                10.0));
    }

    #endregion

    #region Evaluation

    [Theory]
    [InlineData(-1.0, 0.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(2.5, 0.5)]
    [InlineData(5.0, 1.0)]
    [InlineData(7.5, 0.5)]
    [InlineData(10.0, 0.0)]
    [InlineData(11.0, 0.0)]
    public void GetMembership_Should_Return_Expected_Value(
        double input,
        double expected)
    {
        var function = CreateDefault();

        var result = function.GetMembership(input);

        Assert.Equal(expected, result, 6);
    }

    #endregion
}