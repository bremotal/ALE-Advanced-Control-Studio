namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Resultado da geração de código PLC.
/// </summary>
public sealed class PlcGenerationResult
{
    /// <summary>True se a geração foi bem-sucedida.</summary>
    public bool Success { get; init; }

    /// <summary>Nome do bloco gerado (ex.: "CTRL_Fuzzy_Temp").</summary>
    public string BlockName { get; init; } = string.Empty;

    /// <summary>Código Structured Text completo.</summary>
    public string StructuredText { get; init; } = string.Empty;

    /// <summary>Mensagens de aviso durante a geração.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>Mensagens de erro (quando <see cref="Success"/> = false).</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>Plataforma alvo efetivamente gerada.</summary>
    public PlcTarget Target { get; init; }

    public static PlcGenerationResult Ok(string blockName, string st, PlcTarget target, IReadOnlyList<string>? warnings = null)
        => new()
        {
            Success = true,
            BlockName = blockName,
            StructuredText = st,
            Target = target,
            Warnings = warnings ?? []
        };

    public static PlcGenerationResult Fail(string error, PlcTarget target)
        => new()
        {
            Success = false,
            Errors = [error],
            Target = target
        };
}
