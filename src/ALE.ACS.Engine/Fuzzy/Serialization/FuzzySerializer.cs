using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace ALE.ACS.Engine.Fuzzy.Serialization;

/// <summary>
/// Base contract for fuzzy serializers.
/// </summary>
public abstract class FuzzySerializer
{
    /// <summary>
    /// Serializes a payload to bytes.
    /// </summary>
    /// <param name="payload">The payload to serialize.</param>
    /// <returns>The serialized bytes.</returns>
    public abstract byte[] Serialize(FuzzySerializationPayload payload);

    /// <summary>
    /// Deserializes bytes into a payload.
    /// </summary>
    /// <param name="data">The serialized bytes.</param>
    /// <returns>The deserialized payload.</returns>
    public abstract FuzzySerializationPayload? Deserialize(byte[] data);
}

/// <summary>
/// Serializes fuzzy payloads to JSON.
/// </summary>
public sealed class FuzzyJsonSerializer : FuzzySerializer
{
    public override byte[] Serialize(FuzzySerializationPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        payload.Version = payload.Version > 0 ? payload.Version : 1;
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false }));
    }

    public override FuzzySerializationPayload? Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(data);
        return JsonSerializer.Deserialize<FuzzySerializationPayload>(json);
    }
}

/// <summary>
/// Serializes fuzzy payloads to XML.
/// </summary>
public sealed class FuzzyXmlSerializer : FuzzySerializer
{
    public override byte[] Serialize(FuzzySerializationPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        payload.Version = payload.Version > 0 ? payload.Version : 1;

        var serializer = new XmlSerializer(typeof(FuzzySerializationPayload));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, payload);
        return stream.ToArray();
    }

    public override FuzzySerializationPayload? Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
        {
            return null;
        }

        var serializer = new XmlSerializer(typeof(FuzzySerializationPayload));
        using var stream = new MemoryStream(data);
        return serializer.Deserialize(stream) as FuzzySerializationPayload;
    }
}

/// <summary>
/// Serializes fuzzy payloads to a compact binary format.
/// </summary>
public sealed class FuzzyBinarySerializer : FuzzySerializer
{
    public override byte[] Serialize(FuzzySerializationPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        payload.Version = payload.Version > 0 ? payload.Version : 1;

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(payload.Version);
        writer.Write(payload.Name ?? string.Empty);
        writer.Write(payload.Content ?? string.Empty);
        writer.Flush();
        return stream.ToArray();
    }

    public override FuzzySerializationPayload? Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
        {
            return null;
        }

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        return new FuzzySerializationPayload
        {
            Version = reader.ReadInt32(),
            Name = reader.ReadString(),
            Content = reader.ReadString()
        };
    }
}
