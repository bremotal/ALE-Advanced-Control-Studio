namespace ALE.ACS.Engine.Core.Common.Serialization;

/// <summary>
/// Coordinates serialization operations across registered serializers.
/// </summary>
public sealed class SerializationManager
{
    private readonly List<ISerializer> _serializers = [];

    /// <summary>
    /// Registers a serializer.
    /// </summary>
    /// <param name="serializer">The serializer to register.</param>
    public void Register(ISerializer serializer)
    {
        if (serializer is null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }

        _serializers.Add(serializer);
    }

    /// <summary>
    /// Selects a serializer that can handle the supplied format.
    /// </summary>
    /// <param name="format">The format to locate.</param>
    /// <returns>The serializer instance.</returns>
    public ISerializer? GetSerializer(SerializationFormat format)
        => _serializers.FirstOrDefault(serializer => serializer.CanHandle(format));

    /// <summary>
    /// Serializes an object with the appropriate serializer.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="format">The desired format.</param>
    /// <returns>The serialized payload.</returns>
    public string Serialize(object value, SerializationFormat format)
    {
        var serializer = GetSerializer(format) ?? throw new InvalidOperationException($"No serializer registered for {format}.");
        return serializer.Serialize(value);
    }

    /// <summary>
    /// Deserializes a payload with the appropriate serializer.
    /// </summary>
    /// <param name="payload">The payload to deserialize.</param>
    /// <param name="type">The target type.</param>
    /// <param name="format">The serialization format.</param>
    /// <returns>The deserialized value.</returns>
    public object Deserialize(string payload, Type type, SerializationFormat format)
    {
        var serializer = GetSerializer(format) ?? throw new InvalidOperationException($"No serializer registered for {format}.");
        return serializer.Deserialize(payload, type);
    }
}
