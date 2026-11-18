namespace ALE.ACS.Engine.ProcessVariables;

/// <summary>
/// Unidade de engenharia de uma variável de processo.
/// </summary>
/// <remarks>
/// Suporta as unidades comuns em automação industrial: %, °C, bar, m³/h, etc.
/// Para unidades não listadas, use <see cref="Custom"/> e informe o símbolo.
/// </remarks>
public readonly struct EngineeringUnit : IEquatable<EngineeringUnit>
{
    public string Symbol { get; }
    public string Description { get; }

    private EngineeringUnit(string symbol, string description)
    {
        Symbol = symbol;
        Description = description;
    }

    public static readonly EngineeringUnit Dimensionless = new("-", "Sem unidade");
    public static readonly EngineeringUnit Percent = new("%", "Percentual da escala");
    public static readonly EngineeringUnit Celsius = new("°C", "Graus Celsius");
    public static readonly EngineeringUnit Fahrenheit = new("°F", "Graus Fahrenheit");
    public static readonly EngineeringUnit Kelvin = new("K", "Kelvin");
    public static readonly EngineeringUnit Pascal = new("Pa", "Pascal");
    public static readonly EngineeringUnit Hectopascal = new("hPa", "Hectopascal");
    public static readonly EngineeringUnit Bar = new("bar", "Bar");
    public static readonly EngineeringUnit Psi = new("psi", "Libra por polegada quadrada");
    public static readonly EngineeringUnit CubicMeterPerHour = new("m³/h", "Metro cúbico por hora");
    public static readonly EngineeringUnit LiterPerSecond = new("L/s", "Litro por segundo");
    public static readonly EngineeringUnit MeterPerSecond = new("m/s", "Metro por segundo");
    public static readonly EngineeringUnit Rpm = new("rpm", "Rotações por minuto");
    public static readonly EngineeringUnit Ampere = new("A", "Amper");
    public static readonly EngineeringUnit Volt = new("V", "Volt");
    public static readonly EngineeringUnit Watt = new("W", "Watt");
    public static readonly EngineeringUnit Kilowatt = new("kW", "Quilowatt");

    /// <summary>Cria uma unidade customizada quando a padrão não atende.</summary>
    public static EngineeringUnit Custom(string symbol, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        return new EngineeringUnit(symbol, description);
    }

    public override string ToString() => Symbol;
    public bool Equals(EngineeringUnit other) => Symbol == other.Symbol;
    public override bool Equals(object? obj) => obj is EngineeringUnit u && Equals(u);
    public override int GetHashCode() => Symbol.GetHashCode();
    public static bool operator ==(EngineeringUnit a, EngineeringUnit b) => a.Equals(b);
    public static bool operator !=(EngineeringUnit a, EngineeringUnit b) => !a.Equals(b);
}
