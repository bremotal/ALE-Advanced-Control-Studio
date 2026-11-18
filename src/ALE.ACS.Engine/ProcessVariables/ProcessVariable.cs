namespace ALE.ACS.Engine.ProcessVariables;

/// <summary>
/// Variável de processo do usuário (PV, MV, SP, CV, DV, OP).
/// É o bloco atômico que os engines consomem para modelar o controlador
/// e o PLC generator usa para declarar VAR no Structured Text.
/// </summary>
public sealed class ProcessVariable
{
    /// <summary>Nome técnico (vira nome de variável no PLC, sanitizado).</summary>
    public string Name { get; }

    /// <summary>Papel da variável no controlador.</summary>
    public ProcessVariableKind Kind { get; }

    /// <summary>Unidade de engenharia.</summary>
    public EngineeringUnit Unit { get; }

    /// <summary>Limite inferior de engenharia (span baixo).</summary>
    public double Min { get; }

    /// <summary>Limite superior de engenharia (span alto).</summary>
    public double Max { get; }

    /// <summary>Valor nominal de operação (pode ser nulo).</summary>
    public double? Nominal { get; }

    public ProcessVariable(
        string name,
        ProcessVariableKind kind,
        EngineeringUnit unit,
        double min,
        double max,
        double? nominal = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (min >= max)
            throw new ArgumentException($"Min ({min}) deve ser menor que Max ({max}).", nameof(min));

        Name = name;
        Kind = kind;
        Unit = unit;
        Min = min;
        Max = max;
        Nominal = nominal;
    }

    /// <summary>Span de engenharia (Max - Min).</summary>
    public double Span => Max - Min;

    /// <summary>Normaliza um valor de engenharia para [0,1].</summary>
    public double Normalize(double value)
        => Span <= 0 ? 0 : (value - Min) / Span;

    /// <summary>Desnormaliza [0,1] para engenharia.</summary>
    public double Denormalize(double normalized)
        => Min + normalized * Span;

    /// <summary>Satura ao span de engenharia.</summary>
    public double Clamp(double value)
        => Math.Max(Min, Math.Min(Max, value));

    /// <summary>Sanitiza o nome para uso como identificador IEC 61131-3.</summary>
    public string SanitizedName
    {
        get
        {
            var sb = new System.Text.StringBuilder(Name.Length);
            foreach (var c in Name)
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            if (sb.Length > 0 && char.IsDigit(sb[0]))
                sb.Insert(0, '_');
            return sb.ToString();
        }
    }

    public override string ToString()
        => $"{Name} [{Kind}] {Min}..{Max} {Unit.Symbol}";
}
