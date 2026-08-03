namespace ALE.ACS.Engine.Core;

public sealed class ControllerVariable
{
    public ControllerVariable(string name, double value, DateTime timestamp, string quality)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(quality))
            throw new ArgumentException("Variable quality is required.", nameof(quality));

        Name = name;
        Value = value;
        Timestamp = timestamp;
        Quality = quality;
    }

    public string Name { get; }

    public double Value { get; }

    public DateTime Timestamp { get; }

    public string Quality { get; }
}