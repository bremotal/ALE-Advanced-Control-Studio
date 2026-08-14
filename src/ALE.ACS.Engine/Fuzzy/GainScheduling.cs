namespace ALE.ACS.Engine.Fuzzy;

/// <summary>
/// Selects dynamic rule sets based on the current process state.
/// </summary>
public sealed class GainScheduling
{
    private readonly List<(Func<IReadOnlyDictionary<string, double>, bool> Condition, DynamicRules Rules)> _schedules = [];
    private readonly object _sync = new();

    /// <summary>
    /// Registers a scheduling condition and its associated rule set.
    /// </summary>
    /// <param name="condition">Condition evaluated over process inputs.</param>
    /// <param name="rules">Rule set selected when the condition is true.</param>
    public void RegisterSchedule(Func<IReadOnlyDictionary<string, double>, bool> condition, DynamicRules rules)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(rules);

        lock (_sync)
        {
            _schedules.Add((condition, rules));
        }
    }

    /// <summary>
    /// Selects the first matching dynamic rule set for the given process state.
    /// </summary>
    /// <param name="inputs">Current process state.</param>
    /// <returns>Matching dynamic rules.</returns>
    public DynamicRules SelectRules(IReadOnlyDictionary<string, double> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        lock (_sync)
        {
            foreach (var schedule in _schedules)
            {
                if (schedule.Condition(inputs))
                {
                    return schedule.Rules;
                }
            }
        }

        throw new InvalidOperationException("No schedule matched the current process state.");
    }
}
