namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Represents failures that occur during dependency resolution.
/// </summary>
public sealed class DependencyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public DependencyException(string message) : base(message)
    {
    }
}
