namespace ALE.ACS.Engine.Fuzzy.Composition;

/// <summary>
/// Defines a method for composing activated consequent values.
/// </summary>
public interface ICompositionMethod
{
    /// <summary>
    /// Composes two fuzzy degrees.
    /// </summary>
    /// <param name="a">The first degree.</param>
    /// <param name="b">The second degree.</param>
    /// <returns>The composed degree.</returns>
    double Compose(double a, double b);
}

/// <summary>
/// Composes values using the max-min operator.
/// </summary>
public sealed class MaxMinComposition : ICompositionMethod
{
    public double Compose(double a, double b) => Math.Min(a, b);
}

/// <summary>
/// Composes values using the max-product operator.
/// </summary>
public sealed class MaxProductComposition : ICompositionMethod
{
    public double Compose(double a, double b) => a * b;
}
