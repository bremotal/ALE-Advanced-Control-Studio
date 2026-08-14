namespace ALE.ACS.Engine.Fuzzy.Variables;

/// <summary>
/// Represents a fuzzy input or output variable with linguistic terms.
/// </summary>
public sealed class FuzzyVariable
{
    private readonly List<FuzzyTerm> _terms = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FuzzyVariable"/> class.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public FuzzyVariable(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the linguistic terms attached to the variable.
    /// </summary>
    public IReadOnlyCollection<FuzzyTerm> Terms => _terms;

    /// <summary>
    /// Adds a linguistic term to the variable.
    /// </summary>
    /// <param name="term">The fuzzy term.</param>
    public void AddTerm(FuzzyTerm term)
    {
        ArgumentNullException.ThrowIfNull(term);
        _terms.Add(term);
    }
}
