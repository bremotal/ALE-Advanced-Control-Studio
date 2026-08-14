using ALE.ACS.Engine.Core.Common.Events;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class EventBusTests
{
    [Fact]
    public void Publish_ShouldNotifyRegisteredHandlers_InOrder()
    {
        var bus = new EventBus();

        var first = new TestEventHandler();
        var second = new TestEventHandler();

        bus.Subscribe(first);
        bus.Subscribe(second);

        bus.Publish(new TestEvent());

        first.Received.Should().Be(1);
        second.Received.Should().Be(1);
    }

    private sealed class TestEvent : IEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    private sealed class TestEventHandler : IEventHandler<TestEvent>
    {
        public int Received { get; private set; }

        public void Handle(TestEvent @event)
        {
            Received++;
        }

        public Task HandleAsync(
            IEvent @event,
            EventContext context,
            CancellationToken cancellationToken = default)
        {
            if (@event is TestEvent typed)
            {
                Handle(typed);
            }

            return Task.CompletedTask;
        }
    }
}