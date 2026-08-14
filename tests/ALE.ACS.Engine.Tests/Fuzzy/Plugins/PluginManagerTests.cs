using ALE.ACS.Engine.Fuzzy.Defuzzification;
using ALE.ACS.Engine.Fuzzy.MembershipFunctions;
using ALE.ACS.Engine.Fuzzy.Plugins;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Plugins;

public sealed class PluginManagerTests
{
    [Fact]
    public void PluginManager_Should_Register_And_Use_Custom_Membership_Function_Plugin()
    {
        var manager = new PluginManager();
        var plugin = new CustomMembershipFunctionPlugin("FakeMembership", static () => new FakeMembershipFunction());

        manager.RegisterMembershipFunctionPlugin(plugin);

        var found = manager.TryGetMembershipFunctionPlugin("FakeMembership", out var stored);

        found.Should().BeTrue();
        stored.Should().NotBeNull();
        stored!.Create().Evaluate(10.0).Should().Be(0.42);
    }

    [Fact]
    public void PluginManager_Should_Register_And_Use_Custom_Defuzzifier_Plugin()
    {
        var manager = new PluginManager();
        var plugin = new CustomDefuzzifierPlugin("FakeDefuzzifier", static () => new FakeDefuzzifier());

        manager.RegisterDefuzzifierPlugin(plugin);

        var found = manager.TryGetDefuzzifierPlugin("FakeDefuzzifier", out var stored);

        found.Should().BeTrue();
        stored.Should().NotBeNull();
        var value = stored!.Create().Defuzzify([], 0.0, 1.0);
        value.Should().Be(123.0);
    }

    [Fact]
    public void PluginManager_Should_Register_And_Use_Custom_Rule_Operator_Plugin()
    {
        var manager = new PluginManager();
        var plugin = new CustomRuleOperatorPlugin("FakeOperator", static (a, b) => (a + b) / 2.0);

        manager.RegisterRuleOperatorPlugin(plugin);

        var found = manager.TryGetRuleOperatorPlugin("FakeOperator", out var stored);

        found.Should().BeTrue();
        stored.Should().NotBeNull();
        stored!.Evaluate(0.2, 0.8).Should().Be(0.5);
    }

    [Fact]
    public void PluginManager_Should_Be_ThreadSafe_During_Runtime_Registration()
    {
        var manager = new PluginManager();

        Parallel.For(0, 100, i =>
        {
            manager.RegisterRuleOperatorPlugin(new CustomRuleOperatorPlugin($"Operator-{i}", static (a, b) => Math.Max(a, b)));
        });

        for (var i = 0; i < 100; i++)
        {
            manager.TryGetRuleOperatorPlugin($"Operator-{i}", out var plugin).Should().BeTrue();
            plugin!.Evaluate(0.3, 0.7).Should().Be(0.7);
        }
    }

    private sealed class FakeMembershipFunction : IMembershipFunction
    {
        public string Name => "FakeMembership";

        public double Evaluate(double x) => 0.42;

        public (double Min, double Max) GetSupportBounds() => (0.0, 1.0);
    }

    private sealed class FakeDefuzzifier : IDefuzzifier
    {
        public DefuzzificationMethod Method => DefuzzificationMethod.Centroid;

        public double Defuzzify(List<(IMembershipFunction Function, double Weight)> activatedFunctions, double minValue, double maxValue)
            => 123.0;
    }
}
