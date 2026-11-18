using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core.Applications;
using ALE.ACS.Engine.Core.Diagnostics;

namespace ALE.ACS.Engine.Core.Execution;

/// <summary>
/// Orchestrates a deterministic execution pipeline for engineering blocks.
/// </summary>
public sealed class ExecutionPipeline
{
    /// <summary>
    /// Executes the provided strategy within the supplied context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The execution result.</returns>
    public ExecutionResult Execute(ExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var strategy = context.Strategy;

        if (strategy is null || strategy.Blocks.Count == 0)
        {
            var emptyStatistics = new ExecutionStatistics(
                0,
                context.Variables.Count,
                0,
                0,
                TimeSpan.Zero);

            return new ExecutionResult(
                true,
                new Dictionary<string, object?>(),
                [],
                emptyStatistics);
        }

        var diagnostics = new List<DiagnosticMessage>();

        var orderedBlocks = TopologicalSort(
            strategy.Blocks,
            diagnostics);

        if (orderedBlocks is null)
        {
            diagnostics.Add(
                new DiagnosticMessage(
                    DiagnosticSeverity.Error,
                    DiagnosticCategory.Execution,
                    "EXECUTION_CYCLE_DETECTED",
                    "The strategy contains a circular dependency.",
                    DateTime.UtcNow));

            var failedStatistics = new ExecutionStatistics(
                strategy.Blocks.Count,
                context.Variables.Count,
                0,
                1,
                TimeSpan.Zero);

            return new ExecutionResult(
                false,
                new Dictionary<string, object?>(),
                diagnostics,
                failedStatistics);
        }

        var outputs = new Dictionary<string, object?>();

        foreach (var block in orderedBlocks)
        {
            outputs[block.Name] = block.Id;
        }

        var executionStatistics = new ExecutionStatistics(
            orderedBlocks.Count,
            context.Variables.Count,
            0,
            0,
            TimeSpan.Zero);

        return new ExecutionResult(
            true,
            outputs,
            diagnostics,
            executionStatistics);
    }

    /// <summary>
    /// Executa o pipeline <b>invocando os controllers</b> associados a cada block
    /// via o <paramref name="host"/>, em ordem topológica. Os inputs de cada block
    /// são lidos do <see cref="ControllerExecutionHost"/> (VariableStore) e os outputs
    /// são gravados de volta, permitindo comunicação entre controllers por nome.
    /// </summary>
    /// <param name="context">Contexto de execução (carrega a strategy).</param>
    /// <param name="host">Registro block→controller + variável compartilhada.</param>
    /// <returns>Resultado com outputs efetivamente computados pelos controllers.</returns>
    public ExecutionResult ExecuteWithControllers(
        ExecutionContext context,
        ControllerExecutionHost host)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(host);

        var startedAt = DateTime.UtcNow;
        var strategy = context.Strategy;
        var diagnostics = new List<DiagnosticMessage>();
        var outputs = new Dictionary<string, object?>();

        if (strategy is null || strategy.Blocks.Count == 0)
        {
            return new ExecutionResult(
                true,
                outputs,
                diagnostics,
                new ExecutionStatistics(0, context.Variables.Count, 0, 0, TimeSpan.Zero));
        }

        var orderedBlocks = TopologicalSort(strategy.Blocks, diagnostics);

        if (orderedBlocks is null)
        {
            diagnostics.Add(new DiagnosticMessage(
                DiagnosticSeverity.Error,
                DiagnosticCategory.Execution,
                "EXECUTION_CYCLE_DETECTED",
                "The strategy contains a circular dependency.",
                DateTime.UtcNow));

            return new ExecutionResult(
                false,
                outputs,
                diagnostics,
                new ExecutionStatistics(strategy.Blocks.Count, context.Variables.Count, 0, 1, TimeSpan.Zero));
        }

        int executedCount = 0;
        int warningCount = 0;
        int errorCount = 0;

