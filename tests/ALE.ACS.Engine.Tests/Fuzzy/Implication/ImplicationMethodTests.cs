using ALE.ACS.Engine.Fuzzy.Implication;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Implication;

public sealed class ImplicationMethodTests
{
    [Fact]
    public void MinimumImplication_Should_Return_Minimum_Value()
    {
        var implication = new MinimumImplication();
        implication.Implicate(0.8, 0.6).Should().Be(0.6);
    }

    [Fact]
    public void ProductImplication_Should_Return_Product_Value()
    {
        var implication = new ProductImplication();
        implication.Implicate(0.8, 0.6).Should().Be(0.48);
    }

    [Fact]
    public void LarsenImplication_Should_Return_Product_Value()
    {
        var implication = new LarsenImplication();
        implication.Implicate(0.8, 0.6).Should().Be(0.48);
    }
}
