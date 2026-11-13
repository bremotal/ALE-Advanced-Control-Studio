namespace ALE.ACS.Engine.ProcessVariables;

/// <summary>
/// Papel de uma variável de processo no contexto do controlador.
/// </summary>
public enum ProcessVariableKind
{
    /// <summary>Process Variable — medição da planta (saída do processo).</summary>
    PV,

    /// <summary>Manipulated Variable — saída do controlador para o atuador.</summary>
    MV,

    /// <summary>Setpoint — valor desejado para a variável controlada.</summary>
    SP,

    /// <summary>Controlled Variable — variável que se quer manter em SP.</summary>
    CV,

    /// <summary>Disturbance Variable — medição de distúrbio para feedforward.</summary>
    DV,

    /// <summary>Output — valor final enviado ao atuador (pós-limites/clamping).</summary>
    OP
}
