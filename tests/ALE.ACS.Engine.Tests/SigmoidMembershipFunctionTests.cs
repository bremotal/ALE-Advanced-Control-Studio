using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using FluentAssertions;
using Xunit;

namespace ALE.ACS.Engine.Tests;

public class SigmoidMembershipFunctionTests
{
    [Fact]
    public void Constructor_WithZeroSlope_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => new SigmoidMembershipFunction(0.0, 0.0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetMembership_AtCenter_ShouldReturnHalf()
    {
        var mf = new SigmoidMembershipFunction(1.0, 0.0);

        double value = mf.GetMembership(0.0);

        value.Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void GetMembership_WithPositiveSlope_ShouldIncrease()
    {
        var mf = new SigmoidMembershipFunction(2.0, 0.0);

        double left = mf.GetMembership(-5.0);
        double center = mf.GetMembership(0.0);
        double right = mf.GetMembership(5.0);

        left.Should().BeLessThan(center);
        center.Should().BeLessThan(right);
    }

    [Fact]
    public void GetMembership_WithNegativeSlope_ShouldDecrease()
    {
        var mf = new SigmoidMembershipFunction(-2.0, 0.0);

        double left = mf.GetMembership(-5.0);
        double center = mf.GetMembership(0.0);
        double right = mf.GetMembership(5.0);

        left.Should().BeGreaterThan(center);
        center.Should().BeGreaterThan(right);
    }

    [Fact]
    public void GetMembership_ForLargeNegativeValue_ShouldApproachZero()
    {
        var mf = new SigmoidMembershipFunction(1.0, 0.0);

        double value = mf.GetMembership(-100.0);

        value.Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void GetMembership_ForLargePositiveValue_ShouldApproachOne()
    {
        var mf = new SigmoidMembershipFunction(1.0, 0.0);

        double value = mf.GetMembership(100.0);

        value.Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void GetMembership_ShouldAlwaysReturnValueBetweenZeroAndOne()
    {
        var mf = new SigmoidMembershipFunction(3.0, 5.0);

        for (double x = -100.0; x <= 100.0; x += 0.5)
        {
            double value = mf.GetMembership(x);

            value.Should().BeInRange(0.0, 1.0);
        }
    }

    [Fact]
    public void Properties_ShouldBeInitializedCorrectly()
    {
        var mf = new SigmoidMembershipFunction(2.5, 10.0);

        mf.Slope.Should().Be(2.5);
        mf.Center.Should().Be(10.0);
    }
}