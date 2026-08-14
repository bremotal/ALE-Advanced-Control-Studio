namespace ALE.ACS.Engine.Core.Validation;

/// <summary>
/// Provides context for validation operations.
/// </summary>
public sealed class ValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationContext"/> class.
    /// </summary>
    /// <param name="services">The shared services.</param>
    public ValidationContext(IReadOnlyDictionary<string, object?>? services)
    {
        Services = services ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the shared services available during validation.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Services { get; }
}
