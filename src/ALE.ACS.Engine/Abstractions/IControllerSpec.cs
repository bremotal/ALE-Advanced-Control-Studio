using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Especificação imutável de um controlador. É o input de design (gera parâmetros)
/// e de geração de código PLC. Cada engine concreto tem uma subclasse concreta
/// (<see cref="ALE.ACS.Engine.Mpc.MpcSpec"/>, <see cref="ALE.ACS.Engine.Cascade.CascadeSpec"/>, etc.).
/// </summary>
public interface IControllerSpec
{
    /// <summary>Tipo do controlador — despacha factory e PLC generator.</summary>
    ControllerKind Kind { get; }

    /// <summary>Nome legível do controlador (vira FUNCTION_BLOCK name no PLC).</summary>
    string Name { get; }

    /// <summary>Setpoint principal (variável alvo).</summary>
    ProcessVariable Setpoint { get; }

    /// <summary>Variável manipulada (saída do controlador).</summary>
    ProcessVariable Manipulated { get; }

    /// <summary>Variável controlada (medição).</summary>
    ProcessVariable Controlled { get; }

    /// <summary>Distúrbios medidos considerados (feedforward).</summary>
    IReadOnlyList<ProcessVariable> Disturbances { get; }

    /// <summary>Período de varredura (segundos). Vira T#50ms no ST.</summary>
    TimeSpan ScanTime { get; }

    /// <summary>Resultado do design offline (parâmetros pré-computados).</summary>
    /// <remarks>
    /// É <see cref="IControllerDesign"/> genérico porque cada engine produz um design
    /// de tipo próprio. Implementações retornam o concreto.
    /// </remarks>
    IControllerDesign Design();
}
