using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Cascade;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.SmithPredictor;

/// <summary>
/// Spec para Smith Predictor: controlador PID primário + preditor que compensa
/// o tempo morto θ do processo FOPDT.
/// </summary>
public sealed class SmithSpec : IControllerSpec
{
    public required string Name { get; init; }
    public required ProcessVariable Setpoint { get; init; }
    public required ProcessVariable Manipulated { get; init; }
    public required ProcessVariable Controlled { get; init; }
    public IReadOnlyList<ProcessVariable> Disturbances { get; init; } = [];
    public required TimeSpan ScanTime { get; init; }

    /// <summary>Modelo FOPDT identificado do processo.</summary>
    public required FopdtModel Model { get; init; }

    // PID primário (atua sobre o erro entre SP e PV predita — sem atraso)
    public double Kp { get; init; } = 1.0;
    public double Ti { get; init; } = 10.0;
    public double Td { get; init; } = 0.0;
    public double N { get; init; } = 8.0;
    public bool DirectAction { get; init; } = false;
    public double OutMin { get; init; } = 0.0;
    public double OutMax { get; init; } = 100.0;

    public ControllerKind Kind => ControllerKind.SmithPredictor;

    public IControllerDesign Design()
    {
        var valid = Kp > 0 && Ti >= 0 && Td >= 0 && Model.Tau > 0 && Model.Theta >= 0;
        var summary = $"Smith Predictor: PID(Kp={Kp:F3}, Ti={Ti:F1}s) sobre PV sem atraso. " +
                       $"Modelo: {Model}. Dead-time samples = {Model.DeadTimeSamples}.";
        return new SmithDesign(valid, Model.DeadTimeSamples, Model.K, Model.Tau, Model.Theta, summary);
    }
}

public sealed record SmithDesign(
    bool IsValid,
    int DeadTimeSamples,
    double Gain,
    double Tau,
    double Theta,
    string Summary) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}

/// <summary>
/// Controlador Smith Predictor. Estrutura:
///   1. Mantém um buffer da MV passada (atraso de d amostras).
///   2. Prediz a PV sem atraso: y_pred(k) = a*y_pred(k-1) + b*u(k)  [modelo sem θ]
///   3. Compara SP com y_pred (não com a PV real) → controla sem sentir o atraso.
///   4. Correção: adiciona (PV_real - y_with_delay) ao SP para compensar erro de modelo.
/// </summary>
public sealed class SmithController : ControllerBase
{
    private readonly SmithSpec _spec;
    private readonly PidBlock _pid;
    private readonly double[] _mvHistory;   // buffer circular de MV (para aplicar atraso d)
    private int _mvIndex;
    private double _ypred;                  // estado da predição sem atraso
    private double _yWithDelay;             // estado da predição com atraso (para correção)
    private bool _initialized;

    public SmithController(SmithSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        _pid = new PidBlock($"{spec.Name}_PID", spec.Kp, spec.Ti, spec.Td)
        {
            N = spec.N,
            DirectAction = spec.DirectAction,
            OutMin = spec.OutMin,
            OutMax = spec.OutMax
        };
        var d = Math.Max(1, spec.Model.DeadTimeSamples);
        _mvHistory = new double[d];
        _mvIndex = 0;
    }

    public SmithSpec Spec => _spec;
    public PidBlock Pid => _pid;
    public double PredictedPv => _ypred;

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : 0.0;
        var pvReal = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;
        var scan = _spec.ScanTime.TotalSeconds;
        var (a, b, d) = _spec.Model.Discretize();

        // 1. Recupera MV atrasada do buffer
        var uDelayed = _mvHistory[_mvIndex];

        // 2. Predição sem atraso (y_pred) — usa MV atual, não a atrasada
        _ypred = a * _ypred + b * (context.Input.TryGet(_spec.Manipulated.Name, out var uCurrent) ? uCurrent : 0.0);
        if (!_initialized)
        {
            _ypred = pvReal;
            _yWithDelay = pvReal;
            _initialized = true;
        }

        // 3. Predição com atraso (y_with_delay) — propaga MV atrasada
        _yWithDelay = a * _yWithDelay + b * uDelayed;

        // 4. PV aparente = y_pred + (PV_real - y_with_delay)
        //    A correção (PV_real - y_with_delay) compensa erro de modelo e distúrbios
        var pvApparent = _ypred + (pvReal - _yWithDelay);

        // 5. PID atua sobre (SP - pvApparent) — sente o processo sem atraso
        var u = _pid.Step(sp, pvApparent, scan);

        // 6. Atualiza buffer de MV atrasada
        _mvHistory[_mvIndex] = u;
        _mvIndex = (_mvIndex + 1) % Math.Max(1, d);

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, u);
        outputs.Set("PredictedPv", _ypred);
        outputs.Set("PvApparent", pvApparent);

        return new ControllerResult(
            outputs,
            ControllerStatus.Success,
            [new ControllerMessage(ControllerMessageSeverity.Info, "SMITH_OK",
                $"SP={sp:F3} PV={pvReal:F3} PV_aparente={pvApparent:F3} PV_pred={_ypred:F3} MV={u:F3}",
                DateTime.UtcNow)],
            TimeSpan.Zero, true);
    }

    /// <summary>Reseta o preditor (útil para bumpless transfer de manual→auto).</summary>
    public void Reset(double pvInitial)
    {
        _ypred = pvInitial;
        _yWithDelay = pvInitial;
        _initialized = true;
        _pid.Reset();
        Array.Clear(_mvHistory, 0, _mvHistory.Length);
        _mvIndex = 0;
    }
}
