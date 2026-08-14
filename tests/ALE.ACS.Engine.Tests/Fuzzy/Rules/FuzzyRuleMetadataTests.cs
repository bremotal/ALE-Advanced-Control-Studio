using ALE.ACS.Engine.Fuzzy.Expressions;
using ALE.ACS.Engine.Fuzzy.Rules;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Rules;

public sealed class FuzzyRuleMetadataTests
{
    [Fact]
    public void Metadata_Should_Store_Expression_And_Properties()
    {
        var expression = new OperatorNode("AND", [new ClauseNode("Temperature", "High"), new ClauseNode("Pressure", "Low")]);

        var metadata = new FuzzyRuleMetadata
        {
            Weight = 0.75,
            Priority = 3,
            IsEnabled = true,
            Group = "Safety",
            Expression = expression
        };

        metadata.Weight.Should().Be(0.75);
        metadata.Priority.Should().Be(3);
        metadata.IsEnabled.Should().BeTrue();
        metadata.Group.Should().Be("Safety");
        metadata.Expression.Should().BeSameAs(expression);
    }
}
