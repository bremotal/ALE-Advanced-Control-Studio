using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Fuzzy.Rules;
using ALE.ACS.Engine.Fuzzy.Variables;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Fuzzy;

/// <summary>
/// Spec que adapta uma <see cref="FuzzyController"/> configurada para o contrato
/// <see cref="IControllerSpec"/>. Permite que o PLC generator despache por Kind
/// sem precisar de cast especializado.
/// </summary>
public sealed class FuzzySpec : IControllerSpec
{
    private readonly FuzzyController _controller;
    private readonly ProcessVariable _setpoint;
    private readonly ProcessVariable _manipulated;
    private readonly ProcessVariable _controlled;

    public FuzzySpec(
        FuzzyController controller,
        ProcessVariable setpoint,
        ProcessVariable manipulated,
        ProcessVariable controlled,
        TimeSpan scanTime,
        IReadOnlyList<ProcessVariable>? disturbances = null)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _setpoint = setpoint ?? throw new ArgumentNullException(nameof(setpoint));
        _manipulated = manipulated ?? throw new ArgumentNullException(nameof(manipulated));
        _controlled = controlled ?? throw new ArgumentNullException(nameof(controlled));
        ScanTime = scanTime;
        Disturbances = disturbances ?? [];
    }

    public ControllerKind Kind => ControllerKind.Fuzzy;
    public string Name => _controller.Name;
    public ProcessVariable Setpoint => _setpoint;
    public ProcessVariable Manipulated => _manipulated;
    public ProcessVariable Controlled => _controlled;
    public IReadOnlyList<ProcessVariable> Disturbances { get; }
    public TimeSpan ScanTime { get; }

    /// <summary>Variáveis fuzzy de entrada do controlador.</summary>
    public IReadOnlyList<FuzzyVariable> Inputs => _controller.Inputs;

    /// <summary>Variável fuzzy de saída do controlador.</summary>
    public FuzzyVariable Output => _controller.Output;

    /// <summary>Regras fuzzy.</summary>
    public IReadOnlyList<FuzzyRule> Rules => _controller.Rules;

    /// <summary>Tipo de inferência (Mamdani/Sugeno).</summary>
    public Fuzzy.Inference.InferenceEngineType EngineType => _controller.EngineType;

    public IControllerDesign Design()
    {
        var (min, max) = ComputeOutputRange(_controller.Output);
        return new FuzzyDesign(_controller.EngineType, _controller.Rules.Count, min, max);
    }

    /// <summary>
    /// Avalia o controlador fuzzy a partir de um <see cref="ControllerInput"/>.
    /// Delega para a <see cref="FuzzyController"/> interna já configurada.
    /// </summary>
    public double Evaluate(ControllerInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var dict = new Dictionary<string, double>();
        foreach (var v in input.Variables)
            dict[v.Name] = v.Value;
        return _controller.Evaluate(dict);
    }

    /// <summary>Referência à FuzzyController configurada (para uso pela factory).</summary>
    public FuzzyController InnerController => _controller;

    /// <summary>
    /// Calcula o intervalo [min, max] do universo de discurso da variável de
    /// saída, combinando os <see cref="IMembershipFunction.GetSupportBounds"/>
    /// de todos os termos. Usado pelo <see cref="FuzzyController"/> para passar
    /// o range real ao defuzzifier (em vez do [0,100] hard-coded antigo).
    /// </summary>
    internal static (double Min, double Max) ComputeOutputRange(FuzzyVariable output)
    {
        double min = double.PositiveInfinity, max = double.NegativeInfinity;
        bool found = false;
        foreach (var term in output.Terms)
        {
            var (tMin, tMax) = term.MembershipFunction.GetSupportBounds();
            if (!double.IsInfinity(tMin) && !double.IsInfinity(tMax))
            {
                min = Math.Min(min, tMin);
                max = Math.Max(max, tMax);
                found = true;
            }
        }
        if (!found) return (0.0, 100.0);
        if (min >= max) { var p = Math.Max(1.0, Math.Abs(min) * 0.01); return (min - p, max + p); }
        return (min, max);
    }
}

/// <summary>Design pré-computado de um controlador fuzzy.</summary>
public sealed record FuzzyDesign(
    Inference.InferenceEngineType EngineType,
    int RuleCount,
    double OutputMin,
    double OutputMax) : IControllerDesign
{
    public bool IsValid => RuleCount > 0;
    public string Summary =>
        $"Fuzzy ({EngineType}), {RuleCount} regras, output range [{OutputMin:F2}, {OutputMax:F2}]";
}