        foreach (var block in orderedBlocks)
        {
            // Sem controller associado? Pula com aviso (não é erro fatal).
            if (!host.TryGetController(block.Id, out var controller) || controller is null)
            {
                diagnostics.Add(new DiagnosticMessage(
                    DiagnosticSeverity.Warning,
                    DiagnosticCategory.Execution,
                    "BLOCK_WITHOUT_CONTROLLER",
                    $"Block '{block.Name}' has no controller registered; skipped.",
                    DateTime.UtcNow));
                warningCount++;
                outputs[block.Name] = null;
                continue;
            }

            // Monta o ControllerInput a partir das variáveis nomeadas do host.
            // Os inputs do block definem quais variáveis o controller vai ler.
            var input = new ControllerInput();
            foreach (var v in block.Inputs)
            {
                if (host.TryGetVariable(v.Name, out var value))
                {
                    input.AddVariable(new ControllerVariable(
                        v.Name, value, DateTime.UtcNow, "Good"));
                }
            }

            try
            {
                var result = controller.Execute(input);

                // Grava os outputs de volta no host (por nome de variável).
                foreach (var outputVar in result.OutputVariables)
                {
                    host.SetVariable(outputVar.Name, outputVar.Value);
                    outputs[$"{block.Name}.{outputVar.Name}"] = outputVar.Value;
                }

                // Coleta mensagens do controller como diagnostics.
                foreach (var msg in result.Messages)
                {
                    var severity = msg.Severity switch
                    {
                        ControllerMessageSeverity.Info => DiagnosticSeverity.Information,
                        ControllerMessageSeverity.Warning => DiagnosticSeverity.Warning,
                        _ => DiagnosticSeverity.Error
                    };

                    diagnostics.Add(new DiagnosticMessage(
                        severity,
                        DiagnosticCategory.Execution,
                        msg.Code,
                        $"[{block.Name}] {msg.Message}",
                        msg.Timestamp));

                    if (severity == DiagnosticSeverity.Warning) warningCount++;
                    else if (severity == DiagnosticSeverity.Error) errorCount++;
                }

                outputs[block.Name] = result.Status;
                executedCount++;
            }
            catch (Exception ex)
            {
                diagnostics.Add(new DiagnosticMessage(
                    DiagnosticSeverity.Error,
                    DiagnosticCategory.Execution,
                    "CONTROLLER_EXCEPTION",
                    $"[{block.Name}] {ex.Message}",
                    DateTime.UtcNow));
                errorCount++;
                outputs[block.Name] = ControllerStatus.ExecutionFailure;
            }
        }

        var elapsed = DateTime.UtcNow - startedAt;

        return new ExecutionResult(
            success: errorCount == 0,
            outputs,
            diagnostics,
            new ExecutionStatistics(
                executedCount,
                context.Variables.Count,
                warningCount,
                errorCount,
                elapsed));
    }

    private static IReadOnlyList<EngineeringBlock>? TopologicalSort(
        IReadOnlyCollection<EngineeringBlock> blocks,
        List<DiagnosticMessage> diagnostics)
    {
        var graph = blocks.ToDictionary(
            block => block.Id,
            block => block);

        var indegree = blocks.ToDictionary(
            block => block.Id,
            _ => 0);

        var reverseDependencies = blocks.ToDictionary(
            block => block.Id,
            _ => new List<Guid>());

        foreach (var block in blocks)
        {
            foreach (var dependencyId in block.Dependencies)
            {
                if (!graph.ContainsKey(dependencyId))
                {
                    continue;
                }

                indegree[block.Id]++;

                reverseDependencies[dependencyId]
                    .Add(block.Id);
            }
        }

        var queue = new Queue<Guid>(
            indegree
                .Where(item => item.Value == 0)
                .Select(item => item.Key));

        var ordered = new List<Guid>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            ordered.Add(current);

            foreach (var dependentId in reverseDependencies[current])
            {
                indegree[dependentId]--;

                if (indegree[dependentId] == 0)
                {
                    queue.Enqueue(dependentId);
                }
            }
        }

        if (ordered.Count != blocks.Count)
        {
            return null;
        }

        return ordered
            .Select(id => graph[id])
            .ToList();
    }
}