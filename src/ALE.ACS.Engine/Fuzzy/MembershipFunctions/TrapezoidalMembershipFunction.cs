namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Função de pertinência trapezoidal.
/// </summary>
public sealed class TrapezoidalMembershipFunction : MembershipFunctionBase
{
    public double A { get; }
    public double B { get; }
    public double C { get; }
    public double D { get; }

    public TrapezoidalMembershipFunction(
        string name,
        double a,
        double b,
        double c,
        double d)
        : base(name)
    {
        if (!(a < b && b <= c && c < d))
            throw new ArgumentException(
                "Os parâmetros devem obedecer à relação A < B <= C < D.");

        A = a;
        B = b;
        C = c;
        D = d;
    }

    public override double GetMembership(double x)
    {
        if (x <= A || x >= D)
            return 0.0;

        if (x >= B && x <= C)
            return 1.0;

        if (x < B)
            return Clamp01((x - A) / (B - A));

        return Clamp01((D - x) / (D - C));
    }
}