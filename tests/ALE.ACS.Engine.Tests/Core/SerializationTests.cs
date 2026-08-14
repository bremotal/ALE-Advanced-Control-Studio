using ALE.ACS.Engine.Core.Common.Serialization;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class SerializationTests
{
    [Fact]
    public void SerializeAndDeserialize_ShouldUseRegisteredSerializer()
    {
        var manager = new SerializationManager();
        manager.Register(new TestSerializer());

        var serialized = manager.Serialize("hello", SerializationFormat.Json);
        var deserialized = manager.Deserialize(serialized, typeof(string), SerializationFormat.Json);

        serialized.Should().Be("hello");
        deserialized.Should().BeOfType<string>().Which.Should().Be("hello");
    }

    private sealed class TestSerializer : ISerializer
    {
        public SerializationFormat Format => SerializationFormat.Json;

        public string Serialize(object value) => value.ToString()!;

        public object Deserialize(string payload, Type type) => payload;

        public bool CanHandle(SerializationFormat format) => format == SerializationFormat.Json;
    }
}
