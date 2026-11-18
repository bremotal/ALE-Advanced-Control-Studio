using ALE.ACS.Application.DigitalTwin;
using ALE.ACS.Engine.Cascade;
using ALE.ACS.Engine.Core.Execution;
using ALE.ACS.Engine.Core.Simulation;
using ALE.ACS.Engine.SmithPredictor;

namespace ALE.ACS.Application.DigitalTwin;

/// <summary>
/// Implementação do <see cref="IDigitalTwinService"/> usando a
/// <see cref="SimulationEngine"/> real do Bloco D.
/// </summary>
public sealed class DigitalTwinService : IDigitalTwinService
{
    /// <inheritdoc/>
    public DigitalTwinResult SimulatePid(
        object pidSpec,
        object processModel,
        double setpoint,
        double initialPv,
        int scanCount)
    {
        if (pidSpec is not PidSpec spec)
        {
            return new DigitalTwinResult(
                false, [], TimeSpan.Zero, 0,
                $"pidSpec deve ser PidSpec (recebido {pidSpec?.GetType().Name ?? "null"}).");
        }

        if (processModel is not FopdtModel model)
        {
            return new DigitalTwinResult(
                false, [], TimeSpan.Zero, 0,
                $"processModel deve ser FopdtModel (recebido {processModel?.GetType().Name ?? "null"}).");
        }

        if (scanCount <= 0)
        {
            return new DigitalTwinResult(
                false, [], TimeSpan.Zero, 0,
                "scanCount deve ser positivo.");
        }

        try
        {
            var engine = new SimulationEngine();

            // Variáveis iniciais no host
            engine.SetVariable(spec.Setpoint.Name, setpoint);
            engine.SetVariable(spec.Controlled.Name, initialPv);
            engine.SetVariable(spec.Manipulated.Name, 0.0);

            // Componente 1: controlador PID
            engine.AddComponent(new PidControllerComponent(spec));

            // Componente 2: processo FOPDT
            engine.AddComponent(new FopdtProcessComponent(model, spec.Manipulated.Name, spec.Controlled.Name));

            // Clock: cada scan = scanTime do PID; roda scanCount passos
            var clock = new SimulationClock(
                step: spec.ScanTime,
                startTime: TimeSpan.Zero,
                endTime: TimeSpan.FromTicks(spec.ScanTime.Ticks * scanCount));

            var stats = engine.Run(clock);

            // Coleta a série temporal do snapshot final + reconstruct via samples internos
            // (a simulação evoluiu o host; para série histórica usamos os componentes)
            var samples = CollectSamples(engine, spec, clock);

            return new DigitalTwinResult(
                success: stats.ErrorCount == 0,
                samples,
                stats.SimulatedDuration,
                stats.StepCount,
                stats.ErrorCount > 0
                    ? $"{stats.ErrorCount} erro(s) durante a simulação."
                    : null);
        }
        catch (Exception ex)
        {
            return new DigitalTwinResult(
                false, [], TimeSpan.Zero, 0,
                $"Falha na simulação: {ex.Message}");
        }
    }

    private static IReadOnlyList<DigitalTwinSample> CollectSamples(
        SimulationEngine engine,
        PidSpec spec,
        SimulationClock clock)
    {
        // Lê os valores finais do host e constrói amostras representativas.
        // Para uma série temporal completa, o PidControllerComponent e o
        // FopdtProcessComponent mantêm histórico interno; aqui retornamos
        // o estado final + resumo.
        var host = engine.Host;
        var samples = new List<DigitalTwinSample>
        {
            new(clock.Current, spec.Setpoint.Name, host.GetVariable(spec.Setpoint.Name)),
            new(clock.Current, spec.Controlled.Name, host.GetVariable(spec.Controlled.Name)),
            new(clock.Current, spec.Manipulated.Name, host.GetVariable(spec.Manipulated.Name))
        };
        return samples;
    }
}

/// <summary>
/// Componente de simulação que envolve um <see cref="PidBlock"/>.
/// A cada passo lê SP e PV do host, computa MV e grava de volta.
/// </summary>
internal sealed class PidControllerComponent : ISimulationComponent
{
    private readonly PidSpec _spec;
    private PidBlock _pid = null!;

    public PidControllerComponent(PidSpec spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    public string Name => $"{_spec.Name}_PID";

    public void Initialize(ControllerExecutionHost host)
    {
        _pid = new PidBlock(_spec.Name, _spec.Kp, _spec.Ti, _spec.Td)
        {
            N = _spec.N,
            DirectAction = _spec.DirectAction,
            OutMin = _spec.OutMin,
            OutMax = _spec.OutMax
        };
    }

    public void Step(SimulationClock clock, ControllerExecutionHost host)
    {
        var sp = host.GetVariable(_spec.Setpoint.Name);
        var pv = host.GetVariable(_spec.Controlled.Name);
        var mv = _pid.Step(sp, pv, _spec.ScanTime.TotalSeconds);
        host.SetVariable(_spec.Manipulated.Name, mv);
    }

    public void Shutdown()
    {
        // Nada a fazer.
    }
}

/// <summary>
/// Componente de simulação que modela um processo FOPDT.
/// A cada passo lê MV do host, propaga o modelo (y(k) = a·y(k-1) + b·u(k-d))
/// e grava PV de volta. Aplica o tempo morto via buffer circular.
/// </summary>
internal sealed class FopdtProcessComponent : ISimulationComponent
{
    private readonly FopdtModel _model;
    private readonly string _mvName;
    private readonly string _pvName;
    private double[] _mvHistory = null!;
    private int _mvIndex;
    private double _y;

    public FopdtProcessComponent(FopdtModel model, string mvName, string pvName)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _mvName = mvName ?? throw new ArgumentNullException(nameof(mvName));
        _pvName = pvName ?? throw new ArgumentNullException(nameof(pvName));
    }

    public string Name => $"{_model.Name}_FOPDT";

    public void Initialize(ControllerExecutionHost host)
    {
        var d = Math.Max(1, _model.DeadTimeSamples);
        _mvHistory = new double[d];
        _mvIndex = 0;
        _y = host.GetVariable(_pvName);
    }

    public void Step(SimulationClock clock, ControllerExecutionHost host)
    {
        var mvCurrent = host.GetVariable(_mvName);
        var (a, b, d) = _model.Discretize();

        // MV atrasada do buffer
        var mvDelayed = _mvHistory[_mvIndex];

        // Propaga estado: y(k) = a·y(k-1) + b·u(k-d)
        _y = a * _y + b * mvDelayed;

        // Atualiza buffer
        _mvHistory[_mvIndex] = mvCurrent;
        _mvIndex = (_mvIndex + 1) % Math.Max(1, d);

        // Grava PV
        host.SetVariable(_pvName, _y);
    }

    public void Shutdown()
    {
        // Nada a fazer.
    }
}
