namespace ALE.ACS.Engine.Core.Projects;

/// <summary>
/// Represents the outcome of a project repository operation.
/// </summary>
public sealed class RepositoryResult
{
    private RepositoryResult(bool success, string? message, IReadOnlyCollection<string>? messages, IReadOnlyCollection<string>? warnings, IReadOnlyCollection<string>? errors, ProjectSnapshot? snapshot)
    {
        Success = success;
        Message = message;
        Messages = messages ?? Array.Empty<string>();
        Warnings = warnings ?? Array.Empty<string>();
        Errors = errors ?? Array.Empty<string>();
        Snapshot = snapshot;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the primary message.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the messages.
    /// </summary>
    public IReadOnlyCollection<string> Messages { get; }

    /// <summary>
    /// Gets the warnings.
    /// </summary>
    public IReadOnlyCollection<string> Warnings { get; }

    /// <summary>
    /// Gets the errors.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; }

    /// <summary>
    /// Gets the loaded snapshot, if any.
    /// </summary>
    public ProjectSnapshot? Snapshot { get; }

    /// <summary>
    /// Creates a successful repository result.
    /// </summary>
    /// <param name="snapshot">The snapshot to return.</param>
    /// <returns>The repository result.</returns>
    public static RepositoryResult CreateSuccess(ProjectSnapshot? snapshot = null)
        => new(true, null, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), snapshot);

    /// <summary>
    /// Creates a failed repository result.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>The repository result.</returns>
    public static RepositoryResult CreateFailure(string message)
        => new(false, message, new[] { message }, Array.Empty<string>(), new[] { message }, null);
}
