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
    /// <summary>
    /// Initializes a new instance of the <see cref="SigmoidMembershipFunction"/> class.
    /// </summary>
    /// <param name="slope">
    /// Controls the slope of the sigmoid (a).
    /// Positive values create an increasing curve;
    /// negative values create a decreasing curve.
    /// </param>
    /// <param name="center">
    /// Center of the sigmoid (c).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when slope is zero.
    /// </exception>
    public SigmoidMembershipFunction(
        double slope,
        double center)
        : base("Sigmoid")
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

    /// <summary>
    /// Gets the slope parameter (a).
    /// </summary>
    public double Slope { get; }

    /// <summary>
    /// Gets the center parameter (c).
    /// </summary>
    public double Center { get; }

    /// <inheritdoc />
    public override double GetMembership(double x)
    {
        double exponent = -Slope * (x - Center);
        double value = 1.0 / (1.0 + Math.Exp(exponent));

        return Clamp01(value);
    }
}