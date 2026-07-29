namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a classical Z-shaped fuzzy membership function (ZMF).
/// </summary>
/// <remarks>
/// <para>
/// The Z-shaped membership function provides a smooth transition from
/// full membership to zero membership over a finite interval.
/// It is the mirror image of the S-shaped membership function.
/// </para>
/// <para>
/// Mathematical definition:
/// </para>
/// <code>
///                1                              , x ≤ a
///
/// 1 - 2((x-a)/(b-a))²                         , a &lt; x ≤ (a+b)/2
///
///      2((x-b)/(b-a))²                         , (a+b)/2 &lt; x &lt; b
///
///                0                              , x ≥ b
/// </code>
/// <para>
/// This implementation is compatible with:
/// </para>
/// <list type="bullet">
/// <item><description>MATLAB Fuzzy Logic Toolbox (zmf)</description></item>
/// <item><description>scikit-fuzzy (zmf)</description></item>
/// </list>
/// <para>
/// References:
/// </para>
/// <list type="bullet">
/// <item>
/// Timothy J. Ross,
/// <i>Fuzzy Logic with Engineering Applications</i>,
/// 4th Edition.
/// </item>
/// <item>
/// Jang, Sun &amp; Mizutani,
/// <i>Neuro-Fuzzy and Soft Computing</i>.
/// </item>
/// </list>
/// </remarks>
public sealed class ZShapedMembershipFunction : MembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ZShapedMembershipFunction"/> class.
    /// </summary>
    /// <param name="left">
    /// Left transition point (a).
    /// </param>
    /// <param name="right">
    /// Right transition point (b).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="right"/> is less than or equal to
    /// <paramref name="left"/>.
    /// </exception>
    public ZShapedMembershipFunction(
        double left,
        double right)
        : base("Z-Shaped")
    {
        if (right <= left)
        {
            throw new ArgumentOutOfRangeException(
                nameof(right),
                "The right limit must be greater than the left limit.");
        }

        Left = left;
        Right = right;
    }

    /// <summary>
    /// Gets the left transition point.
    /// </summary>
    public double Left { get; }

    /// <summary>
    /// Gets the right transition point.
    /// </summary>
    public double Right { get; }

    /// <inheritdoc />
    public override double GetMembership(double x)
    {
        if (x <= Left)
        {
            return 1.0;
        }

        if (x >= Right)
        {
            return 0.0;
        }

        var midpoint = (Left + Right) / 2.0;
        var width = Right - Left;

        double membership;

        if (x <= midpoint)
        {
            var normalized = (x - Left) / width;
            membership = 1.0 - (2.0 * normalized * normalized);
        }
        else
        {
            var normalized = (x - Right) / width;
            membership = 2.0 * normalized * normalized;
        }

        return Clamp01(membership);
    }
}