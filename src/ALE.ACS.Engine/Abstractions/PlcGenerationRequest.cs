using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Pedido de geração de código PLC a partir de uma spec de controlador.
/// </summary>
public sealed class PlcGenerationRequest
{
    /// <summary>Especificação do controlador a ser traduzida em código.</summary>
    public required IControllerSpec Spec { get; init; }

    /// <summary>Plataforma alvo (Siemens, AB, CodeSys...).</summary>
    public required PlcTarget Target { get; init; }

    /// <summary>Prefixo de nome para FUNCTION_BLOCK / PROGRAM.</summary>
    public string BlockPrefix { get; init; } = "CTRL";

    /// <summary>Indica se deve gerar a variável de instância junto do FB.</summary>
    public bool EmitInstance { get; init; } = true;

    /// <summary>Indica se deve emitir comentários descritivos no ST.</summary>
    public bool EmitComments { get; init; } = true;

    /// <summary>Indica se deve alinhar valores de sintonia como constantes.</summary>
    public bool EmitTuningAsConstants { get; init; } = true;

    /// <summary>
    /// Variáveis adicionais (além de PV/MV/SP/CV/DV) a declarar na interface do bloco.
    /// </summary>
    public IReadOnlyList<ProcessVariable> ExtraVariables { get; init; } = [];
}
