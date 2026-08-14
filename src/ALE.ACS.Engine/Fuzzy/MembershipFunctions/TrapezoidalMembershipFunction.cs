namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a trapezoidal membership function defined by points a, b, c, and d.
/// </summary>
public sealed class TrapezoidalMembershipFunction : MembershipFunctionBase
{
    private readonly double _a;
    private readonly double _b;
    private readonly double _c;
    private readonly double _d;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrapezoidalMembershipFunction"/> class.
    /// </summary>
    /// <param name="a">Left foot point.</param>
    /// <param name="b">Left shoulder point.</param>
    /// <param name="c">Right shoulder point.</param>
    /// <param name="d">Right foot point.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <c>a &lt; b &lt;= c &lt; d</c> is not satisfied.
    /// </exception>
    public TrapezoidalMembershipFunction(double a, double b, double c, double d)
        : base("Trapezoidal")
    {
        if (!(a < b && b <= c && c < d))
        {
            throw new ArgumentException(
                "Invalid trapezoidal points. The relation a < b <= c < d must be satisfied.");
        }

        _a = a;
        _b = b;
        _c = c;
        _d = d;
    }

    /// <summary>
    /// Evaluates the trapezoidal membership degree for a given value.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <returns>The membership degree in the range [0, 1].</returns>
    public override double Evaluate(double x)
    {
        if (x < _a || x > _d)
            return 0.0;

        if (NearlyEqual(x, _a) || NearlyEqual(x, _d))
            return 0.0;

        if (x > _a && x < _b)
            return (x - _a) / (_b - _a);

        if ((x > _b || NearlyEqual(x, _b))
            && (x < _c || NearlyEqual(x, _c)))
        {
            return 1.0;
        }

        return (_d - x) / (_d - _c);
    }

    public override (double Min, double Max) GetSupportBounds()
    {
        return (_a, _d);
    }
}