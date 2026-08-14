using ALE.ACS.Engine.Core.Common.DependencyInjection;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Resolve_ShouldHonorSingletonAndTransientLifetimes()
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(DependencyTarget), typeof(DependencyTarget));
        services.AddTransient(typeof(TransientDependency), typeof(TransientDependency));

        var provider = new ServiceProvider(services);
        var firstSingleton = provider.Resolve(typeof(DependencyTarget));
        var secondSingleton = provider.Resolve(typeof(DependencyTarget));
        var transient = provider.Resolve(typeof(TransientDependency));

        firstSingleton.Should().BeSameAs(secondSingleton);
        transient.Should().NotBeNull();
    }

    private sealed class DependencyTarget
    {
    }

    private sealed class TransientDependency
    {
    }
}
