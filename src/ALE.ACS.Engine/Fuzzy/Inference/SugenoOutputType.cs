namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// Defines the output function type used by the Sugeno engine.
/// </summary>
public enum SugenoOutputType
{
    /// <summary>
    /// Constant output value.
    /// </summary>
    Constant,

    /// <summary>
    /// Linear output function of the form a*x + b.
    /// </summary>
    Linear,

    /// <summary>
    /// Piecewise linear output function based on interpolation points.
    /// </summary>
    PiecewiseLinear
}
