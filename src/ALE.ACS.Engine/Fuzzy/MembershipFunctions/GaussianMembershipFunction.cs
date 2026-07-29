namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a Gaussian fuzzy membership function.
/// </summary>
public sealed class GaussianMembershipFunction : MembershipFunctionBase
{
    /// <summary>
    /// Gets the center (mean) of the Gaussian function.
    /// </summary>
    public double Mean { get; }

    /// <summary>
    /// Gets the standard deviation of the Gaussian function.
    /// </summary>
    public double Sigma { get; }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="GaussianMembershipFunction"/> class.
    /// </summary>
    /// <param name="name">
    /// Membership function name.
    /// </param>
    /// <param name="mean">
    /// Center (mean) of the Gaussian function.
    /// </param>
    /// <param name="sigma">
    /// Standard deviation. Must be greater than zero.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="sigma"/> is less than or equal to zero.
    /// </exception>
    public GaussianMembershipFunction(
        string name,
        double mean,
        double sigma)
        : base(name)
    {
        if (sigma <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sigma),
                "Sigma must be greater than zero.");
        }

        Mean = mean;
        Sigma = sigma;
    }

    /// <inheritdoc />
    public override double GetMembership(double x)
    {
        var delta = x - Mean;

        var exponent = -(delta * delta)
                     / (2.0 * Sigma * Sigma);

        return Clamp01(Math.Exp(exponent));
    }
}