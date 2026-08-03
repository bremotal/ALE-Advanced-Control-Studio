using ALE.ACS.Engine.Abstractions;

namespace ALE.ACS.Engine.Core;

public abstract class ControllerBase : IController, IControllerDiagnostics
{
    private static readonly ControllerExecutionMetrics EmptyMetrics = new(DateTime.MinValue, DateTime.MinValue);
    private readonly List<ControllerMessage> _lastMessages = [];

    public ControllerExecutionMetrics LastExecutionMetrics { get; private set; } = EmptyMetrics;

    public IReadOnlyList<ControllerMessage> LastMessages => _lastMessages;

    public ControllerResult Execute(ControllerInput input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        var startTime = DateTime.UtcNow;
        var context = new ControllerExecutionContext(input, startTime);

        if (input.Variables.Count == 0)
        {
            var invalidInputMessage = new ControllerMessage(
                ControllerMessageSeverity.Error,
                "CTRL_INVALID_INPUT",
                "Controller input must contain at least one variable.",
                DateTime.UtcNow);

            var invalidMetrics = new ControllerExecutionMetrics(startTime, DateTime.UtcNow);
            LastExecutionMetrics = invalidMetrics;
            SetLastMessages([invalidInputMessage]);

            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.InvalidInput,
                [invalidInputMessage],
                invalidMetrics.ElapsedTime,
                false);
        }

        try
        {
            var result = ExecuteCore(context);
            var metrics = new ControllerExecutionMetrics(startTime, DateTime.UtcNow);

            LastExecutionMetrics = metrics;
            SetLastMessages(result.Messages);

            return new ControllerResult(
                result.OutputVariables,
                result.Status,
                result.Messages,
                metrics.ElapsedTime,
                result.Success);
        }
        catch (Exception ex)
        {
            var failureMessage = new ControllerMessage(
                ControllerMessageSeverity.Error,
                "CTRL_EXECUTION_FAILURE",
                ex.Message,
                DateTime.UtcNow);

            var failureMetrics = new ControllerExecutionMetrics(startTime, DateTime.UtcNow);
            LastExecutionMetrics = failureMetrics;
            SetLastMessages([failureMessage]);

            return new ControllerResult(
                new ControllerVariableCollection(),
                ControllerStatus.ExecutionFailure,
                [failureMessage],
                failureMetrics.ElapsedTime,
                false);
        }
    }

    protected abstract ControllerResult ExecuteCore(ControllerExecutionContext context);

    private void SetLastMessages(IReadOnlyList<ControllerMessage> messages)
    {
        _lastMessages.Clear();
        _lastMessages.AddRange(messages);
    }
}