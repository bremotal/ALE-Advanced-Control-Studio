using ALE.ACS.Engine.Fuzzy.Performance;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Performance;

public sealed class PerformanceInfrastructureTests
{
    [Fact]
    public void ObjectPool_Should_Rent_And_Return_Instance()
    {
        var pool = new ObjectPool<List<int>>(() => new List<int>(), capacity: 4);

        var first = pool.Rent();
        first.Add(10);
        pool.Return(first, list => list.Clear());

        var second = pool.Rent();

        ReferenceEquals(first, second).Should().BeTrue();
        second.Should().BeEmpty();
    }

    [Fact]
    public void MembershipCache_Should_Hit_Cached_Value()
    {
        var cache = new MembershipCache(resolution: 1000);

        cache.Set("Temperature.High", 12.3456, 0.77);

        var hit = cache.TryGet("Temperature.High", 12.3456, out var value);

        hit.Should().BeTrue();
        value.Should().Be(0.77);
    }

    [Fact]
    public void RuleCache_Should_Hit_Cached_Value()
    {
        var cache = new RuleCache();

        cache.Set("Rule-1", 0.81);

        var hit = cache.TryGet("Rule-1", out var value);

        hit.Should().BeTrue();
        value.Should().Be(0.81);
    }

    [Fact]
    public void AllocationFreeEvaluation_Should_Compute_Min_Using_Stackalloc()
    {
        Span<double> values = stackalloc double[] { 0.8, 0.5, 0.9, 0.3 };

        var result = AllocationFreeEvaluation.Min(values);

        result.Should().Be(0.3);
    }

    [Fact]
    public void SimdMath_Should_Compute_Batch_Multiplication()
    {
        var left = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var right = new double[] { 2.0, 2.0, 2.0, 2.0, 2.0 };
        var destination = new double[left.Length];

        SimdMath.Multiply(left, right, destination);

        destination.Should().Equal(2.0, 4.0, 6.0, 8.0, 10.0);
    }
}
