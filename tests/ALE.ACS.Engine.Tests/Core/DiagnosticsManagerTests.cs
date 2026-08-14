using ALE.ACS.Engine.Core.Diagnostics;
using FluentAssertions;

namespace ALE.ACS.Engine.Tests.Core;

public sealed class DiagnosticsManagerTests
{
    [Fact]
    public void AddAndQuery_ShouldFilterBySeverity_AndCategory()
    {
        var manager = new DiagnosticsManager();
        manager.Add(new DiagnosticMessage(DiagnosticSeverity.Warning, DiagnosticCategory.Validation, "WARN_001", "Check config", DateTime.UtcNow));
        manager.Add(new DiagnosticMessage(DiagnosticSeverity.Error, DiagnosticCategory.Execution, "ERR_001", "Execution failed", DateTime.UtcNow));

        var warnings = manager.GetMessages(DiagnosticSeverity.Warning);
        var execution = manager.GetMessages(DiagnosticCategory.Execution);

        warnings.Should().ContainSingle(m => m.Code == "WARN_001");
        execution.Should().ContainSingle(m => m.Code == "ERR_001");
    }
}
