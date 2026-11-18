using ALE.ACS.Engine.Core.Execution;
using EngineExecutionContext = ALE.ACS.Engine.Core.Execution.ExecutionContext;

namespace ALE.ACS.Engine.Core.Simulation;

/// <summary>
/// Coordena o lifecycle de uma simulação com loop de tempo real.
/// </summary>
/// <remarks>
/// A simulação avança em passos discretos (clock). A cada passo, todos os
/// <see cref="ISimulationComponent"/> registrados são invocados em ordem,
/// permitindo que controladores e modelos de processo evoluam o estado.
/// </remarks>
public sealed class SimulationEngine
{
    private readonly List<ISimulationComponent> _components = [];
    private readonly ControllerExecutionHost _host = new();
    private SimulationState _state = SimulationState.Created;
    private SimulationStatistics? _statistics;

    /// <summary>Estado corrente da sessão.</summary>
    public SimulationState State => _state;

    /// <summary>Estatísticas da última execução (null se nunca rodou).</summary>
    public SimulationStatistics? Statistics => _statistics;

    /// <summary>Host de variáveis compartilhado entre componentes.</summary>
    public ControllerExecutionHost Host => _host;

    /// <summary>Registra um componente na simulação.</summary>
    public void AddComponent(ISimulationComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (_state == SimulationState.Running)
        {
            throw new InvalidOperationException(
                "Não é possível adicionar componente durante a execução.");
        }
        _components.Add(component);
    }

    /// <summary>Define o valor inicial de uma variável.</summary>
    public void SetVariable(string name, double value)
        => _host.SetVariable(name, value);

    /// <summary>
    /// Executa uma simulação completa do tempo inicial até o final.
    /// </summary>
    /// <param name="clock">Relógio com step e endTime configurados.</param>
    /// <returns>Estatísticas da execução.</returns>
    public SimulationStatistics Run(SimulationClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (_state == SimulationState.Running)
        {
            throw new InvalidOperationException("Simulação já está em execução.");
        }

        var startedAt = DateTime.UtcNow;
        _state = SimulationState.Running;

        var stats = new SimulationStatistics
        {
            ComponentCount = _components.Count,
            SimulatedDuration = TimeSpan.Zero
        };

        // Inicializa componentes
        foreach (var component in _components)
        {
            try
            {
                component.Initialize(_host);
            }
            catch (Exception ex)
            {
                stats.ErrorCount++;
                _state = SimulationState.Aborted;
                stats.TotalDuration = DateTime.UtcNow - startedAt;
                _statistics = stats;
                throw new InvalidOperationException(
                    $"Falha ao inicializar '{component.Name}': {ex.Message}", ex);
            }
        }

        // Loop de tempo
        while (clock.CanAdvance)
        {
            var current = clock.Advance();
            stats.StepCount++;
            stats.SimulatedDuration = current;

            foreach (var component in _components)
            {
                try
                {
                    component.Step(clock, _host);
                }
                catch (Exception ex)
                {
                    stats.ErrorCount++;
                    _state = SimulationState.Aborted;
                    stats.TotalDuration = DateTime.UtcNow - startedAt;

                    // Shutdown dos componentes restantes
                    ShutdownComponents(stats);

                    _statistics = stats;
                    throw new InvalidOperationException(
                        $"Falha no step {stats.StepCount} do componente '{component.Name}': {ex.Message}", ex);
                }
            }
        }

        // Shutdown
        ShutdownComponents(stats);

        _state = SimulationState.Completed;
        stats.TotalDuration = DateTime.UtcNow - startedAt;
        _statistics = stats;

        return stats;
    }

    /// <summary>
    /// Executa um único passo de simulação (modo manual/step-by-step).
    /// </summary>
    public void StepOnce(SimulationClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        if (!clock.CanAdvance)
        {
            return;
        }

        if (_state is not (SimulationState.Created or SimulationState.Running or SimulationState.Paused))
        {
            throw new InvalidOperationException(
                $"Não é possível avançar no estado {_state}.");
        }

        if (_state == SimulationState.Created)
        {
            foreach (var component in _components)
            {
                component.Initialize(_host);
            }
            _state = SimulationState.Running;
        }

        var current = clock.Advance();

        foreach (var component in _components)
        {
            component.Step(clock, _host);
        }
    }

    /// <summary>Pausa a simulação (pode retomar com StepOnce ou Run).</summary>
    public void Pause()
    {
        if (_state == SimulationState.Running)
        {
            _state = SimulationState.Paused;
        }
    }

    /// <summary>Reseta a sessão para o estado inicial.</summary>
    public void Reset()
    {
        _state = SimulationState.Created;
        _host.Clear();
        _statistics = null;
    }

    /// <summary>
    /// Compatibilidade legada: roda um SimulationContext via ExecutionPipeline
    /// (sem controllers, só ordenação topológica). Mantido para não quebrar
    /// callers existentes; novos callers devem usar Run(SimulationClock).
    /// </summary>
    public SimulationResult Run(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var executionContext = new EngineExecutionContext(
            context.Project,
            context.Application,
            context.Strategy,
            context.Variables,
            new Dictionary<string, object?>());

        var pipeline = new ExecutionPipeline();

        var pipelineResult = pipeline.Execute(executionContext);

        return new SimulationResult(
            pipelineResult.Success,
            pipelineResult.Outputs,
            pipelineResult.Diagnostics,
            pipelineResult.Statistics,
            context.Time);
    }

    private void ShutdownComponents(SimulationStatistics stats)
    {
        foreach (var component in _components)
        {
            try
            {
                component.Shutdown();
            }
            catch (Exception)
            {
                stats.WarningCount++;
            }
        }
    }
}
