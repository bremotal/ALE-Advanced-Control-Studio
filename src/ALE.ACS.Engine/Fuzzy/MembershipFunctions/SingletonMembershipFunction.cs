namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a singleton fuzzy membership function.
/// </summary>
/// <remarks>
/// <para>
/// A singleton membership function assumes full membership only at a
/// single point and zero membership everywhere else.
/// </para>
/// <para>
/// Mathematical definition:
/// </para>
/// <code>
/// μ(x) = 1, if x = c
/// μ(x) = 0, otherwise
/// </code>
/// <para>
/// Singleton membership functions are commonly used in:
/// </para>
/// <list type="bullet">
/// <item><description>Sugeno fuzzy systems.</description></item>
/// <item><description>Singleton fuzzification.</description></item>
/// <item><description>Rule consequents.</description></item>
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
public sealed class SingletonMembershipFunction : MembershipFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="SingletonMembershipFunction"/> class.
    /// </summary>
    /// <param name="position">
    /// Position of the singleton.
    /// </param>
    public SingletonMembershipFunction(double position)
        : base("Singleton")
    {
        Position = position;
    }

    /// <summary>
    /// Gets the singleton position.
    /// </summary>
    public double Position { get; }

    /// <inheritdoc />
    public override double Evaluate(double x)
    {
        return x.Equals(Position) ? 1.0 : 0.0;
    }
}