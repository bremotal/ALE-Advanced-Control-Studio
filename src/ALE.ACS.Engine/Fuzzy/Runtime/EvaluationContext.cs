using System.Collections.Concurrent;

namespace ALE.ACS.Engine.Fuzzy.Runtime;

/// <summary>
/// Provides an execution-scoped cache for fuzzy evaluations.
/// </summary>
public sealed class EvaluationContext
{
    private readonly ConcurrentDictionary<string, double> _cache = new();

    /// <summary>
    /// Sets a cached value for the current evaluation.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The cached value.</param>
    public void SetCache(string key, double value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _cache[key] = value;
    }

    /// <summary>
    /// Gets a cached value for the current evaluation.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value or <c>0.0</c> when absent.</returns>
    public double GetCache(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _cache.TryGetValue(key, out var value) ? value : 0.0;
    }
}
