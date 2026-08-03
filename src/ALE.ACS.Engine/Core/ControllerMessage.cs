namespace ALE.ACS.Engine.Core;

public enum ControllerMessageSeverity
{
    Info,
    Warning,
    Error
}

public sealed class ControllerMessage
{
    public ControllerMessage(ControllerMessageSeverity severity, string code, string message, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Message code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message text is required.", nameof(message));

        Severity = severity;
        Code = code;
        Message = message;
        Timestamp = timestamp;
    }

    public ControllerMessageSeverity Severity { get; }

    public string Code { get; }

    public string Message { get; }

    public DateTime Timestamp { get; }
}