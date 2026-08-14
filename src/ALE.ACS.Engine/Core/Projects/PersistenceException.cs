namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Represents failures that occur during project persistence.
/// </summary>
public sealed class PersistenceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersistenceException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public PersistenceException(string message) : base(message)
    {
    }
}
