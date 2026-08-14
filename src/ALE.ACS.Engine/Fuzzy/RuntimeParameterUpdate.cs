using System.Collections.Concurrent;

namespace ALE.ACS.Engine.Fuzzy;

/// <summary>
/// Updates variable bounds at runtime.
/// </summary>
public sealed class RuntimeParameterUpdate
{
    private readonly ConcurrentDictionary<string, VariableBounds> _bounds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Updates bounds for the specified variable.
    /// </summary>
    /// <param name="variableName">Variable name.</param>
    /// <param name="min">Minimum bound.</param>
    /// <param name="max">Maximum bound.</param>
    public void UpdateBounds(string variableName, double min, double max)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variableName);

        if (min > max)
        {
            throw new ArgumentException("Minimum bound cannot be greater than maximum bound.", nameof(min));
        }

        _bounds[variableName] = new VariableBounds(min, max);
    }

    /// <summary>
    /// Tries to get current bounds for the specified variable.
    /// </summary>
    /// <param name="variableName">Variable name.</param>
    /// <param name="bounds">Current bounds when available.</param>
    /// <returns><c>true</c> when bounds are available; otherwise <c>false</c>.</returns>
    public bool TryGetBounds(string variableName, out VariableBounds bounds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variableName);
        return _bounds.TryGetValue(variableName, out bounds);
    }
}

/// <summary>
/// Represents variable bounds used in runtime updates.
/// </summary>
/// <param name="Min">Minimum bound.</param>
/// <param name="Max">Maximum bound.</param>
public readonly record struct VariableBounds(double Min, double Max);
