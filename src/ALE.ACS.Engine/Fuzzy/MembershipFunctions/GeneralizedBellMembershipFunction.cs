namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a generalized bell-shaped membership function.
/// Formula:
/// μ(x) = 1 / (1 + |(x - c) / a|^(2b))
/// Compatible with MATLAB gbellmf and scikit-fuzzy.
/// </summary>
public sealed class GeneralizedBellMembershipFunction : MembershipFunctionBase
{
    public double Width { get; }

    public double Slope { get; }

    public double Center { get; }

    public GeneralizedBellMembershipFunction(
        string name,
        double width,
        double slope,
        double center)
        : base(name)
    {
        if (width <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "Width must be greater than zero.");
        }

        if (slope <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(slope),
                "Slope must be greater than zero.");
        }

        Width = width;
        Slope = slope;
        Center = center;
    }

    public override double Evaluate(double x)
    {
        var ratio = Math.Abs((x - Center) / Width);

        var exponent = Math.Pow(ratio, 2.0 * Slope);

        return Clamp01(
            1.0 / (1.0 + exponent));
    }
}