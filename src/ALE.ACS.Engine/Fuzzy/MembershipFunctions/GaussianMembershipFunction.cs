namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Função de pertinência Gaussiana.
/// </summary>
public sealed class GaussianMembershipFunction : MembershipFunctionBase
{
    public double Mean { get; }

    public double Sigma { get; }

    public GaussianMembershipFunction(
        string name,
        double mean,
        double sigma)
        : base(name)
    {
        if (sigma <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(sigma),
                "Sigma deve ser maior que zero.");

        Mean = mean;
        Sigma = sigma;
    }

    public override double GetMembership(double x)
    {
        var exponent = -Math.Pow(x - Mean, 2) / (2 * Sigma * Sigma);

        return Clamp01(Math.Exp(exponent));
    }
}