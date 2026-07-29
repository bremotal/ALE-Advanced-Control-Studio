using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Types;

namespace ALE.ACS.Engine.Tests.Fuzzy.MembershipFunctions;

public class PiecewiseLinearMembershipFunctionTests
{
    private static PiecewiseLinearMembershipFunction CreateDefault()
    {
        return new(
            "Custom",
            [
                new MembershipPoint(0.0, 0.0),
                new MembershipPoint(50.0, 1.0),
                new MembershipPoint(100.0, 0.0)
            ]);
    }

    #region Construction

    [Fact]
    public void Should_Create_Valid_PiecewiseLinear_Membership_Function()
    {
        var mf = CreateDefault();

        Assert.NotNull(mf);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_Invalid(string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new PiecewiseLinearMembershipFunction(
                name,
                [
                    new MembershipPoint(0, 0),
                    new MembershipPoint(1, 1)
                ]));
    }

    [Fact]
    public void Should_Throw_When_Name_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PiecewiseLinearMembershipFunction(
                null!,
                [
                    new MembershipPoint(0, 0),
                    new MembershipPoint(1, 1)
                ]));
    }

    [Fact]
    public void Should_Throw_When_Points_Are_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PiecewiseLinearMembershipFunction(
                "Custom",
                null!));
    }

    [Fact]
    public void Should_Throw_When_Less_Than_Two_Points_Are_Provided()
    {
        Assert.Throws<ArgumentException>(() =>
            new PiecewiseLinearMembershipFunction(
                "Custom",
                [
                    new MembershipPoint(0, 0)
                ]));
    }

    [Fact]
    public void Should_Throw_When_Points_Are_Not_Ordered()
    {
        Assert.Throws<ArgumentException>(() =>
            new PiecewiseLinearMembershipFunction(
                "Custom",
                [
                    new MembershipPoint(50, 1),
                    new MembershipPoint(0, 0),
                    new MembershipPoint(100, 0)
                ]));
    }

    [Fact]
    public void Should_Throw_When_Points_Have_Duplicate_X()
    {
        Assert.Throws<ArgumentException>(() =>
            new PiecewiseLinearMembershipFunction(
                "Custom",
                [
                    new MembershipPoint(0, 0),
                    new MembershipPoint(50, 1),
                    new MembershipPoint(50, 0.5),
                    new MembershipPoint(100, 0)
                ]));
    }

    #endregion

    #region Properties

    [Fact]
    public void Should_Expose_Name()
    {
        var mf = CreateDefault();

        Assert.Equal("Custom", mf.Name);
    }

    [Fact]
    public void Should_Expose_All_Points()
    {
        var mf = CreateDefault();

        Assert.Equal(3, mf.Points.Count);
    }

    #endregion

    #region Evaluation

    [Theory]
    [InlineData(-10.0, 0.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(25.0, 0.5)]
    [InlineData(50.0, 1.0)]
    [InlineData(75.0, 0.5)]
    [InlineData(100.0, 0.0)]
    [InlineData(150.0, 0.0)]
    public void Should_Return_Expected_Membership(
        double input,
        double expected)
    {
        var mf = CreateDefault();

        Assert.Equal(
            expected,
            mf.GetMembership(input),
            6);
    }

    #endregion

    #region Boundary Conditions

    [Theory]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void Should_Return_Zero_For_Extreme_Values(
        double input)
    {
        var mf = CreateDefault();

        Assert.Equal(
            0.0,
            mf.GetMembership(input),
            6);
    }

    #endregion
}