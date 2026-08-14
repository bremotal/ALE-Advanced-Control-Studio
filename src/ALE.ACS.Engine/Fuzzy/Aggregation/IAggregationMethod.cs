namespace ALE.ACS.Engine.Fuzzy.Aggregation;

/// <summary>
/// Defines a method for aggregating fuzzy consequent values.
/// </summary>
public interface IAggregationMethod
{
    /// <summary>
    /// Aggregates two fuzzy degrees.
    /// </summary>
    /// <param name="a">The first degree.</param>
    /// <param name="b">The second degree.</param>
    /// <returns>The aggregated degree.</returns>
    double Aggregate(double a, double b);
}

/// <summary>
/// Aggregates values using the maximum operator.
/// </summary>
public sealed class MaximumAggregation : IAggregationMethod
{
    public double Aggregate(double a, double b) => Math.Max(a, b);
}

/// <summary>
/// Aggregates values using the sum operator, capped at 1.0.
/// </summary>
public sealed class SumAggregation : IAggregationMethod
{
    public double Aggregate(double a, double b) => Math.Min(1.0, a + b);
}

/// <summary>
/// Aggregates values using the bounded sum operator.
/// </summary>
public sealed class BoundedSumAggregation : IAggregationMethod
{
    public double Aggregate(double a, double b) => Math.Min(1.0, a + b);
}

/// <summary>
/// Aggregates values using the probabilistic sum operator.
/// </summary>
public sealed class ProbabilisticSumAggregation : IAggregationMethod
{
    public double Aggregate(double a, double b) => Math.Round(a + b - (a * b), 12);
}
