namespace ALE.ACS.Engine.Fuzzy.Inference;

/// <summary>
/// Represents a Sugeno output function that can be constant, linear, or piecewise linear.
/// </summary>
public sealed class SugenoOutputFunction
{
    private readonly IReadOnlyList<SugenoOutputPoint> _points = Array.Empty<SugenoOutputPoint>();

    /// <summary>
    /// Initializes a constant Sugeno output function.
    /// </summary>
    public SugenoOutputFunction(SugenoOutputType type, double constantValue)
        : this(type, constantValue, 0.0, 0.0, [])
    {
    }

    /// <summary>
    /// Initializes a linear Sugeno output function.
    /// </summary>
    public SugenoOutputFunction(SugenoOutputType type, double a, double b)
        : this(type, b, a, 0.0, [])
    {
    }

    /// <summary>
    /// Initializes a piecewise linear Sugeno output function.
    /// </summary>
    public SugenoOutputFunction(SugenoOutputType type, IEnumerable<SugenoOutputPoint> points)
        : this(type, 0.0, 0.0, 0.0, points)
    {
    }

    private SugenoOutputFunction(SugenoOutputType type, double constantValue, double a, double b, IEnumerable<SugenoOutputPoint>? points)
    {
        if (type == SugenoOutputType.Constant)
        {
            ConstantValue = constantValue;
        }
        else if (type == SugenoOutputType.Linear)
        {
            A = a;
            B = constantValue;
        }
        else
        {
            _points = points?.ToList() ?? [];
        }

        Type = type;
    }

    /// <summary>
    /// Gets the output function type.
    /// </summary>
    public SugenoOutputType Type { get; }

    /// <summary>
    /// Gets the constant output value for constant functions.
    /// </summary>
    public double ConstantValue { get; }

    /// <summary>
    /// Gets the slope coefficient for linear functions.
    /// </summary>
    public double A { get; }

    /// <summary>
    /// Gets the intercept coefficient for linear functions.
    /// </summary>
    public double B { get; }

    /// <summary>
    /// Gets the interpolation points for piecewise linear functions.
    /// </summary>
    public IReadOnlyList<SugenoOutputPoint> Points => _points;

    /// <summary>
    /// Evaluates the function for the supplied input.
    /// </summary>
    public double Evaluate(double x)
    {
        return Type switch
        {
            SugenoOutputType.Constant => ConstantValue,
            SugenoOutputType.Linear => (A * x) + B,
            SugenoOutputType.PiecewiseLinear => EvaluatePiecewiseLinear(x),
            _ => throw new NotSupportedException()
        };
    }

    private double EvaluatePiecewiseLinear(double x)
    {
        if (_points.Count == 0)
        {
            return 0.0;
        }

        if (x <= _points[0].X)
        {
            return _points[0].Y;
        }

        for (var i = 1; i < _points.Count; i++)
        {
            var current = _points[i];
            var previous = _points[i - 1];

            if (x <= current.X)
            {
                if (Math.Abs(current.X - previous.X) < 1e-12)
                {
                    return current.Y;
                }

                var ratio = (x - previous.X) / (current.X - previous.X);
                return previous.Y + ((current.Y - previous.Y) * ratio);
            }
        }

        var lastPoint = _points[^1];
        var previousLastPoint = _points[^2];
        if (Math.Abs(lastPoint.X - previousLastPoint.X) < 1e-12)
        {
            return lastPoint.Y;
        }

        var extrapolationRatio = (x - previousLastPoint.X) / (lastPoint.X - previousLastPoint.X);
        return previousLastPoint.Y + ((lastPoint.Y - previousLastPoint.Y) * extrapolationRatio);
    }
}
