using ALE.ACS.Engine.Fuzzy.Types;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a triangular membership function.
/// </summary>
public sealed class TriangularMembershipFunction
    : PointBasedMembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="TriangularMembershipFunction"/> class.
    /// </summary>
    /// <param name="name">
    /// Membership function name.
    /// </param>
    /// <param name="left">
    /// Left point.
    /// </param>
    /// <param name="center">
    /// Peak point.
    /// </param>
    /// <param name="right">
    /// Right point.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the parameters do not define a valid triangular
    /// membership function.
    /// </exception>
    public TriangularMembershipFunction(
        string name,
        double left,
        double center,
        double right)
        : base(
            name,
            [
                new MembershipPoint(left, 0.0),
                new MembershipPoint(center, 1.0),
                new MembershipPoint(right, 0.0)
            ])
    {
        if (left >= center)
        {
            throw new ArgumentException(
                "The left point must be less than the center point.",
                nameof(left));
        }

        if (center >= right)
        {
            throw new ArgumentException(
                "The center point must be less than the right point.",
                nameof(center));
        }
    }

    /// <inheritdoc />
    public override double GetMembership(double x)
    {
        return Interpolate(x);
    }
}