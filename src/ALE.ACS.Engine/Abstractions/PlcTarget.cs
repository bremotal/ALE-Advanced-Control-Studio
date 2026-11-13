namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Alvo de plataforma PLC para o qual o código será gerado.
/// </summary>
public enum PlcTarget
{
    /// <summary>Structured Text genérico IEC 61131-3. Compatível com CodeSys.</summary>
    GenericIec,

    /// <summary>Siemens S7-1200/1500 (TIA Portal). Variações em SCL.</summary>
    SiemensS7,

    /// <summary>Allen-Bradley ControlLogix / CompactLogix (RSLogix/Studio 5000).</summary>
    AllenBradley,

    /// <summary>Schneider Modicon M340/M580 (Unity Pro / Control Expert).</summary>
    SchneiderM
}
