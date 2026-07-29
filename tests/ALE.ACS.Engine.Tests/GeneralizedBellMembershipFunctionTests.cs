using FluentAssertions;
using Xunit;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions.Tests;

public sealed class GeneralizedBellMembershipFunctionTests
{
    [Fact]
    public void Constructor_Should_Create_Instance_With_Valid_Parameters()
    {
        // Arrange & Act
        var function = new GeneralizedBellMembershipFunction(
            width: 2.0,
            slope: 4.0,
            center: 10.0);

        // Assert
        function.Width.Should().Be(2.0);
        function.Slope.Should().Be(4.0);
        function.Center.Should().Be(10.0);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Width_Is_Zero()
    {
        // Arrange
        var action = () => new GeneralizedBellMembershipFunction(
            width: 0.0,
            slope: 2.0,
            center: 0.0);

        // Act & Assert
        action.Should()
              .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Membership_Should_Be_One_At_Center()
    {
        // Arrange
        var function = new GeneralizedBellMembershipFunction(
            width: 2.0,
            slope: 2.0,
            center: 5.0);

        // Act
        var membership = function.GetMembership(5.0);

        // Assert
        membership.Should().BeApproximately(1.0, 1e-12);
    }

    [Fact]
    public void Membership_Should_Be_Symmetric()
    {
        // Arrange
        var function = new GeneralizedBellMembershipFunction(
            width: 2.0,
            slope: 2.0,
            center: 5.0);

        // Act
        var left = function.GetMembership(3.0);
        var right = function.GetMembership(7.0);

        // Assert
        left.Should().BeApproximately(right, 1e-12);
    }

    [Fact]
    public void Membership_Should_Decrease_Away_From_Center()
    {
        // Arrange
        var function = new GeneralizedBellMembershipFunction(
            width: 2.0,
            slope: 2.0,
            center: 0.0);

        // Act
        var center = function.GetMembership(0.0);
        var near = function.GetMembership(1.0);
        var far = function.GetMembership(10.0);

        // Assert
        center.Should().BeGreaterThan(near);
        near.Should().BeGreaterThan(far);
    }

    [Fact]
    public void Membership_Should_Be_Close_To_Zero_For_Distant_Values()
    {
        // Arrange
        var function = new GeneralizedBellMembershipFunction(
            width: 1.0,
            slope: 4.0,
            center: 0.0);

        // Act
        var membership = function.GetMembership(100.0);

        // Assert
        membership.Should().BeLessThan(0.00000001);
    }

    [Fact]
    public void Membership_Should_Follow_The_Generalized_Bell_Equation()
    {
        // Arrange
        var function = new GeneralizedBellMembershipFunction(
            width: 2.0,
            slope: 2.0,
            center: 0.0);

        // Act
        var actual = function.GetMembership(2.0);

        // Expected:
        // 1 / (1 + |(2-0)/2|^(2*2))
        // = 1 / (1 + 1^4)
        // = 0.5
        const double expected = 0.5;

        // Assert
        actual.Should().BeApproximately(expected, 1e-12);
    }
}