using ALE.ACS.Engine.Fuzzy.Types;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a trapezoidal membership function.
/// </summary>
public sealed class TrapezoidalMembershipFunction
    : PointBasedMembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="TrapezoidalMembershipFunction"/> class.
    /// </summary>
    /// <param name="name">
    /// Membership function name.
    /// </param>
    /// <param name="left">
    /// Left foot.
    /// </param>
    /// <param name="leftShoulder">
    /// Left shoulder.
    /// </param>
    /// <param name="rightShoulder">
    /// Right shoulder.
    /// </param>
    /// <param name="right">
    /// Right foot.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the parameters do not define a valid trapezoidal
    /// membership function.
    /// </exception>
    public TrapezoidalMembershipFunction(
        string name,
        double left,
        double leftShoulder,
        double rightShoulder,
        double right)
        : base(
            name,
            [
                new MembershipPoint(left, 0.0),
                new MembershipPoint(leftShoulder, 1.0),
                new MembershipPoint(rightShoulder, 1.0),
                new MembershipPoint(right, 0.0)
            ])
    {
        if (left >= leftShoulder)
        {
            throw new ArgumentException(
                "The left point must be less than the left shoulder.",
                nameof(left));
        }

        if (leftShoulder > rightShoulder)
        {
            throw new ArgumentException(
                "The left shoulder must be less than or equal to the right shoulder.",
                nameof(leftShoulder));
        }

        if (rightShoulder >= right)
        {
            throw new ArgumentException(
                "The right shoulder must be less than the right point.",
                nameof(rightShoulder));
        }
    }

    /// <inheritdoc />
    public override double GetMembership(double x)
    {
        return Interpolate(x);
    }
}