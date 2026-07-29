namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Função de pertinência triangular.
/// </summary>
public sealed class TriangularMembershipFunction : MembershipFunctionBase
{
    public double A { get; }
    public double B { get; }
    public double C { get; }

    public TriangularMembershipFunction(
        string name,
        double a,
        double b,
        double c)
        : base(name)
    {
        if (!(a < b && b < c))
            throw new ArgumentException(
                "Os parâmetros devem obedecer à relação A < B < C.");

        A = a;
        B = b;
        C = c;
    }

    public override double GetMembership(double x)
    {
        if (x <= A || x >= C)
            return 0.0;

        if (x == B)
            return 1.0;

        if (x < B)
            return Clamp01((x - A) / (B - A));

        return Clamp01((C - x) / (C - B));
    }
}