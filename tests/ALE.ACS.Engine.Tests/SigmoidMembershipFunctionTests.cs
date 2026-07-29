using FluentAssertions;
using Xunit;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions.Tests;

public sealed class SigmoidMembershipFunctionTests
{
    [Fact]
    public void Constructor_Should_Create_Instance_With_Valid_Parameters()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: 2.0,
            center: 10.0);

        function.Name.Should().Be("Sigmoid");
        function.Slope.Should().Be(2.0);
        function.Center.Should().Be(10.0);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Slope_Is_Zero()
    {
        var action = () => new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: 0.0,
            center: 0.0);

        action.Should()
              .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Membership_Should_Be_Half_At_Center()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: 2.0,
            center: 5.0);

        var membership = function.GetMembership(5.0);

        membership.Should().BeApproximately(0.5, 1e-12);
    }

    [Fact]
    public void Increasing_Sigmoid_Should_Approach_One()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: 2.0,
            center: 0.0);

        function.GetMembership(100.0)
            .Should()
            .BeGreaterThan(0.999999);
    }

    [Fact]
    public void Increasing_Sigmoid_Should_Approach_Zero()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: 2.0,
            center: 0.0);

        function.GetMembership(-100.0)
            .Should()
            .BeLessThan(0.000001);
    }

    [Fact]
    public void Decreasing_Sigmoid_Should_Approach_One_On_Left()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: -2.0,
            center: 0.0);

        function.GetMembership(-100.0)
            .Should()
            .BeGreaterThan(0.999999);
    }

    [Fact]
    public void Decreasing_Sigmoid_Should_Approach_Zero_On_Right()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: -2.0,
            center: 0.0);

        function.GetMembership(100.0)
            .Should()
            .BeLessThan(0.000001);
    }

    [Fact]
    public void Membership_Should_Be_Monotonic_For_Positive_Slope()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: 2.0,
            center: 0.0);

        var a = function.GetMembership(-1.0);
        var b = function.GetMembership(0.0);
        var c = function.GetMembership(1.0);

        a.Should().BeLessThan(b);
        b.Should().BeLessThan(c);
    }

    [Fact]
    public void Membership_Should_Be_Monotonic_For_Negative_Slope()
    {
        var function = new SigmoidMembershipFunction(
            name: "Sigmoid",
            slope: -2.0,
            center: 0.0);

        var a = function.GetMembership(-1.0);
        var b = function.GetMembership(0.0);
        var c = function.GetMembership(1.0);

        a.Should().BeGreaterThan(b);
        b.Should().BeGreaterThan(c);
    }
}