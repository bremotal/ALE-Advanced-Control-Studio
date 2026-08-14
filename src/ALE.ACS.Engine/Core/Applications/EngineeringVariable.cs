namespace ALE.ACS.Engine.Core.Applications;

/// <summary>
/// Represents a variable in an engineering process.
/// </summary>
public sealed class EngineeringVariable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EngineeringVariable"/> class.
    /// </summary>
    /// <param name="id">The variable identifier.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="unit">The unit of the variable.</param>
    /// <param name="description">The variable description.</param>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    /// <param name="isRequired">Whether the variable is required.</param>
    public EngineeringVariable(Guid id, string name, string? unit, string? description, double minimum, double maximum, bool isRequired)
    {
        Id = id;
        Name = name;
        Unit = unit;
        Description = description;
        Minimum = minimum;
        Maximum = maximum;
        IsRequired = isRequired;
    }

    /// <summary>
    /// Gets the variable identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the variable name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the variable unit.
    /// </summary>
    public string? Unit { get; }

    /// <summary>
    /// Gets the variable description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Gets a value indicating whether the variable is required.
    /// </summary>
    public bool IsRequired { get; }
}
