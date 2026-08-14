using ALE.ACS.Engine.Core.Applications;
using ALE.ACS.Engine.Core.Execution;
using FluentAssertions;

using EngineExecutionContext = ALE.ACS.Engine.Core.Execution.ExecutionContext;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class ExecutionPipelineTests
{
    [Fact]
    public void Execute_ShouldBuildDeterministicPlan_AndReportStatistics()
    {
        var variable1 = new EngineeringVariable(
            Guid.NewGuid(),
            "Temp",
            "°C",
            "Temperature",
            0,
            100,
            true);

        var variable2 = new EngineeringVariable(
            Guid.NewGuid(),
            "Pressure",
            "bar",
            "Pressure",
            1,
            10,
            true);

        var sourceBlock = new EngineeringBlock(
            Guid.NewGuid(),
            "Source",
            "Source",
            [variable1],
            [variable2],
            null,
            []);

        var targetBlock = new EngineeringBlock(
            Guid.NewGuid(),
            "Target",
            "Target",
            [variable2],
            [],
            null,
            []);

        var strategy = new EngineeringStrategy(
            Guid.NewGuid(),
            "Strategy",
            "Test strategy",
            [sourceBlock, targetBlock]);

        var context = new EngineExecutionContext(
            null,
            null,
            strategy,
            [variable1, variable2],
            new Dictionary<string, object?>());

        var pipeline = new ExecutionPipeline();

        var result = pipeline.Execute(context);

        result.Success.Should().BeTrue();
        result.Outputs.Should().ContainKeys(sourceBlock.Name, targetBlock.Name);
        result.Statistics.BlockCount.Should().Be(2);
        result.Statistics.VariableCount.Should().Be(2);
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Execute_ShouldDetectCycles_AndFailGracefully()
    {
        var variable = new EngineeringVariable(
            Guid.NewGuid(),
            "Signal",
            "mA",
            "Signal",
            0,
            100,
            true);

        var blockA = new EngineeringBlock(
            Guid.NewGuid(),
            "A",
            "A",
            [variable],
            [variable],
            null,
            []);

        var blockB = new EngineeringBlock(
            Guid.NewGuid(),
            "B",
            "B",
            [variable],
            [variable],
            null,
            []);

        blockA.AddDependency(blockB.Id);
        blockB.AddDependency(blockA.Id);

        var strategy = new EngineeringStrategy(
            Guid.NewGuid(),
            "Strategy",
            "Cycle",
            [blockA, blockB]);

        var context = new EngineExecutionContext(
            null,
            null,
            strategy,
            [variable],
            new Dictionary<string, object?>());

        var pipeline = new ExecutionPipeline();

        var result = pipeline.Execute(context);

        result.Success.Should().BeFalse();
        result.Diagnostics.Should()
            .ContainSingle(d => d.Code == "EXECUTION_CYCLE_DETECTED");

        result.Statistics.BlockCount.Should().Be(2);
        result.Statistics.ErrorCount.Should().Be(1);
        result.Outputs.Should().BeEmpty();
    }
}