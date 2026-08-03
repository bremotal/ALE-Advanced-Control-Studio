using FluentAssertions;
using Xunit;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions.Tests;

public sealed class GeneralizedBellMembershipFunctionTests
{
    [Fact]
    public void Constructor_Should_Create_Instance_With_Valid_Parameters()
    {
        var function = new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 2.0,
            slope: 4.0,
            center: 10.0);

        function.Name.Should().Be("Generalized Bell");
        function.Width.Should().Be(2.0);
        function.Slope.Should().Be(4.0);
        function.Center.Should().Be(10.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Constructor_Should_Throw_When_Width_Is_Invalid(double width)
    {
        var action = () => new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: width,
            slope: 2.0,
            center: 0.0);

        action.Should()
              .Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Constructor_Should_Throw_When_Slope_Is_Invalid(double slope)
    {
        var action = () => new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 2.0,
            slope: slope,
            center: 0.0);

        action.Should()
              .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Membership_Should_Be_One_At_Center()
    {
        var function = new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 2.0,
            slope: 2.0,
            center: 5.0);

        var membership = function.Evaluate(5.0);

        membership.Should().BeApproximately(1.0, 1e-12);
    }

    [Fact]
    public void Membership_Should_Be_Symmetric()
    {
        var function = new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 2.0,
            slope: 2.0,
            center: 5.0);

        var left = function.Evaluate(3.0);
        var right = function.Evaluate(7.0);

        left.Should().BeApproximately(right, 1e-12);
    }

    [Fact]
    public void Membership_Should_Decrease_Away_From_Center()
    {
        var function = new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 2.0,
            slope: 2.0,
            center: 0.0);

        var center = function.Evaluate(0.0);
        var near = function.Evaluate(1.0);
        var far = function.Evaluate(10.0);

        center.Should().BeGreaterThan(near);
        near.Should().BeGreaterThan(far);
    }

    [Fact]
    public void Membership_Should_Be_Close_To_Zero_For_Distant_Values()
    {
        var function = new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 1.0,
            slope: 4.0,
            center: 0.0);

        var membership = function.Evaluate(100.0);

        membership.Should().BeLessThan(1e-8);
    }

    [Fact]
    public void Membership_Should_Follow_The_Generalized_Bell_Equation()
    {
        var function = new GeneralizedBellMembershipFunction(
            name: "Generalized Bell",
            width: 2.0,
            slope: 2.0,
            center: 0.0);

        var actual = function.Evaluate(2.0);

        actual.Should().BeApproximately(0.5, 1e-12);
    }
}