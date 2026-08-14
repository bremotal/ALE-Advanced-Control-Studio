using ALE.ACS.Engine.Fuzzy.Activation;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Activation;

public sealed class ActivationMethodTests
{
    [Fact]
    public void MinimumActivation_Should_Return_Minimum_Value()
    {
        var activation = new MinimumActivation();
        activation.Activate(0.8, 0.6).Should().Be(0.6);
    }

    [Fact]
    public void ProductActivation_Should_Return_Product_Value()
    {
        var activation = new ProductActivation();
        activation.Activate(0.8, 0.6).Should().Be(0.48);
    }
}
