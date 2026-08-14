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