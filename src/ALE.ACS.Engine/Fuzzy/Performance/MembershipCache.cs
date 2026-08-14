using System.Collections.Concurrent;

namespace ALE.ACS.Engine.Fuzzy.Performance;

/// <summary>
/// Cache for discretized membership values.
/// </summary>
public sealed class MembershipCache
{
    private readonly ConcurrentDictionary<string, double> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _resolution;

    /// <summary>
    /// Initializes a new instance of the <see cref="MembershipCache"/> class.
    /// </summary>
    /// <param name="resolution">Discretization resolution used to build cache keys.</param>
    public MembershipCache(int resolution = 1000)
    {
        if (resolution <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(resolution));
        }

        _resolution = resolution;
    }

    /// <summary>
    /// Attempts to get a cached membership value.
    /// </summary>
    /// <param name="termKey">The term cache key.</param>
    /// <param name="input">The input value.</param>
    /// <param name="value">The cached value when found.</param>
    /// <returns><c>true</c> when a cached value is found; otherwise <c>false</c>.</returns>
    public bool TryGet(string termKey, double input, out double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(termKey);
        return _cache.TryGetValue(BuildKey(termKey, input), out value);
    }

    /// <summary>
    /// Stores a membership value in the cache.
    /// </summary>
    /// <param name="termKey">The term cache key.</param>
    /// <param name="input">The input value.</param>
    /// <param name="value">The membership value.</param>
    public void Set(string termKey, double input, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(termKey);
        _cache[BuildKey(termKey, input)] = value;
    }

    private string BuildKey(string termKey, double input)
    {
        var discretized = (int)Math.Round(input * _resolution, MidpointRounding.AwayFromZero);
        return $"{termKey}:{discretized}";
    }
}
