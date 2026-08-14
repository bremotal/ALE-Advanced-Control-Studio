namespace ALE.ACS.Engine.Core.Applications;

/// <summary>
/// Represents a block in an engineering strategy.
/// </summary>
public sealed class EngineeringBlock
{
    private readonly List<Guid> _dependencies = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineeringBlock"/> class.
    /// </summary>
    /// <param name="id">The block identifier.</param>
    /// <param name="name">The block name.</param>
    /// <param name="description">The block description.</param>
    /// <param name="inputs">The input variables.</param>
    /// <param name="outputs">The output variables.</param>
    /// <param name="configuration">The block configuration values.</param>
    /// <param name="dependencies">The dependency identifiers.</param>
    public EngineeringBlock(
        Guid id,
        string name,
        string? description,
        IReadOnlyCollection<EngineeringVariable>? inputs,
        IReadOnlyCollection<EngineeringVariable>? outputs,
        IReadOnlyDictionary<string, string>? configuration,
        IReadOnlyCollection<Guid>? dependencies)
    {
        Id = id;
        Name = name;
        Description = description;
        Inputs = inputs ?? [];
        Outputs = outputs ?? [];
        Configuration = configuration ?? new Dictionary<string, string>();
        _dependencies.AddRange(dependencies ?? []);
    }

    /// <summary>
    /// Gets the block identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the block name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the input variables.
    /// </summary>
    public IReadOnlyCollection<EngineeringVariable> Inputs { get; }

    /// <summary>
    /// Gets the output variables.
    /// </summary>
    public IReadOnlyCollection<EngineeringVariable> Outputs { get; }

    /// <summary>
    /// Gets the configuration dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, string> Configuration { get; }

    /// <summary>
    /// Gets the dependency identifiers.
    /// </summary>
    public IReadOnlyCollection<Guid> Dependencies => _dependencies;

    /// <summary>
    /// Adds a dependency to the block.
    /// </summary>
    /// <param name="dependencyId">The dependency identifier.</param>
    public void AddDependency(Guid dependencyId)
    {
        if (!_dependencies.Contains(dependencyId))
        {
            _dependencies.Add(dependencyId);
        }
    }
}
