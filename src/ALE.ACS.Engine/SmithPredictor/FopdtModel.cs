namespace ALE.ACS.Engine.SmithPredictor;

/// <summary>
/// Modelo First-Order Plus Dead Time (FOPDT) — o modelo canônico para processo com atraso.
///   G(s) = K * exp(-θ s) / (τ s + 1)
/// </summary>
/// <remarks>
/// Parâmetros típicos identificados por teste de degrau:
///   K  = ganho estático (ΔPV/ΔMV)
///   τ  = constante de tempo (tempo para 63.2% da resposta final)
///   θ  = tempo morto (atraso de transporte / instrumentação)
/// O Smith Predictor compensa o θ separando o modelo em parte sem atraso (τ, K) e parte atrasada.
/// </summary>
public sealed class FopdtModel
{
    public double K { get; }       // ganho estático
    public double Tau { get; }     // constante de tempo (segundos)
    public double Theta { get; }  // tempo morto (segundos)
    public double Ts { get; }     // período de amostragem (segundos)
    public string Name { get; }

    /// <summary>Número de amostras equivalentes ao tempo morto (arredondado).</summary>
    public int DeadTimeSamples => (int)Math.Round(Theta / Ts);

    public FopdtModel(string name, double k, double tau, double theta, double ts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (k == 0) throw new ArgumentException("K não pode ser zero.", nameof(k));
        if (tau <= 0) throw new ArgumentException("τ deve ser positivo.", nameof(tau));
        if (theta < 0) throw new ArgumentException("θ não pode ser negativo.", nameof(theta));
        if (ts <= 0) throw new ArgumentException("Ts deve ser positivo.", nameof(ts));

        Name = name;
        K = k;
        Tau = tau;
        Theta = theta;
        Ts = ts;
    }

    /// <summary>
    /// Discretiza o modelo FOPDT por Euler backward, sem o tempo morto.
    /// Resultado: y(k) = a * y(k-1) + b * u(k-d), onde d = DeadTimeSamples.
    /// </summary>
    public (double a, double b, int d) Discretize()
    {
        var a = 1.0 - Ts / Tau;
        var b = K * Ts / Tau;
        var d = DeadTimeSamples;
        return (a, b, d);
    }

    /// <summary>Simula um passo do modelo (sem predição — só propaga estado).</summary>
    public double Step(double[] stateHistory, double uPrevDelayed)
    {
        var (a, b, d) = Discretize();
        if (stateHistory.Length == 0) return 0.0;
        // y(k) = a * y(k-1) + b * u(k-d)
        return a * stateHistory[^1] + b * uPrevDelayed;
    }

    public override string ToString() => $"FOPDT(K={K:F3}, τ={Tau:F2}s, θ={Theta:F2}s, Ts={Ts:F3}s)";
}
