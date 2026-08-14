namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Defines supported export formats.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Siemens SCL export.
    /// </summary>
    SiemensSCL,

    /// <summary>
    /// Rockwell Structured Text export.
    /// </summary>
    RockwellST,

    /// <summary>
    /// CODESYS Structured Text export.
    /// </summary>
    CODESYSST,

    /// <summary>
    /// Lookup table export.
    /// </summary>
    LookupTable,

    /// <summary>
    /// JavaScript Object Notation export.
    /// </summary>
    Json,

    /// <summary>
    /// Extensible Markup Language export.
    /// </summary>
    Xml,

    /// <summary>
    /// Comma separated values export.
    /// </summary>
    Csv
}
