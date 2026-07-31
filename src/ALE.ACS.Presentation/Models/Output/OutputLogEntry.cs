namespace ALE.ACS.Presentation.Models.Output;

public enum OutputLogLevel
{
    Info,
    Warning,
    Error
}

public sealed class OutputLogEntry
{
    public OutputLogEntry(OutputLogLevel level, string message)
    {
        Timestamp = DateTime.Now;
        Level = level;
        Message = message;
    }

    public DateTime Timestamp { get; }

    public OutputLogLevel Level { get; }

    public string Message { get; }
}
