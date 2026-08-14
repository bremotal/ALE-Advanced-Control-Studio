namespace ALE.ACS.Application.Controllers;

/// <summary>
/// Defines a base IDE controller abstraction.
/// </summary>
public interface IIdeController
{
    /// <summary>
    /// Gets the controller type identifier.
    /// </summary>
    string Type { get; }
}

/// <summary>
/// Factory for creating IDE controller instances.
/// </summary>
public sealed class ControllerFactory
{
    /// <summary>
    /// Creates an IDE controller for the provided type.
    /// </summary>
    /// <param name="type">Controller type identifier.</param>
    /// <returns>A controller instance.</returns>
    public IIdeController Create(string type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        return new IdeController(type.ToLowerInvariant());
    }

    private sealed class IdeController : IIdeController
    {
        public IdeController(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }
}
