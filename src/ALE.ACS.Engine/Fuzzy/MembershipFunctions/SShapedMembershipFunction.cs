namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a classical S-shaped fuzzy membership function (SMF).
/// </summary>
/// <remarks>
/// <para>
/// The S-shaped membership function provides a smooth transition from
/// zero membership to full membership over a finite interval.
/// </para>
/// <para>
/// Mathematical definition:
/// </para>
/// <code>
///                0                              , x ≤ a
///
///      2((x-a)/(b-a))²                         , a &lt; x ≤ (a+b)/2
///
/// 1 - 2((x-b)/(b-a))²                         , (a+b)/2 &lt; x &lt; b
///
///                1                              , x ≥ b
/// </code>
/// <para>
/// This implementation is compatible with:
/// </para>
/// <list type="bullet">
/// <item><description>MATLAB Fuzzy Logic Toolbox (smf)</description></item>
/// <item><description>scikit-fuzzy (smf)</description></item>
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
/// Jang, Sun & Mizutani,
/// <i>Neuro-Fuzzy and Soft Computing</i>.
/// </item>
/// </list>
/// </remarks>
public sealed class SShapedMembershipFunction : MembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SShapedMembershipFunction"/> class.
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
    public SShapedMembershipFunction(
        double left,
        double right)
        : base("S-Shaped")
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
    public override double Evaluate(double x)
    {
        if (x <= Left)
        {
            return 0.0;
        }

        if (x >= Right)
        {
            return 1.0;
        }

        var midpoint = (Left + Right) / 2.0;
        var width = Right - Left;

        double membership;

        if (x <= midpoint)
        {
            var normalized = (x - Left) / width;
            membership = 2.0 * normalized * normalized;
        }
        else
        {
            var normalized = (x - Right) / width;
            membership = 1.0 - (2.0 * normalized * normalized);
        }

        return Clamp01(membership);
    }
}