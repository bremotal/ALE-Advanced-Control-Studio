namespace ALE.ACS.Engine.Core.Export;

/// <summary>
/// Represents failures that occur during export operations.
/// </summary>
public sealed class ExportException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExportException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ExportException(string message) : base(message)
    {
    }
}
