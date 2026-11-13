// ============================================================================
//  ControllerApiShims — ponte entre os engines deste pacote e os contratos
//  reais do repo (namespace ALE.ACS.Engine.Core).
//
//  NÃO altera nenhuma classe existente. Apenas adiciona dois métodos de
//  extensão usados pelos engines novos (MPC, Adaptive MPC, Cascade, Smith,
//  Linearization, Optimization, ControllerFactory):
//    - ControllerInput.TryGet(name, out double)   — leitura por nome
//    - ControllerVariableCollection.Set(name, v)  — escrita de saída
//
//  A classe vive no namespace Core de propósito: todo arquivo de engine que
//  já faz `using ALE.ACS.Engine.Core;` enxerga as extensões sem editar usings.
// ============================================================================

namespace ALE.ACS.Engine.Core;

public static class ControllerApiShims
{
    /// <summary>
    /// Lê o valor double de uma variável do input pelo nome (match
    /// case-insensitive, igual ao TryGet da ControllerVariableCollection).
    /// Retorna false se a variável não estiver presente.
    /// </summary>
    public static bool TryGet(this ControllerInput input, string name, out double value)
    {
        if (input is not null && input.TryGetVariable(name, out var variable) && variable is not null)
        {
            value = variable.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Adiciona um valor de saída à coleção de variáveis do controller.
    /// Os engines criam uma coleção nova a cada ExecuteCore, então o padrão
    /// de uso é um Set por nome — implementado como Add dos contratos reais.
    /// Timestamp = agora; Quality = "GOOD" (dado produzido pelo engine).
    /// </summary>
    public static void Set(this ControllerVariableCollection collection, string name, double value)
    {
        collection.Add(new ControllerVariable(name, value, DateTime.UtcNow, "GOOD"));
    }
}
