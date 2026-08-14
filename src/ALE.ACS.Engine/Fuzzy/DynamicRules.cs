namespace ALE.ACS.Engine.Fuzzy;

/// <summary>
/// Holds a dynamic, thread-safe rule set that can be updated at runtime.
/// </summary>
public sealed class DynamicRules
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private List<string> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicRules"/> class.
    /// </summary>
    /// <param name="rules">Initial rule identifiers.</param>
    public DynamicRules(IEnumerable<string> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = [.. rules.Where(static rule => !string.IsNullOrWhiteSpace(rule))];

        if (_rules.Count == 0)
        {
            throw new ArgumentException("At least one rule is required.", nameof(rules));
        }
    }

    /// <summary>
    /// Replaces the current rule set at runtime.
    /// </summary>
    /// <param name="rules">New rule identifiers.</param>
    public void Update(IEnumerable<string> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var next = rules.Where(static rule => !string.IsNullOrWhiteSpace(rule)).ToList();
        if (next.Count == 0)
        {
            throw new ArgumentException("At least one rule is required.", nameof(rules));
        }

        _lock.EnterWriteLock();
        try
        {
            _rules = next;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets a snapshot of the current rule identifiers.
    /// </summary>
    /// <returns>Immutable snapshot of current rules.</returns>
    public IReadOnlyList<string> GetRules()
    {
        _lock.EnterReadLock();
        try
        {
            return _rules.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
