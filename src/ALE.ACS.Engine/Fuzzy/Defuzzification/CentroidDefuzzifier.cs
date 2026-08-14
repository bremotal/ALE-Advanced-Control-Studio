using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Fuzzy.Defuzzification;

/// <summary>
/// Computes a crisp value using centroid defuzzification over a discretized domain.
/// </summary>
public sealed class CentroidDefuzzifier : IDefuzzifier
{
    private const int DiscretizationSteps = 1000;

    /// <inheritdoc />
    public DefuzzificationMethod Method => DefuzzificationMethod.Centroid;

    /// <inheritdoc />
    public double Defuzzify(List<(IMembershipFunction Function, double Weight)> activatedFunctions, double minValue, double maxValue)
    {
        if (activatedFunctions is null || activatedFunctions.Count == 0)
        {
            return 0.0;
        }

        if (minValue > maxValue)
        {
            (minValue, maxValue) = (maxValue, minValue);
        }

        if (Math.Abs(maxValue - minValue) < 1e-12)
        {
            return minValue;
        }

        double numerator = 0.0;
        double denominator = 0.0;

        for (var i = 0; i <= DiscretizationSteps; i++)
        {
            var x = minValue + ((maxValue - minValue) * i / DiscretizationSteps);
            var y = EvaluateUnion(activatedFunctions, x);
            numerator += x * y;
            denominator += y;
        }

        return Math.Abs(denominator) < 1e-12 ? 0.0 : numerator / denominator;
    }

    private static double EvaluateUnion(List<(IMembershipFunction Function, double Weight)> activatedFunctions, double x)
    {
        double union = 0.0;

        foreach (var (function, weight) in activatedFunctions)
        {
            if (function is null)
            {
                continue;
            }

            var membership = function.Evaluate(x);
            var clipped = Math.Min(weight, membership);
            union = Math.Max(union, clipped);
        }

        return union;
    }
}
