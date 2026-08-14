namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Represents a non-blocking engineering recommendation.
/// </summary>
public sealed class Recommendation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Recommendation"/> class.
    /// </summary>
    /// <param name="code">The recommendation code.</param>
    /// <param name="text">The recommendation text.</param>
    public Recommendation(string code, string text)
    {
        Code = code;
        Text = text;
    }

    /// <summary>
    /// Gets the recommendation code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the recommendation text.
    /// </summary>
    public string Text { get; }
}
