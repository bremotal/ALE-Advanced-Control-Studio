namespace ALE.ACS.Engine.Fuzzy.Diagnostics;

/// <summary>
/// Represents the complete validation report for a fuzzy knowledge base.
/// </summary>
public sealed class ValidationReport
{
    private readonly List<ValidationResult> _results = [];

    /// <summary>
    /// Gets the collected validation results.
    /// </summary>
    public IReadOnlyList<ValidationResult> Results => _results;

    /// <summary>
    /// Adds a validation result to the report.
    /// </summary>
    /// <param name="result">The result to add.</param>
    public void Add(ValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        _results.Add(result);
    }
}
