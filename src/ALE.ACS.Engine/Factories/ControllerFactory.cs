using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.AdaptiveMpc;
using ALE.ACS.Engine.Cascade;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Linearization;
using ALE.ACS.Engine.Mpc;
using ALE.ACS.Engine.Optimization;
using ALE.ACS.Engine.SmithPredictor;

namespace ALE.ACS.Engine.Factories;

/// <summary>
/// Implementação concreta de <see cref="IControllerFactory"/> que despacha por string.
/// Conecta o contrato existente no repo com os engines deste pacote.
/// </summary>
/// <remarks>
/// A interface <c>IControllerFactory</c> no repo só tem dois métodos:
///   <c>IController Create(string controllerType)</c> e
///   <c>bool CanCreate(string controllerType)</c>.
/// Esta factory aceita essas chamadas, mas como um controlador precisa de uma
/// <c>IControllerSpec</c> para ser instanciado, a versão por string requer que
/// a spec já tenha sido registrada (via <see cref="RegisterSpec"/>).
/// </remarks>
public sealed class ControllerFactory : IControllerFactory
{
    private readonly Dictionary<string, IControllerSpec> _specsByStringKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<IControllerSpec, IController>> _buildersByKind = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PID"] = spec => new PidController((PidSpec)spec),
        ["MPC"] = spec => new MpcController((MpcSpec)spec),
        ["ADAPTIVEMPC"] = spec => new AdaptiveMpcController((AdaptiveMpcSpec)spec),
        ["CASCADE"] = spec => new CascadeController((CascadeSpec)spec),
        ["SMITH"] = spec => new SmithController((SmithSpec)spec),
        ["SMITHPREDICTOR"] = spec => new SmithController((SmithSpec)spec),
        ["LINEARIZATION"] = spec => new LinearizationService().Create((LinearizationSpec)spec),
        ["FEEDBACKLINEARIZATION"] = spec => new LinearizationService().Create((LinearizationSpec)spec),
        ["OPTIMIZATION"] = spec => new OptimizationController((OptimizationSpec)spec),
        // Fuzzy é tratado à parte: FuzzyController não herda de ControllerBase
        // (facade próprio no repo). Delegamos para um adaptador.
        ["FUZZY"] = spec => new FuzzyControllerAdapter((Fuzzy.FuzzySpec)spec)
    };

    /// <summary>Registra uma spec nomeada, que depois poderá ser instanciada por string.</summary>
    public void RegisterSpec(string key, IControllerSpec spec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(spec);
        _specsByStringKey[key] = spec;
    }

    public bool CanCreate(string controllerType)
        => !string.IsNullOrWhiteSpace(controllerType)
           && (_buildersByKind.ContainsKey(controllerType) || _specsByStringKey.ContainsKey(controllerType));

    public IController Create(string controllerType)
    {
        if (string.IsNullOrWhiteSpace(controllerType))
            throw new ArgumentException("Tipo de controlador não pode ser vazio.", nameof(controllerType));

        // Se houver uma spec registrada com essa chave, usa a spec
        if (_specsByStringKey.TryGetValue(controllerType, out var spec))
        {
            return CreateFromSpec(spec);
        }

        // Senão, tenta pelo tipo (sem spec — apenas para tipos que aceitam defaults; nenhum aceita por enquanto)
        throw new InvalidOperationException(
            $"Não há spec registrada para '{controllerType}'. " +
            "Use RegisterSpec(key, spec) antes de Create(key), ou Create(spec) direto.");
    }

    /// <summary>Cria um controlador a partir de uma spec tipada.</summary>
    public IController Create(IControllerSpec spec)
        => CreateFromSpec(spec);

    private IController CreateFromSpec(IControllerSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        var key = spec.Kind.ToString().ToUpperInvariant();
        if (!_buildersByKind.TryGetValue(key, out var builder))
            throw new InvalidOperationException($"Sem builder para ControllerKind '{spec.Kind}'.");
        return builder(spec);
    }
}

/// <summary>
/// Adaptador que faz <c>FuzzyController</c> (facade existente no repo, não herda ControllerBase)
/// conformar a <see cref="IController"/> para uso uniforme na factory.
/// Reusa a instância de <see cref="Fuzzy.FuzzyController"/> já configurada (não recria).
/// </summary>
internal sealed class FuzzyControllerAdapter : IController, IControllerDiagnostics
{
    private readonly Fuzzy.FuzzySpec _spec;

    public FuzzyControllerAdapter(Fuzzy.FuzzySpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    public ControllerResult Execute(ControllerInput input)
    {
        var dict = new Dictionary<string, double>();
        foreach (var v in input.Variables)
            dict[v.Name] = v.Value;

        try
        {
            // A FuzzyController real já está configurada dentro da FuzzySpec.
            // Reusamos a instância — não recriamos, pois isso exigiria conhecer o engine.
            var output = _spec.Evaluate(input);
            var outs = new ControllerVariableCollection();
            outs.Set(_spec.Manipulated.Name, output);
            return new ControllerResult(outs, ControllerStatus.Success,
                [new ControllerMessage(ControllerMessageSeverity.Info, "FUZZY_OK",
                    $"{_spec.Name}: out={output:F4}", DateTime.UtcNow)],
                TimeSpan.Zero, true);
        }
        catch (Exception ex)
        {
            return new ControllerResult(new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [new ControllerMessage(ControllerMessageSeverity.Error, "FUZZY_FAIL", ex.Message, DateTime.UtcNow)],
                TimeSpan.Zero, false);
        }
    }

    public ControllerExecutionMetrics LastExecutionMetrics
        => new(DateTime.MinValue, DateTime.MinValue);
    public IReadOnlyList<ControllerMessage> LastMessages => [];
}
