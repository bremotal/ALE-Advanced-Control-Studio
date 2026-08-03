namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a sigmoid membership function.
///
/// Formula:
/// μ(x) = 1 / (1 + e^(-a(x-c)))
///
/// Compatible with MATLAB sigmf and scikit-fuzzy.
/// </summary>
public sealed class SigmoidMembershipFunction : MembershipFunctionBase
{
    public double Slope { get; }

    public double Center { get; }

    public SigmoidMembershipFunction(
        string name,
        double slope,
        double center)
        : base(name)
    {
        if (slope == 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slope),
                "Slope must be different from zero.");
        }

        Slope = slope;
        Center = center;
    }

    public override double Evaluate(double x)
    {
        var exponent = -Slope * (x - Center);

        var value = 1.0 /
            (1.0 + Math.Exp(exponent));

        return Clamp01(value);
    }
}