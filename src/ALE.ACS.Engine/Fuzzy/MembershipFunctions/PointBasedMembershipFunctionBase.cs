using ALE.ACS.Engine.Fuzzy.Types;

namespace ALE.ACS.Engine.Fuzzy.MembershipFunctions;

/// <summary>
/// Base class for membership functions defined by an ordered
/// collection of points.
/// </summary>
public abstract class PointBasedMembershipFunctionBase
    : MembershipFunctionBase
{
    private readonly MembershipPoint[] _points;

    protected PointBasedMembershipFunctionBase(
        string name,
        IEnumerable<MembershipPoint> points)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(points);

        _points = points.ToArray();

        if (_points.Length < 2)
        {
            throw new ArgumentException(
                "At least two membership points are required.",
                nameof(points));
        }

        for (int i = 1; i < _points.Length; i++)
        {
            if (_points[i].X <= _points[i - 1].X)
            {
                throw new ArgumentException(
                    "Membership points must be ordered by X in ascending order.",
                    nameof(points));
            }
        }
    }

    /// <summary>
    /// Gets the membership points.
    /// </summary>
    public IReadOnlyList<MembershipPoint> Points => _points;

    /// <summary>
    /// Performs linear interpolation between the membership points.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <returns>Membership degree.</returns>
    protected double Interpolate(double x)
    {
        if (x <= _points[0].X)
        {
            return _points[0].Y;
        }

        if (x >= _points[^1].X)
        {
            return _points[^1].Y;
        }

        for (int i = 0; i < _points.Length - 1; i++)
        {
            MembershipPoint left = _points[i];
            MembershipPoint right = _points[i + 1];

            if (x >= left.X && x <= right.X)
            {
                double fraction =
                    (x - left.X) / (right.X - left.X);

                double value =
                    left.Y + ((right.Y - left.Y) * fraction);

                return Clamp01(value);
            }
        }

        return 0.0;
    }
}