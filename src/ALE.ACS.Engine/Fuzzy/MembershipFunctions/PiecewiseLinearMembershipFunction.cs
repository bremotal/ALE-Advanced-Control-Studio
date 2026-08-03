using ALE.ACS.Engine.Fuzzy.Types;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a piecewise linear fuzzy membership function.
/// </summary>
/// <remarks>
/// A piecewise linear membership function is defined by an ordered
/// collection of membership points. Membership values are obtained
/// by linear interpolation between adjacent points.
/// </remarks>
public sealed class PiecewiseLinearMembershipFunction
    : PointBasedMembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PiecewiseLinearMembershipFunction"/> class.
    /// </summary>
    /// <param name="name">
    /// Membership function name.
    /// </param>
    /// <param name="points">
    /// Ordered collection of membership points.
    /// </param>
    public PiecewiseLinearMembershipFunction(
        string name,
        IEnumerable<MembershipPoint> points)
        : base(name, points)
    {
    }

    /// <inheritdoc />
    public override double Evaluate(double x)
    {
        return Interpolate(x);
    }
}