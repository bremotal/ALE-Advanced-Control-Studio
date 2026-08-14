namespace ALE.ACS.Engine.Fuzzy.Variables;

using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Represents a linguistic term for a fuzzy variable.
/// </summary>
public sealed class FuzzyTerm
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyTerm"/> class.
    /// </summary>
    /// <param name="name">The linguistic term name.</param>
    /// <param name="membershipFunction">The membership function associated with the term.</param>
    public FuzzyTerm(string name, IMembershipFunction membershipFunction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        MembershipFunction = membershipFunction ?? throw new ArgumentNullException(nameof(membershipFunction));

        Name = name;
    }

    /// <summary>
    /// Gets the term name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the membership function used by the term.
    /// </summary>
    public IMembershipFunction MembershipFunction { get; }

    /// <summary>
    /// Evaluates the membership degree of the term for the supplied value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>The membership degree in the range [0, 1].</returns>
    public double Evaluate(double value)
        => MembershipFunction.Evaluate(value);
}
