using ALE.ACS.Engine.Fuzzy.Composition;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Composition;

public sealed class CompositionMethodTests
{
    [Fact]
    public void MaxMinComposition_Should_Return_Maximum_Of_Minima()
    {
        var composition = new MaxMinComposition();
        composition.Compose(0.8, 0.6).Should().Be(0.6);
    }

    [Fact]
    public void MaxProductComposition_Should_Return_Maximum_Of_Products()
    {
        var composition = new MaxProductComposition();
        composition.Compose(0.8, 0.6).Should().Be(0.48);
    }
}
