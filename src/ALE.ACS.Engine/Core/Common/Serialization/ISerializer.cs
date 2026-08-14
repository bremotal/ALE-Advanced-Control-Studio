namespace ALE.ACS.Engine.Core.Common.Serialization;

/// <summary>
/// Defines a serializer for engineering objects.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Gets the format handled by the serializer.
    /// </summary>
    SerializationFormat Format { get; }

    /// <summary>
    /// Serializes an object to a string payload.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized payload.</returns>
    string Serialize(object value);

    /// <summary>
    /// Deserializes a payload into an object.
    /// </summary>
    /// <param name="payload">The payload to deserialize.</param>
    /// <param name="type">The target type.</param>
    /// <returns>The deserialized object.</returns>
    object Deserialize(string payload, Type type);

    /// <summary>
    /// Determines whether the serializer can handle the supplied format.
    /// </summary>
    /// <param name="format">The format to evaluate.</param>
    /// <returns>True when the format is supported.</returns>
    bool CanHandle(SerializationFormat format);
}
