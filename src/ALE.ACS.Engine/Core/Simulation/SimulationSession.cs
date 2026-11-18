using ALE.ACS.Engine.Core.Execution;

namespace ALE.ACS.Engine.Core.Simulation;

/// <summary>
/// Relógio lógico da simulação. Avança em passos discretos de <see cref="Step"/>.
/// </summary>
public sealed class SimulationClock
{
    public SimulationClock(TimeSpan step, TimeSpan? startTime = null, TimeSpan? endTime = null)
    {
        if (step <= TimeSpan.Zero)
        {
            throw new ArgumentException("Step deve ser positivo.", nameof(step));
        }

        Step = step;
        Current = startTime ?? TimeSpan.Zero;
        EndTime = endTime;
    }

    /// <summary>Delta de tempo de cada passo (período de amostragem).</summary>
    public TimeSpan Step { get; }

    /// <summary>Tempo lógico corrente.</summary>
    public TimeSpan Current { get; private set; }

    /// <summary>Tempo final opcional (null = sem limite).</summary>
    public TimeSpan? EndTime { get; }

    /// <summary>Indica se ainda há tempo a avançar.</summary>
    public bool CanAdvance => EndTime is null || Current < EndTime;

    /// <summary>Avança um passo e retorna o novo tempo corrente.</summary>
    public TimeSpan Advance()
    {
        Current += Step;
        return Current;
    }

    /// <summary>Reseta o relógio para o tempo inicial.</summary>
    public void Reset(TimeSpan? startTime = null)
    {
        Current = startTime ?? TimeSpan.Zero;
    }
}

/// <summary>
/// Estado de uma sessão de simulação.
/// </summary>
public enum SimulationState
{
    /// <summary>Sessão criada mas não iniciada.</summary>
    Created,

    /// <summary>Simulação em execução.</summary>
    Running,

    /// <summary>Simulação pausada (pode retomar).</summary>
    Paused,

    /// <summary>Simulação concluída com sucesso.</summary>
    Completed,

    /// <summary>Simulação abortada por erro.</summary>
    Aborted
}

/// <summary>
/// Componente que participa do ciclo de simulação (ex.: um controlador, um
/// modelo de processo, um logger). Recebe o clock e o host de variáveis.
/// </summary>
public interface ISimulationComponent
{
    /// <summary>Nome do componente.</summary>
    string Name { get; }

    /// <summary>Chamado uma vez no início da simulação.</summary>
    void Initialize(ControllerExecutionHost host);

    /// <summary>Chamado a cada passo de simulação.</summary>
    void Step(SimulationClock clock, ControllerExecutionHost host);

    /// <summary>Chamado ao final da simulação.</summary>
    void Shutdown();
}

/// <summary>
/// Estatísticas acumuladas de uma sessão de simulação.
/// </summary>
public sealed class SimulationStatistics
{
    public int StepCount { get; set; }
    public int ComponentCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan SimulatedDuration { get; set; }
}
