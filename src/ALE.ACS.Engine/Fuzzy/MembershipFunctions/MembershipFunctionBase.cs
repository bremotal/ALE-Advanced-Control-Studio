namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Base class for all membership functions.
/// Provides common functionality and validation.
/// </summary>
public abstract class MembershipFunctionBase : IMembershipFunction
{
    protected const double Epsilon = 1e-12;

    protected MembershipFunctionBase(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public string Name { get; }

    public abstract double Evaluate(double x);

    protected static bool NearlyEqual(double a, double b)
    {
        return Math.Abs(a - b) <= Epsilon;
    }

    protected static double Clamp01(double value)
    {
        return Math.Clamp(value, 0.0, 1.0);
    }
}