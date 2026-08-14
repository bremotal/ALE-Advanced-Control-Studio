namespace ALE.ACS.Engine.Core.Common.Serialization;

/// <summary>
/// Represents failures that occur during serialization.
/// </summary>
public sealed class SerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public SerializationException(string message) : base(message)
    {
    }
}
