namespace ALE.ACS.Engine.Fuzzy.Types;

/// <summary>
/// Represents a point of a fuzzy membership function.
/// </summary>
/// <remarks>
/// <para>
/// A membership point defines a coordinate (X,Y)
/// of a fuzzy membership function.
/// </para>
/// <para>
/// X may assume any finite value.
/// Y must always belong to the interval [0,1].
/// </para>
/// <para>
/// This type is intended to be reused by:
/// </para>
/// <list type="bullet">
/// <item><description>Piecewise linear membership functions.</description></item>
/// <item><description>Graphical editors.</description></item>
/// <item><description>MATLAB/FIS importers.</description></item>
/// <item><description>FCL import/export.</description></item>
/// <item><description>Serialization.</description></item>
/// </list>
/// </remarks>
public readonly record struct MembershipPoint
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="MembershipPoint"/> structure.
    /// </summary>
    /// <param name="x">
    /// X coordinate.
    /// </param>
    /// <param name="y">
    /// Membership value.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when one of the values is invalid.
    /// </exception>
    public MembershipPoint(double x, double y)
    {
        if (double.IsNaN(x))
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                "X cannot be NaN.");
        }

        if (double.IsInfinity(x))
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                "X cannot be infinite.");
        }

        if (double.IsNaN(y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(y),
                "Y cannot be NaN.");
        }

        if (double.IsInfinity(y))
        {
            throw new ArgumentOutOfRangeException(
                nameof(y),
                "Y cannot be infinite.");
        }

        if (y < 0.0 || y > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(y),
                "Y must be between 0.0 and 1.0.");
        }

        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the membership value.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Returns a string representation of the point.
    /// </summary>
    /// <returns>
    /// A string in the format "(X,Y)".
    /// </returns>
    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}