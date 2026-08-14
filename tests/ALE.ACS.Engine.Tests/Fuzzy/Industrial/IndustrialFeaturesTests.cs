using ALE.ACS.Engine.Fuzzy;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Industrial;

public sealed class IndustrialFeaturesTests
{
    [Fact]
    public void MultiOutputController_Should_Evaluate_Multiple_Outputs()
    {
        var controller = new MultiOutputController([
            new OutputChannel("Fan", inputs => inputs["Temperature"] * 2.0),
            new OutputChannel("Valve", inputs => 100.0 - inputs["Temperature"]) 
        ]);

        var result = controller.Evaluate(new Dictionary<string, double>
        {
            ["Temperature"] = 30.0
        });

        result.Should().ContainKey("Fan").WhoseValue.Should().Be(60.0);
        result.Should().ContainKey("Valve").WhoseValue.Should().Be(70.0);
    }

    [Fact]
    public void DynamicRules_Should_Update_Rules_At_Runtime()
    {
        var dynamicRules = new DynamicRules(["Rule-A", "Rule-B"]);

        dynamicRules.Update(["Rule-C"]);

        dynamicRules.GetRules().Should().Equal("Rule-C");
    }

    [Fact]
    public void RuntimeParameterUpdate_Should_Update_Variable_Limits_At_Runtime()
    {
        var updater = new RuntimeParameterUpdate();

        updater.UpdateBounds("Temperature", -10.0, 80.0);
        updater.UpdateBounds("Temperature", 0.0, 100.0);

        var found = updater.TryGetBounds("Temperature", out var bounds);

        found.Should().BeTrue();
        bounds.Min.Should().Be(0.0);
        bounds.Max.Should().Be(100.0);
    }

    [Fact]
    public void GainScheduling_Should_Switch_Rule_Sets_Based_On_Process_State()
    {
        var lowLoadRules = new DynamicRules(["Rule-Low-1"]);
        var highLoadRules = new DynamicRules(["Rule-High-1"]);

        var scheduling = new GainScheduling();
        scheduling.RegisterSchedule(inputs => inputs["Load"] < 0.7, lowLoadRules);
        scheduling.RegisterSchedule(inputs => inputs["Load"] >= 0.7, highLoadRules);

        var selected = scheduling.SelectRules(new Dictionary<string, double> { ["Load"] = 0.9 });

        selected.GetRules().Should().Equal("Rule-High-1");
    }
}
