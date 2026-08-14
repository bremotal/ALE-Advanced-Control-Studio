namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Represents a technical warning emitted by the engine.
/// </summary>
public sealed class WarningRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WarningRecord"/> class.
    /// </summary>
    /// <param name="code">The warning code.</param>
    /// <param name="text">The warning text.</param>
    public WarningRecord(string code, string text)
    {
        Code = code;
        Text = text;
    }

    /// <summary>
    /// Gets the warning code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the warning text.
    /// </summary>
    public string Text { get; }
}
