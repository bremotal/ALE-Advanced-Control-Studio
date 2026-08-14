using ALE.ACS.Engine.Fuzzy.Aggregation;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Aggregation;

public sealed class AggregationMethodTests
{
    [Fact]
    public void MaximumAggregation_Should_Return_Maximum_Value()
    {
        var aggregation = new MaximumAggregation();
        aggregation.Aggregate(0.3, 0.7).Should().Be(0.7);
    }

    [Fact]
    public void SumAggregation_Should_Return_Sum_Bounded_To_One()
    {
        var aggregation = new SumAggregation();
        aggregation.Aggregate(0.6, 0.7).Should().Be(1.0);
    }

    [Fact]
    public void BoundedSumAggregation_Should_Return_Bounded_Sum()
    {
        var aggregation = new BoundedSumAggregation();
        aggregation.Aggregate(0.6, 0.7).Should().Be(1.0);
    }

    [Fact]
    public void ProbabilisticSumAggregation_Should_Return_Probabilistic_Sum()
    {
        var aggregation = new ProbabilisticSumAggregation();
        aggregation.Aggregate(0.6, 0.7).Should().Be(0.88);
    }
}
