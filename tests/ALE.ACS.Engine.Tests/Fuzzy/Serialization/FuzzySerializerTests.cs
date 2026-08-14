using System.Text;
using ALE.ACS.Engine.Fuzzy.Serialization;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Fuzzy.Serialization;

public sealed class FuzzySerializerTests
{
    [Fact]
    public void JsonSerializer_Should_RoundTrip_And_Preserve_Version()
    {
        var serializer = new FuzzyJsonSerializer();
        var payload = new FuzzySerializationPayload
        {
            Version = 1,
            Name = "Demo",
            Content = "temperature=high"
        };

        var bytes = serializer.Serialize(payload);
        var roundTrip = serializer.Deserialize(bytes);

        roundTrip.Should().NotBeNull();
        roundTrip!.Version.Should().Be(1);
        roundTrip.Name.Should().Be("Demo");
        roundTrip.Content.Should().Be("temperature=high");
    }

    [Fact]
    public void XmlSerializer_Should_RoundTrip_And_Preserve_Version()
    {
        var serializer = new FuzzyXmlSerializer();
        var payload = new FuzzySerializationPayload
        {
            Version = 2,
            Name = "XmlDemo",
            Content = "pressure=low"
        };

        var bytes = serializer.Serialize(payload);
        var roundTrip = serializer.Deserialize(bytes);

        roundTrip.Should().NotBeNull();
        roundTrip!.Version.Should().Be(2);
        roundTrip.Name.Should().Be("XmlDemo");
        roundTrip.Content.Should().Be("pressure=low");
    }

    [Fact]
    public void BinarySerializer_Should_RoundTrip_And_Preserve_Version()
    {
        var serializer = new FuzzyBinarySerializer();
        var payload = new FuzzySerializationPayload
        {
            Version = 3,
            Name = "BinaryDemo",
            Content = "speed=fast"
        };

        var bytes = serializer.Serialize(payload);
        var roundTrip = serializer.Deserialize(bytes);

        roundTrip.Should().NotBeNull();
        roundTrip!.Version.Should().Be(3);
        roundTrip.Name.Should().Be("BinaryDemo");
        roundTrip.Content.Should().Be("speed=fast");
    }
}
