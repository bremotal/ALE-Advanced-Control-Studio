namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a triangular membership function defined by points a, b, and c.
/// </summary>
public sealed class TriangularMembershipFunction : MembershipFunctionBase
{
    private readonly double _a;
    private readonly double _b;
    private readonly double _c;

    /// <summary>
    /// Initializes a new instance of the <see cref="TriangularMembershipFunction"/> class.
    /// </summary>
    /// <param name="a">Left base point.</param>
    /// <param name="b">Peak point.</param>
    /// <param name="c">Right base point.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <c>a &lt; b &lt; c</c> is not satisfied.
    /// </exception>
    public TriangularMembershipFunction(double a, double b, double c)
        : base("Triangular")
    {
        if (!(a < b && b < c))
        {
            throw new ArgumentException(
                "Invalid triangular points. The relation a < b < c must be satisfied.");
        }

        _a = a;
        _b = b;
        _c = c;
    }

    /// <summary>
    /// Evaluates the triangular membership degree for a given value.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <returns>The membership degree in the range [0, 1].</returns>
    public override double Evaluate(double x)
    {
        if (x < _a || x > _c)
            return 0.0;

        if (NearlyEqual(x, _a) || NearlyEqual(x, _c))
            return 0.0;

        if (NearlyEqual(x, _b))
            return 1.0;

        if (x < _b)
            return (x - _a) / (_b - _a);

        return (_c - x) / (_c - _b);
    }
}