namespace ALE.ACS.Engine.Optimization;

/// <summary>
/// Resolve um Quadratic Program de forma simples (sem restrições OU com restrições de caixa).
/// Implementa duas estratégias:
///  - Sem restrições: resolve o sistema linear (H + ρI) x = -g com ρ pequeno (regularização Levenberg-Marquardt).
///  - Com restrições de caixa (lb ≤ x ≤ ub): projeção + iteração de gradient projection (5-20 iterações).
/// </summary>
/// <remarks>
/// Formulação: min 0.5 * x' H x + g' x    s.t. lb ≤ x ≤ ub
/// Para MPC: H = 2*(S' Q S + R),  g = 2*S' Q (x̂ - x_ref),  onde S é a matriz de resposta ao passo.
/// Não é o solver mais eficiente do mundo; é correto e legível. Para PLC, o que se implanta
/// é a matriz de ganho K pré-computada offline (ver MpcDesign), não este solver on-line.
/// </remarks>
public sealed class QpSolver
{
    public QpSolution Solve(Matrix H, double[] g, double[]? lb = null, double[]? ub = null,
        int maxIter = 50, double tolerance = 1e-6)
    {
        var n = g.Length;
        if (H.Rows != n || H.Cols != n)
            throw new ArgumentException("Dimensões H/g incompatíveis.");

        var hasBox = lb is not null && ub is not null;
        if (hasBox && (lb!.Length != n || ub!.Length != n))
            throw new ArgumentException("Limites com tamanho errado.");

        // Regularização para garantir H positivo-definido
        var rho = 1e-6;
        var Hreg = H.Add(Matrix.Identity(n).Scale(rho));

        if (!hasBox)
        {
            // Sistema linear: x = -(H+ρI)^-1 g
            var x = Matrix.Solve(Hreg, g.Select(v => -v).ToArray());
            return new QpSolution(x, Iterations: 1, Optimal: true, Objective: ComputeObjective(H, g, x));
        }

        // Box-constrained: gradient projection
        var xCur = new double[n];
        // Inicializa no ponto médio da caixa (ou em 0)
        for (int i = 0; i < n; i++)
            xCur[i] = Math.Max(lb![i], Math.Min(ub![i], 0.0));

        double prevObj = double.PositiveInfinity;
        bool optimal = false;
        int iter;
        for (iter = 0; iter < maxIter; iter++)
        {
            // Gradiente = H x + g
            var grad = Hreg.MultiplyVector(xCur);
            for (int i = 0; i < n; i++) grad[i] += g[i];

            // Passo: x_new = x - α grad, com α = 1/||H||_inf (step conservador)
            double alpha = 1.0 / (1.0 + NormInf(Hreg));
            var xNew = new double[n];
            for (int i = 0; i < n; i++)
            {
                xNew[i] = xCur[i] - alpha * grad[i];
                // Projeta na caixa
                xNew[i] = Math.Max(lb![i], Math.Min(ub![i], xNew[i]));
            }

            var obj = ComputeObjective(H, g, xNew);
            if (Math.Abs(prevObj - obj) < tolerance * (1 + Math.Abs(obj)))
            {
                xCur = xNew;
                optimal = true;
                break;
            }
            xCur = xNew;
            prevObj = obj;
        }

        return new QpSolution(xCur, iter + 1, optimal, ComputeObjective(H, g, xCur));
    }

    private static double NormInf(Matrix M)
    {
        double max = 0;
        for (int i = 0; i < M.Rows; i++)
        {
            double s = 0;
            for (int j = 0; j < M.Cols; j++) s += Math.Abs(M[i, j]);
            if (s > max) max = s;
        }
        return max;
    }

    private static double ComputeObjective(Matrix H, double[] g, double[] x)
    {
        var Hx = H.MultiplyVector(x);
        double obj = 0;
        for (int i = 0; i < x.Length; i++) obj += 0.5 * x[i] * Hx[i] + g[i] * x[i];
        return obj;
    }
}

public sealed record QpSolution(double[] X, int Iterations, bool Optimal, double Objective);
