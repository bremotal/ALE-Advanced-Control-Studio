namespace ALE.ACS.Engine.Core.Common.Serialization;

/// <summary>
/// Manages known serializers by format.
/// </summary>
public sealed class SerializerRegistry
{
    private readonly Dictionary<SerializationFormat, ISerializer> _serializers = [];

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

        _serializers[serializer.Format] = serializer;
    }

    /// <summary>
    /// Gets a serializer by format.
    /// </summary>
    /// <param name="format">The serialization format.</param>
    /// <returns>The serializer instance.</returns>
    public ISerializer? Get(SerializationFormat format)
        => _serializers.TryGetValue(format, out var serializer) ? serializer : null;
}
