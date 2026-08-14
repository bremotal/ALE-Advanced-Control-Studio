using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Fuzzy.Defuzzification;

/// <summary>
/// Computes a crisp value using mean of maximum defuzzification.
/// </summary>
public sealed class MeanOfMaximumDefuzzifier : IDefuzzifier
{
    private const int DiscretizationSteps = 1000;

    /// <inheritdoc />
    public DefuzzificationMethod Method => DefuzzificationMethod.MeanOfMaximum;

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

        double maxMembership = 0.0;
        double sumX = 0.0;
        int count = 0;

        for (var i = 0; i <= DiscretizationSteps; i++)
        {
            var x = minValue + ((maxValue - minValue) * i / DiscretizationSteps);
            var y = EvaluateUnion(activatedFunctions, x);

            if (y > maxMembership)
            {
                maxMembership = y;
                sumX = x;
                count = 1;
            }
            else if (Math.Abs(y - maxMembership) < 1e-12)
            {
                sumX += x;
                count++;
            }
        }

        return count == 0 ? 0.0 : sumX / count;
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
