using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Optimization;

namespace ALE.ACS.Engine.Linearization;

/// <summary>
/// Controlador que aplica a lei de linearização por realimentação:
///   u = (v - f(x)) / g(x)
/// onde v é a nova entrada linear (saída de PID externo sobre y - SP).
/// Em produção, esta lei vira um bloco Structured Text simples (comentado no generator).
/// </summary>
public sealed class FeedbackLinearizationController : ControllerBase
{
    private readonly LinearizationSpec _spec;
    private double[] _x;        // estado estimado
    private bool _initialized;

    public FeedbackLinearizationController(LinearizationSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        _x = spec.NominalState ?? Array.Empty<double>();
        if (_x.Length == 0) throw new ArgumentException("Estado nominal é obrigatório para feedback linearization.");
    }

    public LinearizationSpec Spec => _spec;
    public double[] EstimatedState => _x;

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : 0.0;
        var y = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;
        var v = context.Input.TryGet("V", out var vVal) ? vVal : sp;  // v default = SP

        // 1. Atualiza estado estimado (simplificação: assume x̂ ≈ [PV])
        // Em produção: observador (Kalman/Luenberger) a partir de (u, y).
        if (!_initialized)
        {
            _x = (double[])_x.Clone();
            if (_x.Length > 0) _x[0] = y;
            _initialized = true;
        }
        else if (_x.Length > 0)
        {
            _x[0] = y;  // estado observado = saída (caso SISO trivial)
        }

        // 2. Calcula f(x) e g(x)
        var fx = _spec.F(_x);
        var gx = _spec.G(_x);

        // 3. Lei de linearização: u = (v - f(x)) / g(x)
        // Para SISO: g é vetor coluna, tomamos o elemento que multiplica u em ẋ_1
        if (gx.Rows < 1 || gx.Cols < 1)
        {
            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [new ControllerMessage(ControllerMessageSeverity.Error, "LIN_G_EMPTY",
                    "g(x) deve ter ao menos 1 linha e 1 coluna.", DateTime.UtcNow)],
                TimeSpan.Zero, false);
        }

        var gEffective = gx[0, 0];  // elemento de maior influência
        if (Math.Abs(gEffective) < 1e-12)
        {
            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [new ControllerMessage(ControllerMessageSeverity.Error, "LIN_G_ZERO",
                    $"g(x) ≈ 0 em x=[{string.Join(",", _x.Select(v => v.ToString("F3")))}] — não linearizável aqui.", DateTime.UtcNow)],
                TimeSpan.Zero, false);
        }

        var uRaw = (_spec.InputGain * v - fx[0]) / gEffective;
        var u = Math.Max(_spec.OutMin, Math.Min(_spec.OutMax, uRaw));

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, u);

        return new ControllerResult(
            outputs,
            ControllerStatus.Success,
            [new ControllerMessage(ControllerMessageSeverity.Info, "LIN_OK",
                $"SP={sp:F3} PV={y:F3} v={v:F3} u={u:F3} (f={fx[0]:F3}, g={gEffective:F3})",
                DateTime.UtcNow)],
            TimeSpan.Zero, true);
    }
}

/// <summary>
/// Facade que expõe o serviço de linearização — usado pela camada Application
/// para despachar entre Feedback e Exact.
/// </summary>
public sealed class LinearizationService
{
    public ControllerBase Create(LinearizationSpec spec)
    {
        return spec.Mode switch
        {
            LinearizationMode.Feedback => new FeedbackLinearizationController(spec),
            LinearizationMode.Exact => new ExactLinearizationController(spec),
            _ => throw new ArgumentOutOfRangeException(nameof(spec.Mode))
        };
    }
}

/// <summary>
/// Linearização exata entrada-saída usando derivadas de Lie.
/// Para SISO afim em entrada: u = (v - Lf h(x)) / Lg h(x)
/// onde Lf h = ∂h/∂x · f(x) e Lg h = ∂h/∂x · g(x).
/// </summary>
public sealed class ExactLinearizationController : ControllerBase
{
    private readonly LinearizationSpec _spec;
    private double[] _x;
    private bool _initialized;

    public ExactLinearizationController(LinearizationSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        if (spec.DhDx is null)
            throw new ArgumentException("Linearização exata requer DhDx.", nameof(spec));
        _x = spec.NominalState ?? throw new ArgumentException("Estado nominal obrigatório para linearização exata.");
    }

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : 0.0;
        var y = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;
        var v = context.Input.TryGet("V", out var vVal) ? vVal : sp;

        if (!_initialized)
        {
            _x = (double[])_x.Clone();
            if (_x.Length > 0) _x[0] = y;
            _initialized = true;
        }
        else if (_x.Length > 0)
        {
            _x[0] = y;
        }

        var fx = _spec.F(_x);
        var gx = _spec.G(_x);
        var dh = _spec.DhDx!(_x);

        // Lf h = Dh · f
        double Lfh = 0;
        for (int i = 0; i < dh.Length && i < fx.Length; i++) Lfh += dh[i] * fx[i];

        // Lg h = Dh · g (assume SISO: g é vetor coluna)
        double Lgh = 0;
        for (int i = 0; i < dh.Length && i < gx.Rows; i++) Lgh += dh[i] * gx[i, 0];

        if (Math.Abs(Lgh) < 1e-12)
        {
            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [new ControllerMessage(ControllerMessageSeverity.Error, "LIN_LGH_ZERO",
                    $"Lg h ≈ 0 — sistema perde relatividade de grau em x=[{string.Join(",", _x.Select(v => v.ToString("F3")))}].",
                    DateTime.UtcNow)],
                TimeSpan.Zero, false);
        }

        var uRaw = (v - Lfh) / Lgh;
        var u = Math.Max(_spec.OutMin, Math.Min(_spec.OutMax, uRaw));

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, u);

        return new ControllerResult(
            outputs,
            ControllerStatus.Success,
            [new ControllerMessage(ControllerMessageSeverity.Info, "LIN_EXACT_OK",
                $"SP={sp:F3} PV={y:F3} v={v:F3} u={u:F3} (Lf h={Lfh:F3}, Lg h={Lgh:F3})",
                DateTime.UtcNow)],
            TimeSpan.Zero, true);
    }
}
