using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Mpc;
using ALE.ACS.Engine.Optimization;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.AdaptiveMpc;

/// <summary>
/// Spec para Adaptive MPC. Estende <see cref="MpcSpec"/> com parâmetros do RLS
/// que identifica o modelo online.
/// </summary>
public sealed class AdaptiveMpcSpec : IControllerSpec
{
    public required string Name { get; init; }
    public required ProcessVariable Setpoint { get; init; }
    public required ProcessVariable Manipulated { get; init; }
    public required ProcessVariable Controlled { get; init; }
    public IReadOnlyList<ProcessVariable> Disturbances { get; init; } = [];
    public required TimeSpan ScanTime { get; init; }

    /// <summary>Modelo inicial (será atualizado pelo RLS em runtime).</summary>
    public required ProcessModel InitialModel { get; init; }

    /// <summary>Ordem do modelo ARX (na + nb). Para SISO FOPDT, usar 2.</summary>
    public int ArxOrder { get; init; } = 2;

    /// <summary>Forgetting factor λ do RLS (0.95 - 0.999).</summary>
    public double ForgettingFactor { get; init; } = 0.99;

    /// <summary>
    /// Threshold: se a mudança relativa nos parâmetros do modelo exceder este valor,
    /// re-desenha o QP. Evita re-design desnecessário a cada scan.
    /// </summary>
    public double RedesignThreshold { get; init; } = 0.05;

    public int PredictionHorizon { get; init; } = 20;
    public int ControlHorizon { get; init; } = 5;
    public double OutputWeight { get; init; } = 1.0;
    public double InputWeight { get; init; } = 0.1;
    public double InputRateWeight { get; init; } = 0.0;
    public double InputMin { get; init; } = -1e6;
    public double InputMax { get; init; } = 1e6;
    public double InputRateMin { get; init; } = -1e6;
    public double InputRateMax { get; init; } = 1e6;

    public ControllerKind Kind => ControllerKind.AdaptiveMpc;

    public IControllerDesign Design()
    {
        // Design inicial: usa o InitialModel como base.
        var mpcSpec = BuildMpcSpec(InitialModel);
        var design = (MpcDesign)mpcSpec.Design();
        return new AdaptiveMpcDesign(design.IsValid,
            $"Adaptive MPC: λ={ForgettingFactor:F3}, ArxOrder={ArxOrder}, " +
            $"redesign@{RedesignThreshold:P0}. " + design.Summary,
            design.K);
    }

    /// <summary>Constrói uma MpcSpec a partir de um modelo (para re-design em runtime).</summary>
    public MpcSpec BuildMpcSpec(ProcessModel model)
    {
        return new MpcSpec
        {
            Name = Name,
            Setpoint = Setpoint,
            Manipulated = Manipulated,
            Controlled = Controlled,
            Disturbances = Disturbances,
            ScanTime = ScanTime,
            Model = model,
            PredictionHorizon = PredictionHorizon,
            ControlHorizon = ControlHorizon,
            OutputWeight = OutputWeight,
            InputWeight = InputWeight,
            InputRateWeight = InputRateWeight,
            InputMin = InputMin,
            InputMax = InputMax,
            InputRateMin = InputRateMin,
            InputRateMax = InputRateMax
        };
    }
}

public sealed record AdaptiveMpcDesign(
    bool IsValid,
    string Summary,
    Matrix? K) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}
