using ALE.ACS.Engine.Core.Plugins;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class PluginManagerTests
{
    [Fact]
    public void RegisterAndInitialize_ShouldManageLifecycle()
    {
        var manager = new PluginManager();
        var plugin = new TestPlugin();

        manager.Register(plugin);
        manager.InitializeAll();

        manager.GetPluginState(plugin.Descriptor.Id).Should().Be(PluginState.Initialized);
        manager.ListPlugins().Should().ContainSingle(p => p.Descriptor.Id == plugin.Descriptor.Id);

        manager.ShutdownAll();
        manager.GetPluginState(plugin.Descriptor.Id).Should().Be(PluginState.ShutDown);
    }

    private sealed class TestPlugin : IEngineeringPlugin
    {
        public PluginDescriptor Descriptor { get; } = new("test-plugin", "Test Plugin", "1.0", "Tests", "Used by tests", []);

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }
    }
}
