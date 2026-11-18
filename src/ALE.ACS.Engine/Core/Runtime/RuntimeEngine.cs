using ALE.ACS.Engine.Core.Execution;
using EngineExecutionContext = ALE.ACS.Engine.Core.Execution.ExecutionContext;

namespace ALE.ACS.Engine.Core.Runtime;

/// <summary>
/// Coordena sessões de runtime com scan loop real.
/// </summary>
/// <remarks>
/// O runtime difere da simulação: em runtime, o tempo é wall-clock (cada scan
/// dura <c>Configuration.ScanTime</c>) e o objetivo é executar controllers em
/// produção (não em simulação acelerada).
/// </remarks>
public sealed class RuntimeEngine
{
    /// <summary>
    /// Executa uma sessão de runtime completa até atingir MaxScans ou falha.
    /// </summary>
    /// <param name="session">Sessão configurada com components e configuration.</param>
    /// <returns>Estatísticas da execução.</returns>
    public RuntimeStatistics Execute(RuntimeSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.State == RuntimeState.Running)
        {
            throw new RuntimeException("Sessão já está em execução.");
        }

        var startedAt = DateTime.UtcNow;
        session.State = RuntimeState.Running;

        var stats = new RuntimeStatistics
        {
            ComponentCount = session.Scheduler.Components.Count
        };

        // Inicializa componentes
        try
        {
            session.Scheduler.InitializeAll(session.Host);
        }
        catch (Exception ex)
        {
            stats.ErrorCount++;
            session.State = RuntimeState.Faulted;
            stats.TotalDuration = DateTime.UtcNow - startedAt;
            session.Statistics = stats;
            throw new RuntimeException(
                $"Falha ao inicializar runtime: {ex.Message}", ex);
        }

        long scansExecuted = 0;
        var scanDurations = new List<double>();
        var maxScans = session.Configuration.MaxScans;

        try
        {
            while (session.State == RuntimeState.Running)
            {
                if (maxScans > 0 && scansExecuted >= maxScans)
                {
                    break;
                }

                var scanStart = DateTime.UtcNow;

                session.Scheduler.ExecuteScan(session.Host);

                var scanElapsed = DateTime.UtcNow - scanStart;
                stats.LastScanDuration = scanElapsed;
                scanDurations.Add(scanElapsed.TotalMilliseconds);

                scansExecuted++;
                stats.ScanCount = scansExecuted;

                // Aguarda o resto do scan time (wall-clock) se o scan foi mais rápido
                var remaining = session.Configuration.ScanTime - scanElapsed;
                if (remaining > TimeSpan.Zero)
                {
                    Thread.Sleep(remaining);
                }
            }
        }
        catch (Exception ex)
        {
            stats.ErrorCount++;
            session.State = RuntimeState.Faulted;
            stats.TotalDuration = DateTime.UtcNow - startedAt;
            stats.AverageScanDurationMs = scanDurations.Count > 0
                ? scanDurations.Average() : 0;
            session.Statistics = stats;

            // Shutdown dos componentes
            try { session.Scheduler.ShutdownAll(); } catch { /* swallow */ }

            throw new RuntimeException(
                $"Falha no scan {scansExecuted}: {ex.Message}", ex);
        }

        // Shutdown
        try
        {
            session.Scheduler.ShutdownAll();
        }
        catch (Exception)
        {
            stats.WarningCount++;
        }

        session.State = RuntimeState.Stopped;
        stats.TotalDuration = DateTime.UtcNow - startedAt;
        stats.AverageScanDurationMs = scanDurations.Count > 0
            ? scanDurations.Average() : 0;
        session.Statistics = stats;

        return stats;
    }

    /// <summary>
    /// Para a execução da sessão (marcando State = Stopped para o loop encerrar).
    /// </summary>
    public void Stop(RuntimeSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.State == RuntimeState.Running)
        {
            session.State = RuntimeState.Stopped;
        }
    }

    /// <summary>
    /// Executa um único scan manualmente (modo step-by-step).
    /// </summary>
    public void ExecuteOnce(RuntimeSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.State is not (RuntimeState.Created or RuntimeState.Running or RuntimeState.Stopped))
        {
            throw new RuntimeException(
                $"Não é possível executar scan no estado {session.State}.");
        }

        if (session.State == RuntimeState.Created)
        {
            session.Scheduler.InitializeAll(session.Host);
            session.State = RuntimeState.Running;
        }

        session.Scheduler.ExecuteScan(session.Host);
    }

    /// <summary>
    /// Compatibilidade legada: executa um RuntimeContext via ExecutionPipeline
    /// (sem controllers). Mantido para não quebrar callers existentes.
    /// </summary>
    public RuntimeResult Execute(RuntimeContext context)
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

        return new RuntimeResult(
            pipelineResult.Success,
            pipelineResult.Diagnostics,
            pipelineResult.Statistics,
            TimeSpan.Zero);
    }
}
