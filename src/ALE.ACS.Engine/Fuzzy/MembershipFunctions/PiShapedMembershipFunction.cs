namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a classical Pi-shaped fuzzy membership function (PIMF).
/// </summary>
/// <remarks>
/// <para>
/// The Pi-shaped membership function is composed of:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// An S-shaped transition between <c>a</c> and <c>b</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// A plateau with full membership between <c>b</c> and <c>c</c>.
/// </description>
/// </item>
/// <item>
/// <description>
/// A Z-shaped transition between <c>c</c> and <c>d</c>.
/// </description>
/// </item>
/// </list>
/// <para>
/// This implementation follows the behavior of the MATLAB Fuzzy Logic
/// Toolbox (pimf) and scikit-fuzzy while reusing the existing
/// S-shaped and Z-shaped membership functions.
/// </para>
/// </remarks>
public sealed class PiShapedMembershipFunction : MembershipFunctionBase
{
    private readonly SShapedMembershipFunction _leftFunction;
    private readonly ZShapedMembershipFunction _rightFunction;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="PiShapedMembershipFunction"/> class.
    /// </summary>
    /// <param name="leftFoot">
    /// Parameter a.
    /// </param>
    /// <param name="leftShoulder">
    /// Parameter b.
    /// </param>
    /// <param name="rightShoulder">
    /// Parameter c.
    /// </param>
    /// <param name="rightFoot">
    /// Parameter d.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the parameters do not satisfy
    /// a &lt; b ≤ c &lt; d.
    /// </exception>
    public PiShapedMembershipFunction(
        double leftFoot,
        double leftShoulder,
        double rightShoulder,
        double rightFoot)
        : base("Pi-Shaped")
    {
        if (leftFoot >= leftShoulder)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leftShoulder),
                "leftShoulder must be greater than leftFoot.");
        }

        if (leftShoulder > rightShoulder)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rightShoulder),
                "rightShoulder must be greater than or equal to leftShoulder.");
        }

        if (rightShoulder >= rightFoot)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rightFoot),
                "rightFoot must be greater than rightShoulder.");
        }

        LeftFoot = leftFoot;
        LeftShoulder = leftShoulder;
        RightShoulder = rightShoulder;
        RightFoot = rightFoot;

        _leftFunction = new SShapedMembershipFunction(
            leftFoot,
            leftShoulder);

        _rightFunction = new ZShapedMembershipFunction(
            rightShoulder,
            rightFoot);
    }

    /// <summary>
    /// Gets parameter a.
    /// </summary>
    public double LeftFoot { get; }

    /// <summary>
    /// Gets parameter b.
    /// </summary>
    public double LeftShoulder { get; }

    /// <summary>
    /// Gets parameter c.
    /// </summary>
    public double RightShoulder { get; }

    /// <summary>
    /// Gets parameter d.
    /// </summary>
    public double RightFoot { get; }

    /// <inheritdoc />
    public override double Evaluate(double x)
    {
        if (x <= LeftFoot)
        {
            return 0.0;
        }

        if (x >= RightFoot)
        {
            return 0.0;
        }

        if (x < LeftShoulder)
        {
            return _leftFunction.Evaluate(x);
        }

        if (x <= RightShoulder)
        {
            return 1.0;
        }

        return _rightFunction.Evaluate(x);
    }
}