using ALE.ACS.Engine.Fuzzy.MembershipFunctions;

namespace ALE.ACS.Engine.Fuzzy.Defuzzification;

/// <summary>
/// Defines the contract for fuzzy defuzzifiers.
/// </summary>
public interface IDefuzzifier
{
    /// <summary>
    /// Gets the defuzzification method implemented by the instance.
    /// </summary>
    DefuzzificationMethod Method { get; }

    /// <summary>
    /// Defuzzifies a list of activated membership functions and their activation weights.
    /// </summary>
    /// <param name="activatedFunctions">Activated membership functions and their weights.</param>
    /// <param name="minValue">Lower bound of the output domain.</param>
    /// <param name="maxValue">Upper bound of the output domain.</param>
    /// <returns>The defuzzified crisp value.</returns>
    double Defuzzify(List<(IMembershipFunction Function, double Weight)> activatedFunctions, double minValue, double maxValue);
}
