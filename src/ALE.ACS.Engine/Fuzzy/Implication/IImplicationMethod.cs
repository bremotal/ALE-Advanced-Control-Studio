namespace ALE.ACS.Engine.Fuzzy.Implication;

/// <summary>
/// Defines a method for applying implication to a fuzzy consequent.
/// </summary>
public interface IImplicationMethod
{
    /// <summary>
    /// Implicates a consequent degree using the activation degree.
    /// </summary>
    /// <param name="degree">The activation degree.</param>
    /// <param name="value">The consequent value.</param>
    /// <returns>The implied degree.</returns>
    double Implicate(double degree, double value);
}

/// <summary>
/// Applies the minimum implication operator.
/// </summary>
public sealed class MinimumImplication : IImplicationMethod
{
    public double Implicate(double degree, double value) => Math.Min(degree, value);
}

/// <summary>
/// Applies the product implication operator.
/// </summary>
public sealed class ProductImplication : IImplicationMethod
{
    public double Implicate(double degree, double value) => degree * value;
}

/// <summary>
/// Applies the Larsen implication operator.
/// </summary>
public sealed class LarsenImplication : IImplicationMethod
{
    public double Implicate(double degree, double value) => degree * value;
}
