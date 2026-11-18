using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Cascade;

/// <summary>
/// Spec para um controlador PID de malha única.
/// </summary>
public sealed class PidSpec : IControllerSpec
{
    public string Name { get; }
    public ProcessVariable Setpoint { get; }
    public ProcessVariable Manipulated { get; }
    public ProcessVariable Controlled { get; }
    public IReadOnlyList<ProcessVariable> Disturbances { get; }
    public TimeSpan ScanTime { get; }

    public double Kp { get; init; } = 1.0;
    public double Ti { get; init; } = 10.0;  // segundos
    public double Td { get; init; } = 0.0;
    public double N { get; init; } = 8.0;
    public bool DirectAction { get; init; } = false;
    public double OutMin { get; init; } = 0.0;
    public double OutMax { get; init; } = 100.0;
    public string? InputFilterTimeConstant { get; init; }  // τ do filtro de PV, em segundos (string p/ YAML)

    public ControllerKind Kind => ControllerKind.Pid;

    public PidSpec(
        string name,
        ProcessVariable setpoint,
        ProcessVariable manipulated,
        ProcessVariable controlled,
        TimeSpan scanTime,
        IReadOnlyList<ProcessVariable>? disturbances = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Setpoint = setpoint ?? throw new ArgumentNullException(nameof(setpoint));
        Manipulated = manipulated ?? throw new ArgumentNullException(nameof(manipulated));
        Controlled = controlled ?? throw new ArgumentNullException(nameof(controlled));
        ScanTime = scanTime;
        Disturbances = disturbances ?? [];
    }

    public IControllerDesign Design()
    {
        var valid = Kp > 0 && Ti >= 0 && Td >= 0 && OutMax > OutMin;
        var summary = $"PID Kp={Kp:F4}, Ti={Ti:F2}s, Td={Td:F2}s, Out=[{OutMin:F1},{OutMax:F1}]";
        if (DirectAction) summary += " (Direct)";
        return new PidDesign(valid, Kp, Ti, Td, OutMin, OutMax, summary);
    }
}

public sealed record PidDesign(
    bool IsValid,
    double Kp,
    double Ti,
    double Td,
    double OutMin,
    double OutMax,
    string Summary) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}

/// <summary>
/// Controlador PID de malha única. Implementa <see cref="ControllerBase"/> para validação
/// e debug no .NET; em produção, roda no PLC via código ST gerado.
/// </summary>
public sealed class PidController : ControllerBase
{
    private readonly PidSpec _spec;
    private readonly PidBlock _block;

    public PidController(PidSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        _block = new PidBlock(spec.Name, spec.Kp, spec.Ti, spec.Td)
        {
            N = spec.N,
            DirectAction = spec.DirectAction,
            OutMin = spec.OutMin,
            OutMax = spec.OutMax
        };
    }

    public PidBlock Block => _block;
    public PidSpec Spec => _spec;

    protected override ControllerResult ExecuteCore(ControllerExecutionContext context)
    {
        var sp = context.Input.TryGet(_spec.Setpoint.Name, out var spVal) ? spVal : _spec.Setpoint.Nominal ?? 0.0;
        var pv = context.Input.TryGet(_spec.Controlled.Name, out var pvVal) ? pvVal : 0.0;

        var u = _block.Step(sp, pv, _spec.ScanTime.TotalSeconds);

        var outputs = new ControllerVariableCollection();
        outputs.Set(_spec.Manipulated.Name, u);

        return new ControllerResult(
            outputs,
            ControllerStatus.Success,
            [
                new ControllerMessage(ControllerMessageSeverity.Info, "PID_OK",
                    $"{_spec.Name}: SP={sp:F3} PV={pv:F3} MV={u:F3}", DateTime.UtcNow)
            ],
            TimeSpan.Zero,
            true);
    }
}
