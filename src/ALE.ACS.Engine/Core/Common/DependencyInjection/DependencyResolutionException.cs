namespace ALE.ACS.Engine.Core.Common.DependencyInjection;

/// <summary>
/// Represents an error that occurs when resolving a service.
/// </summary>
public sealed class DependencyResolutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public DependencyResolutionException(string message) : base(message)
    {
    }
}
