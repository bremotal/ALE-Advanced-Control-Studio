namespace ALE.ACS.Engine.Fuzzy.Runtime;

/// <summary>
/// Orchestrates fuzzy execution using a reusable evaluation context.
/// </summary>
public sealed class FuzzyRuntime
{
    private readonly EvaluationContext _evaluationContext = new();

    /// <summary>
    /// Executes the supplied compiled controller against the current inputs.
    /// </summary>
    /// <param name="controller">The compiled controller.</param>
    /// <param name="inputs">The input values.</param>
    /// <returns>The execution result.</returns>
    public double Execute(CompiledController controller, IReadOnlyDictionary<string, double> inputs)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(inputs);

        var executionContext = new FuzzyExecutionContext(inputs);
        _evaluationContext.SetCache("execution:inputs", inputs.Count);
        return controller.Execute(inputs);
    }
}
