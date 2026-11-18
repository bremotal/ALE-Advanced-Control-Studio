using ALE.ACS.Engine.Abstractions;

namespace ALE.ACS.Engine.PlcCodeGen;

/// <summary>
/// Quirks por fabricante de PLC. Os geradores consultam este helper para
/// emitir a sintaxe correta da plataforma alvo.
/// </summary>
public static class PlcPlatformQuirks
{
    /// <summary>Sufixo do tipo de dados REAL/LREAL por plataforma.</summary>
    public static string RealType(PlcTarget target, int precision = 32)
    {
        return (target, precision) switch
        {
            (PlcTarget.SiemensS7, 32) => "REAL",
            (PlcTarget.SiemensS7, 64) => "LREAL",
            (PlcTarget.AllenBradley, _) => "REAL",
            (PlcTarget.SchneiderM, _) => "REAL",
            _ => "REAL"
        };
    }

    /// <summary>Sufixo do tipo inteiro.</summary>
    public static string IntType(PlcTarget target, int bits = 16)
    {
        return (target, bits) switch
        {
            (PlcTarget.SiemensS7, 16) => "INT",
            (PlcTarget.SiemensS7, 32) => "DINT",
            (PlcTarget.SiemensS7, 64) => "LINT",
            (PlcTarget.AllenBradley, _) => "DINT",
            (PlcTarget.SchneiderM, 16) => "INT",
            (PlcTarget.SchneiderM, 32) => "DINT",
            _ => "INT"
        };
    }

    /// <summary>Sufixo de tipo para tempo (TIME).</summary>
    public static string TimeType(PlcTarget target) => target == PlcTarget.AllenBradley ? "TIME" : "TIME";

    /// <summary>Sintaxe de declaração de variável com valor inicial.</summary>
    public static string VarDecl(PlcTarget target, string name, string type, string? init = null)
    {
        var id = StructuredTextWriter.Id(name);
        return init is null
            ? $"{id} : {type};"
            : $"{id} : {type} := {init};";
    }

    /// <summary>Palavra-chave de bloco FUNCTION_BLOCK vs PROGRAM.</summary>
    public static string BlockKeyword(PlcTarget target, bool asProgram)
        => asProgram ? "PROGRAM" : "FUNCTION_BLOCK";

    /// <summary>Sufixo do fechamento do bloco.</summary>
    public static string EndBlockKeyword(PlcTarget target, bool asProgram)
        => asProgram ? "END_PROGRAM" : "END_FUNCTION_BLOCK";

    /// <summary>Indica se a plataforma suporta array nativo (todos suportam, mas Siemens usa ARRAY[lo..hi]).</summary>
    public static string ArraySyntax(PlcTarget target, int lo, int hi, string elementType)
        => target == PlcTarget.SiemensS7
            ? $"ARRAY[{lo}..{hi}] OF {elementType}"
            : $"ARRAY[{lo}..{hi}] OF {elementType}";
}
