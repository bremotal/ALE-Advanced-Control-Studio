using System;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a generalized bell-shaped membership function.
/// Formula:
/// μ(x) = 1 / (1 + |(x - c) / a|^(2b))
/// Compatible with MATLAB gbellmf and scikit-fuzzy.
/// </summary>
public sealed class GeneralizedBellMembershipFunction : MembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralizedBellMembershipFunction"/> class.
    /// </summary>
    /// <param name="width">Controls the width of the bell (a ≠ 0).</param>
    /// <param name="slope">Controls the slope of the bell.</param>
    /// <param name="center">Center of the bell.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when width is zero.
    /// </exception>
    public GeneralizedBellMembershipFunction(
        double width,
        double slope,
        double center)
        : base("Generalized Bell")
    {
        if (width == 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                "Width must be different from zero.");
        }

        Width = width;
        Slope = slope;
        Center = center;
    }

    /// <summary>
    /// Gets the width parameter (a).
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the slope parameter (b).
    /// </summary>
    public double Slope { get; }

    /// <summary>
    /// Gets the center parameter (c).
    /// </summary>
    public double Center { get; }

    /// <inheritdoc />
    public override double GetMembership(double x)
    {
        double ratio = Math.Abs((x - Center) / Width);
        double exponent = Math.Pow(ratio, 2.0 * Slope);

        return 1.0 / (1.0 + exponent);
    }
}