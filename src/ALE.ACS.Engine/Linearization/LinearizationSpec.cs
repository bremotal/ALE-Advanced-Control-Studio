using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Core;
using ALE.ACS.Engine.Optimization;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Linearization;

/// <summary>
/// Spec para linearização por realimentação. O sistema tem a forma:
///   ẋ = f(x) + g(x) u
///   y = h(x)
/// A lei de controle u = (v - f(x)) / g(x) lineariza entrada-saída, onde v é a
/// nova entrada (tipicamente a saída de um PID externo sobre y - SP).
/// </summary>
public sealed class LinearizationSpec : IControllerSpec
{
    public required string Name { get; init; }
    public required ProcessVariable Setpoint { get; init; }
    public required ProcessVariable Manipulated { get; init; }
    public required ProcessVariable Controlled { get; init; }
    public IReadOnlyList<ProcessVariable> Disturbances { get; init; } = [];
    public required TimeSpan ScanTime { get; init; }

    /// <summary>Função f(x) da dinâmica — retorna o vetor ẋ_drift (sem entrada).</summary>
    public required Func<double[], double[]> F { get; init; }

    /// <summary>Função g(x) — retorna a matriz n×m onde ẋ = f + g·u.</summary>
    public required Func<double[], Matrix> G { get; init; }

    /// <summary>Função h(x) — saída medida.</summary>
    public required Func<double[], double> H { get; init; }

    /// <summary>Jacobiana de h em relação a x (∂h/∂x). Necessária para linearização exata.</summary>
    public Func<double[], double[]>? DhDx { get; init; }

    /// <summary>Estado nominal x₀ em torno do qual linearizar (para feedback linearization clássica).</summary>
    public double[]? NominalState { get; init; }

    /// <summary>Ganho da nova entrada v (geralmente 1 — v é a saída de um PID externo).</summary>
    public double InputGain { get; init; } = 1.0;

    /// <summary>Limites da MV.</summary>
    public double OutMin { get; init; } = -1e6;
    public double OutMax { get; init; } = 1e6;

    /// <summary>Tipo de linearização.</summary>
    public LinearizationMode Mode { get; init; } = LinearizationMode.Feedback;

    public ControllerKind Kind => ControllerKind.Linearization;

    public IControllerDesign Design()
    {
        var n = NominalState?.Length ?? 0;
        if (Mode == LinearizationMode.Exact && DhDx is null)
            return new LinearizationDesign(false, "Linearização exata requer DhDx (Jacobiana de h).", null, null, null);

        // Valida F, G, H em torno do ponto nominal
        try
        {
            var x0 = NominalState ?? new double[] { 0 };
            var fx = F(x0);
            var gx = G(x0);
            if (fx.Length != x0.Length || gx.Rows != x0.Length)
                return new LinearizationDesign(false, "Dimensões de f/g incompatíveis com estado.", null, null, null);
            var hx = H(x0);
            // Para exata: Lg h = DhDx · g(x), deve ser não-nulo
            if (Mode == LinearizationMode.Exact && DhDx is not null)
            {
                var dh = DhDx(x0);
                var Lgh = 0.0;
                for (int i = 0; i < dh.Length; i++) Lgh += dh[i] * gx[i, 0];
                if (Math.Abs(Lgh) < 1e-12)
                    return new LinearizationDesign(false, "Lg h ≈ 0 — sistema não linearizável neste ponto.", null, null, null);
            }
            var summary = $"Linearização ({Mode}): n={x0.Length}, h(x0)={hx:F3}.";
            return new LinearizationDesign(true, summary, NominalState, F, G);
        }
        catch (Exception ex)
        {
            return new LinearizationDesign(false, $"Falha no design: {ex.Message}", null, null, null);
        }
    }
}

public enum LinearizationMode
{
    /// <summary>Linearização por realimentação: u = (v - f(x)) / g(x).</summary>
    Feedback,

    /// <summary>Linearização exata entrada-saída (geometric): usa Lie derivatives.</summary>
    Exact
}

public sealed record LinearizationDesign(
    bool IsValid,
    string Summary,
    double[]? NominalState,
    Func<double[], double[]>? F,
    Func<double[], Matrix>? G) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}
