using System.Collections.Concurrent;

namespace ALE.ACS.Engine.Fuzzy.Performance;

/// <summary>
/// Cache for compiled rule activation values.
/// </summary>
public sealed class RuleCache
{
    private readonly ConcurrentDictionary<string, double> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Attempts to get a cached rule activation.
    /// </summary>
    /// <param name="ruleKey">The unique rule key.</param>
    /// <param name="value">The cached activation value when found.</param>
    /// <returns><c>true</c> when found; otherwise <c>false</c>.</returns>
    public bool TryGet(string ruleKey, out double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleKey);
        return _cache.TryGetValue(ruleKey, out value);
    }

    /// <summary>
    /// Stores a rule activation value.
    /// </summary>
    /// <param name="ruleKey">The unique rule key.</param>
    /// <param name="value">The rule activation value.</param>
    public void Set(string ruleKey, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleKey);
        _cache[ruleKey] = value;
    }
}
