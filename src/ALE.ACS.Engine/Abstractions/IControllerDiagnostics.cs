using ALE.ACS.Engine.Core;

namespace ALE.ACS.Engine.Abstractions;

public interface IControllerDiagnostics
{
    ControllerExecutionMetrics LastExecutionMetrics { get; }

    IReadOnlyList<ControllerMessage> LastMessages { get; }
}