namespace ALE.ACS.Engine.Core.Common.Serialization;

/// <summary>
/// Defines supported serialization formats.
/// </summary>
public enum SerializationFormat
{
    /// <summary>
    /// JavaScript Object Notation.
    /// </summary>
    Json,

    /// <summary>
    /// Extensible Markup Language.
    /// </summary>
    Xml,

    /// <summary>
    /// Binary serialization.
    /// </summary>
    Binary,

    /// <summary>
    /// Custom serialization format.
    /// </summary>
    Custom
}
