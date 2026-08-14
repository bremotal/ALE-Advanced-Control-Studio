namespace ALE.ACS.Engine.Core.Diagnostics;

/// <summary>
/// Defines a provider that can contribute diagnostic messages.
/// </summary>
public interface IDiagnosticProvider
{
    /// <summary>
    /// Publishes diagnostics to the manager.
    /// </summary>
    /// <param name="manager">The diagnostics manager.</param>
    void Publish(DiagnosticsManager manager);
}
