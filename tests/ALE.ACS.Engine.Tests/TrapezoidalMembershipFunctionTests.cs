using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Tests.Fuzzy.MembershipFunctions;

public class TrapezoidalMembershipFunctionTests
{
    private static TrapezoidalMembershipFunction CreateDefault()
    {
        return new("High", 0.0, 25.0, 75.0, 100.0);
    }

    #region Construction

    [Fact]
    public void Should_Create_Valid_Trapezoidal_Membership_Function()
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
            new TrapezoidalMembershipFunction(
                name,
                0,
                25,
                75,
                100));
    }

    [Fact]
    public void Should_Throw_When_Name_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TrapezoidalMembershipFunction(
                null!,
                0,
                25,
                75,
                100));
    }

    [Theory]
    [InlineData(10, 5, 20, 30)]
    [InlineData(0, 20, 10, 30)]
    [InlineData(0, 20, 30, 25)]
    public void Should_Throw_When_Parameters_Are_Invalid(
        double left,
        double leftShoulder,
        double rightShoulder,
        double right)
    {
        Assert.Throws<ArgumentException>(() =>
            new TrapezoidalMembershipFunction(
                "High",
                left,
                leftShoulder,
                rightShoulder,
                right));
    }

    #endregion

    #region Properties

    [Fact]
    public void Should_Expose_Name()
    {
        var mf = CreateDefault();

        Assert.Equal("High", mf.Name);
    }

    #endregion

    #region Evaluation

    [Theory]
    [InlineData(-10.0, 0.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(12.5, 0.5)]
    [InlineData(25.0, 1.0)]
    [InlineData(50.0, 1.0)]
    [InlineData(75.0, 1.0)]
    [InlineData(87.5, 0.5)]
    [InlineData(100.0, 0.0)]
    [InlineData(110.0, 0.0)]
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