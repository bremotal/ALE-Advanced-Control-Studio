using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Cascade;

/// <summary>
/// Spec para controle cascata (master + slave). O master controla a PV primária
/// e produz o setpoint do slave; o slave controla a PV secundária (rápida)
/// e produz a MV para o atuador.
/// </summary>
public sealed class CascadeSpec : IControllerSpec
{
    public required string Name { get; init; }
    public required ProcessVariable Setpoint { get; init; }       // SP primário
    public required ProcessVariable Manipulated { get; init; }     // saída do slave (para atuador)
    public required ProcessVariable Controlled { get; init; }      // PV primário (lento)
    public IReadOnlyList<ProcessVariable> Disturbances { get; init; } = [];
    public required TimeSpan ScanTime { get; init; }

    /// <summary>PV secundária (rápida) — alvo do slave.</summary>
    public required ProcessVariable SecondaryControlled { get; init; }

    // Master (slow loop)
    public double MasterKp { get; init; } = 1.0;
    public double MasterTi { get; init; } = 30.0;
    public double MasterTd { get; init; } = 0.0;
    public double MasterN { get; init; } = 8.0;
    public double MasterOutMin { get; init; } = 0.0;   // = range do SecondaryControlled
    public double MasterOutMax { get; init; } = 100.0;
    public bool MasterDirectAction { get; init; } = false;

    // Slave (fast loop)
    public double SlaveKp { get; init; } = 2.0;
    public double SlaveTi { get; init; } = 5.0;
    public double SlaveTd { get; init; } = 0.0;
    public double SlaveN { get; init; } = 10.0;
    public double SlaveOutMin { get; init; } = 0.0;    // = range do Manipulated
    public double SlaveOutMax { get; init; } = 100.0;
    public bool SlaveDirectAction { get; init; } = false;

    public ControllerKind Kind => ControllerKind.Cascade;

    public IControllerDesign Design()
    {
        var valid = MasterKp > 0 && SlaveKp > 0
            && MasterOutMax > MasterOutMin && SlaveOutMax > SlaveOutMin;
        var summary = $"Cascade: Master(Kp={MasterKp:F3}, Ti={MasterTi:F1}s) → " +
                       $"Slave(Kp={SlaveKp:F3}, Ti={SlaveTi:F1}s). " +
                       $"SP_slave ∈ [{MasterOutMin:F1}, {MasterOutMax:F1}], MV ∈ [{SlaveOutMin:F1}, {SlaveOutMax:F1}].";
        return new CascadeDesign(valid, MasterOutMin, MasterOutMax, SlaveOutMin, SlaveOutMax, summary);
    }
}

public sealed record CascadeDesign(
    bool IsValid,
    double MasterOutMin,
    double MasterOutMax,
    double SlaveOutMin,
    double SlaveOutMax,
    string Summary) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}

/// <summary>
/// Controlador cascata com master + slave. Master computa SP do slave;
/// slave computa MV para o atuador. Anti-windup por back-calculation no master
/// quando o slave satura.
/// </summary>
public sealed class CascadeController : ControllerBase
{
    private readonly CascadeSpec _spec;
    private readonly PidBlock _master;
    private readonly PidBlock _slave;

    public CascadeController(CascadeSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        _master = new PidBlock($"{spec.Name}_Master", spec.MasterKp, spec.MasterTi, spec.MasterTd)
        {
            N = spec.MasterN,
            DirectAction = spec.MasterDirectAction,
            OutMin = spec.MasterOutMin,
            OutMax = spec.MasterOutMax
        };
        _slave = new PidBlock($"{spec.Name}_Slave", spec.SlaveKp, spec.SlaveTi, spec.SlaveTd)
        {
            N = spec.SlaveN,
            DirectAction = spec.SlaveDirectAction,
            OutMin = spec.SlaveOutMin,
            OutMax = spec.SlaveOutMax
        };
    }

    public PidBlock Master => _master;
    public PidBlock Slave => _slave;
    public CascadeSpec Spec => _spec;

    /// <summary>SP atual do slave (= saída do master).</summary>
    public double SlaveSetpoint { get; private set; }

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : 0.0;
        var pvPrimary = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;
        var pvSecondary = context.Input.TryGet(_spec.SecondaryControlled.Name, out var pv2Val) ? pv2Val : 0.0;
        var scan = _spec.ScanTime.TotalSeconds;

        // Master: SP_primary vs PV_primary → SP_secondary
        var spSlave = _master.Step(sp, pvPrimary, scan);
        SlaveSetpoint = spSlave;

        // Slave: SP_secondary vs PV_secondary → MV
        var mv = _slave.Step(spSlave, pvSecondary, scan);

        // Anti-windup no master: se o slave saturou, "freeze" a integral do master
        // (tracking: o master deve acompanhar o que o slave consegue)
        // Heurística simples: se MV escravo está saturado, ajusta integral do master
        if (mv <= _spec.SlaveOutMin + 1e-6 || mv >= _spec.SlaveOutMax - 1e-6)
        {
            _master.BumplessTransfer(spSlave, pvPrimary, sp, scan);
        }

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, mv);
        outputs.Set($"SP_{_spec.SecondaryControlled.Name}", spSlave);

        return new ControllerResult(
            outputs,
            ControllerStatus.Success,
            [new ControllerMessage(ControllerMessageSeverity.Info, "CASCADE_OK",
                $"SP={sp:F3} PV1={pvPrimary:F3} SP2={spSlave:F3} PV2={pvSecondary:F3} MV={mv:F3}",
                DateTime.UtcNow)],
            TimeSpan.Zero, true);
    }
}
