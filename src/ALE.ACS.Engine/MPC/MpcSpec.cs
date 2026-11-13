using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.Optimization;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Mpc;

/// <summary>
/// Especificação de um controlador MPC.
/// </summary>
public sealed class MpcSpec : IControllerSpec
{
    public required string Name { get; init; }
    public required ProcessVariable Setpoint { get; init; }
    public required ProcessVariable Manipulated { get; init; }
    public required ProcessVariable Controlled { get; init; }
    public IReadOnlyList<ProcessVariable> Disturbances { get; init; } = [];
    public required TimeSpan ScanTime { get; init; }

    /// <summary>Modelo discreto do processo (A, B, C).</summary>
    public required ProcessModel Model { get; init; }

    /// <summary>Horizonte de predição (passos).</summary>
    public int PredictionHorizon { get; init; } = 20;

    /// <summary>Horizonte de controle (passos; ≤ PredictionHorizon).</summary>
    public int ControlHorizon { get; init; } = 5;

    /// <summary>Peso da variação da saída em relação ao SP (Q — diagonal).</summary>
    public double OutputWeight { get; init; } = 1.0;

    /// <summary>Peso da variação da MV (R — diagonal, penaliza agressividade).</summary>
    public double InputWeight { get; init; } = 0.1;

    /// <summary>Peso da taxa de variação da MV (Δu).</summary>
    public double InputRateWeight { get; init; } = 0.0;

    /// <summary>Limite inferior da MV.</summary>
    public double InputMin { get; init; } = -1e6;

    /// <summary>Limite superior da MV.</summary>
    public double InputMax { get; init; } = 1e6;

    /// <summary>Limite inferior da taxa de variação da MV.</summary>
    public double InputRateMin { get; init; } = -1e6;

    /// <summary>Limite superior da taxa de variação da MV.</summary>
    public double InputRateMax { get; init; } = 1e6;

    public ControllerKind Kind => ControllerKind.Mpc;

    public IControllerDesign Design()
    {
        if (PredictionHorizon <= 0 || ControlHorizon <= 0)
            return new MpcDesign(false, "H/P devem ser > 0.", null, null, null);
        if (ControlHorizon > PredictionHorizon)
            return new MpcDesign(false, "ControlHorizon não pode exceder PredictionHorizon.", null, null, null);

        try
        {
            var (S, K) = BuildPredictionAndGain();
            var summary = $"MPC: Hp={PredictionHorizon}, Hu={ControlHorizon}, " +
                           $"n={Model.States} estados, MV=[{InputMin:F1},{InputMax:F1}]. " +
                           $"Gain matrix K ({K.Rows}×{K.Cols}) pré-computada.";
            return new MpcDesign(true, summary, S, K, Model);
        }
        catch (Exception ex)
        {
            return new MpcDesign(false, $"Falha no design: {ex.Message}", null, null, null);
        }
    }

    /// <summary>
    /// Constrói a matriz de predição S (Hp × Hu) e a matriz de ganho K (Hu × n_states).
    /// K é a solução do QP sem restrições, pré-computada offline para implantação em PLC.
    /// </summary>
    private (Matrix S, Matrix K) BuildPredictionAndGain()
    {
        var n = Model.States;
        var m = Model.Inputs;   // MVs
        var Hp = PredictionHorizon;
        var Hu = ControlHorizon;

        // Matriz de resposta ao passo (predictor): prediz y(k+i) = sum_{j=1..min(i,Hu)} S_i-j * Δu(k+j) + free_response
        // Para SISO simples, S é Hp × Hu.
        // Construímos via simulação do modelo livre (x(k+i) = A^i x0 + sum A^(i-j-1) B u_j)
        var S = new Matrix(Hp, Hu);
        for (int j = 0; j < Hu; j++)
        {
            // Para cada coluna j (corresponde a um pulso unitário em Δu na posição j)
            var x = new double[n];
            for (int i = 0; i < Hp; i++)
            {
                // Aplica u_j = 1 nas primeiras Hu amostras
                var u = new double[m];
                if (i >= j) u[0] = 1.0;  // SISO: assume m=1
                // Passo: x(k+1) = A x + B u
                var Ax = Model.A.MultiplyVector(x);
                var Bu = Model.B.MultiplyVector(u);
                x = Ax.Zip(Bu, (a, b) => a + b).ToArray();
                var y = Model.C.MultiplyVector(x);
                S[i, j] = y[0];
            }
        }

        // H = 2 * (S' Q S + R + Δu' R_rate Δu)
        // Q: Hp × Hp (diagonal OutputWeight), R: Hu × Hu (diagonal InputWeight)
        // Para SISO, simplifica para:
        var StS = S.Transpose().Multiply(S);   // Hu × Hu
        var H = StS.Scale(2.0 * OutputWeight);
        for (int i = 0; i < Hu; i++)
            H[i, i] += 2.0 * (InputWeight + InputRateWeight);

        // K = H^-1 S' Q  (matriz de ganho ótima sem restrições, Hu × Hp ou reduzida)
        // Para implantação em PLC: K * (x_ref - x̂) = Δu ótimo (vetor Hu), usa-se apenas o primeiro.
        var St = S.Transpose();
        // K = H^-1 * S' * Q  (Q = OutputWeight * I)
        // Resolvemos coluna a coluna: K col_j = H^-1 * (S' * Q * e_j) = H^-1 * OutputWeight * S'_row_j
        // Mais simples: K = H^-1 * S'  (assumindo Q=I*OutputWeight, fold em H)
        var K = new Matrix(Hu, n + 1);  // ganho: Hu × (n_states + 1, com termo de setpoint)
        // Para SISO e SP constante: Δu = -K (x̂ - x_ref). Implementamos K_lin = H^-1 * S' * C_concat.
        // Aqui simplificamos: K_lin tal que Δu(k) = -K_lin * (x̂(k) - x_ref)
        // K_lin = (S' S + ρI)^-1 S'  →  primeira linha de K_lin dá o ganho do MPC.

        // Constrói pseudo-H = 2*(S' Q S + R) e resolve K_lin = H^-1 * 2 * Q * S
        // (sem fator 2 pq vai cancelar com o gradiente)
        var Hreg = H.Add(Matrix.Identity(Hu).Scale(1e-6));
        for (int j = 0; j < n + 1; j++)
        {
            double[] col;
            if (j < n)
            {
                // col_j de (S' * Q * C_extend), onde C_extend = [C; 0...0]
                // Para SISO: a contribuição de x̂[j] para y(k+i) é C A^j * x̂
                col = new double[Hu];
                for (int r = 0; r < Hu; r++)
                {
                    // S[r, k] * contribuição de x̂[j] na predição y(k+r+1)
                    double contrib = 0;
                    var xState = new double[n];
                    xState[j] = 1.0;
                    var ypred = Model.C.MultiplyVector(Model.A.MultiplyVector(xState));
                    contrib = ypred[0];
                    col[r] = contrib * OutputWeight;
                }
            }
            else
            {
                // Coluna do SP: contribuição -1 em cada y(k+i)
                col = new double[Hu];
                for (int r = 0; r < Hu; r++) col[r] = -OutputWeight;
            }
            var kcol = Matrix.Solve(Hreg, col);
            for (int i = 0; i < Hu; i++) K[i, j] = kcol[i];
        }

        return (S, K);
    }
}

public sealed record MpcDesign(
    bool IsValid,
    string Summary,
    Matrix? S,
    Matrix? K,
    ProcessModel? Model) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}
