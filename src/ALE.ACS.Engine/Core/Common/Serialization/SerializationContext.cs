namespace ALE.ACS.Engine.Core.Common.Serialization;

/// <summary>
/// Exposes context data during serialization operations.
/// </summary>
public sealed class SerializationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationContext"/> class.
    /// </summary>
    /// <param name="metadata">The metadata provided to the serializer.</param>
    public SerializationContext(IReadOnlyDictionary<string, object?>? metadata)
    {
        Metadata = metadata ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the metadata associated with the serialization operation.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; }
}
