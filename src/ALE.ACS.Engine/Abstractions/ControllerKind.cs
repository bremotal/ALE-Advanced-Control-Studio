namespace ALE.ACS.Engine.Abstractions;

/// <summary>
/// Identifica o tipo de controlador. Usado pela <see cref="IControllerFactory"/>
/// para despachar a construção e pelo <see cref="IPlcCodeGenerator"/> para
/// selecionar o template de código.
/// </summary>
public enum ControllerKind
{
    /// <summary>Lógica fuzzy (Mamdani/Sugeno). Já existe no repo.</summary>
    Fuzzy,

    /// <summary>Model Predictive Control com QP.</summary>
    Mpc,

    /// <summary>MPC adaptativo com identificação RLS.</summary>
    AdaptiveMpc,

    /// <summary>Cascade master/slave.</summary>
    Cascade,

    /// <summary>Smith predictor para processos com tempo morto.</summary>
    SmithPredictor,

    /// <summary>Linearização por realimentação.</summary>
    Linearization,

    /// <summary>Otimização LP/QP standalone.</summary>
    Optimization,

    /// <summary>PID clássico (single loop). Usado por Cascade e Smith internamente.</summary>
    Pid
}
