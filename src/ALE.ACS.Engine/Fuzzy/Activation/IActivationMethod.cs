namespace ALE.ACS.Engine.Fuzzy.Activation;

/// <summary>
/// Defines a method for activating a consequent using the antecedent degree.
/// </summary>
public interface IActivationMethod
{
    /// <summary>
    /// Activates a consequent degree.
    /// </summary>
    /// <param name="degree">The rule firing degree.</param>
    /// <param name="value">The consequent value.</param>
    /// <returns>The activated degree.</returns>
    double Activate(double degree, double value);
}

/// <summary>
/// Applies the minimum activation operator.
/// </summary>
public sealed class MinimumActivation : IActivationMethod
{
    public double Activate(double degree, double value) => Math.Min(degree, value);
}

/// <summary>
/// Applies the product activation operator.
/// </summary>
public sealed class ProductActivation : IActivationMethod
{
    public double Activate(double degree, double value) => degree * value;
}
