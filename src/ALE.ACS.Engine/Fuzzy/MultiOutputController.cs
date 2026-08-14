namespace ALE.ACS.Engine.Fuzzy;

/// <summary>
/// Supports fuzzy controllers with multiple output channels.
/// </summary>
public sealed class MultiOutputController
{
    private readonly List<OutputChannel> _channels;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiOutputController"/> class.
    /// </summary>
    /// <param name="channels">Configured output channels.</param>
    public MultiOutputController(IEnumerable<OutputChannel> channels)
    {
        ArgumentNullException.ThrowIfNull(channels);

        _channels = [.. channels.Where(static channel => channel is not null)];

        if (_channels.Count == 0)
        {
            throw new ArgumentException("At least one output channel is required.", nameof(channels));
        }
    }

    /// <summary>
    /// Evaluates all configured output channels for the given inputs.
    /// </summary>
    /// <param name="inputs">Input process values.</param>
    /// <returns>Calculated outputs by channel name.</returns>
    public IReadOnlyDictionary<string, double> Evaluate(IReadOnlyDictionary<string, double> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var channel in _channels)
        {
            result[channel.Name] = channel.Evaluate(inputs);
        }

        return result;
    }
}

/// <summary>
/// Represents one output channel in a multi-output fuzzy controller.
/// </summary>
/// <param name="Name">Output channel name.</param>
/// <param name="Evaluator">Evaluation delegate for the channel.</param>
public sealed record OutputChannel(string Name, Func<IReadOnlyDictionary<string, double>, double> Evaluator)
{
    /// <summary>
    /// Evaluates this output channel.
    /// </summary>
    /// <param name="inputs">Input process values.</param>
    /// <returns>Output value.</returns>
    public double Evaluate(IReadOnlyDictionary<string, double> inputs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentNullException.ThrowIfNull(Evaluator);
        ArgumentNullException.ThrowIfNull(inputs);

        return Evaluator(inputs);
    }
}
