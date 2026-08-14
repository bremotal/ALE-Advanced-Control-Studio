namespace ALE.ACS.Engine.Fuzzy.Runtime;

/// <summary>
/// Represents the execution inputs for a fuzzy evaluation round.
/// </summary>
public ref struct FuzzyExecutionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyExecutionContext"/> struct.
    /// </summary>
    /// <param name="inputs">The input dictionary for this execution.</param>
    public FuzzyExecutionContext(IReadOnlyDictionary<string, double> inputs)
    {
        Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
    }

    /// <summary>
    /// Gets the current inputs.
    /// </summary>
    public IReadOnlyDictionary<string, double> Inputs { get; }
}
