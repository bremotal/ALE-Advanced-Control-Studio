using ALE.ACS.Application.Controllers;
using ALE.ACS.Application.EventBus;
using ALE.ACS.Application.Projects;
using Xunit;

namespace ALE.ACS.Application.Tests.IDE;

public sealed class IdeInfrastructureTests
{
    [Fact]
    public void EventBus_Should_Publish_And_Subscribe_Event()
    {
        var eventBus = new InMemoryEventBus();
        var received = string.Empty;

        eventBus.Subscribe<ProjectCreatedEvent>(message => received = message.ProjectName);

        eventBus.Publish(new ProjectCreatedEvent("Studio"));

        Assert.Equal("Studio", received);
    }

    [Fact]
    public void ControllerFactory_Should_Create_Controller_Instance()
    {
        var factory = new ControllerFactory();

        var controller = factory.Create("plant");

        Assert.NotNull(controller);
        Assert.Equal("plant", controller.Type);
    }

    [Fact]
    public void ProjectManager_Should_Create_Project()
    {
        var manager = new ProjectManager();

        var result = manager.CreateProject("Industrial Project");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Industrial Project", result.Value!.Name);
    }
}
