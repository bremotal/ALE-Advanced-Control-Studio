using System.Xml.Serialization;

namespace ALE.ACS.Engine.Fuzzy.Serialization;

/// <summary>
/// Represents a serializable fuzzy payload with versioning support.
/// </summary>
[XmlRoot("FuzzySerializationPayload")]
public sealed class FuzzySerializationPayload
{
    /// <summary>
    /// Gets or sets the serialization version.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Gets or sets the payload name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the payload content.
    /// </summary>
    public string? Content { get; set; }
}
