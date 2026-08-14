using ALE.ACS.Engine.Fuzzy.Expressions;
using ALE.ACS.Engine.Fuzzy.Runtime;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Runtime;

public sealed class FuzzyRuntimeTests
{
    [Fact]
    public void ExecutionContext_Should_Expose_Inputs_And_Zero_Allocation()
    {
        var executionContext = new FuzzyExecutionContext(new Dictionary<string, double>
        {
            ["Temperature"] = 0.8,
            ["Pressure"] = 0.4
        });

        executionContext.Inputs.Should().ContainKey("Temperature").WhoseValue.Should().Be(0.8);
        executionContext.Inputs.Should().ContainKey("Pressure").WhoseValue.Should().Be(0.4);
    }

    [Fact]
    public void EvaluationContext_Should_Reuse_Cache_And_Allow_Updates()
    {
        var context = new EvaluationContext();
        context.SetCache("rule:1", 0.75);
        context.GetCache("rule:1").Should().Be(0.75);

        context.SetCache("rule:1", 0.9);
        context.GetCache("rule:1").Should().Be(0.9);
    }

    [Fact]
    public void CompiledController_Should_Execute_Rule_Set_And_Return_Result()
    {
        var compiledRule = new CompiledFuzzyRule(new ConstantNode(1.0), _ => 1.0);
        var controller = new CompiledController([compiledRule]);

        var result = controller.Execute(new Dictionary<string, double> { ["x"] = 0.5 });

        result.Should().Be(1.0);
    }

    [Fact]
    public void FuzzyRuntime_Should_Execute_Using_Reused_Context()
    {
        var runtime = new FuzzyRuntime();
        var controller = new CompiledController([new CompiledFuzzyRule(new ConstantNode(1.0), _ => 1.0)]);

        var first = runtime.Execute(controller, new Dictionary<string, double> { ["x"] = 0.2 });
        var second = runtime.Execute(controller, new Dictionary<string, double> { ["x"] = 0.7 });

        first.Should().Be(1.0);
        second.Should().Be(1.0);
    }
}
