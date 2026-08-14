namespace ALE.ACS.Engine.Fuzzy.Defuzzification;

/// <summary>
/// Identifies the defuzzification method to apply.
/// </summary>
public enum DefuzzificationMethod
{
    /// <summary>
    /// Centroid-based defuzzification.
    /// </summary>
    Centroid,

    /// <summary>
    /// Bisector-based defuzzification.
    /// </summary>
    Bisector,

    /// <summary>
    /// Mean of maximum defuzzification.
    /// </summary>
    MeanOfMaximum,

    /// <summary>
    /// Left maximum defuzzification.
    /// </summary>
    LeftMaximum,

    /// <summary>
    /// Right maximum defuzzification.
    /// </summary>
    RightMaximum
}
