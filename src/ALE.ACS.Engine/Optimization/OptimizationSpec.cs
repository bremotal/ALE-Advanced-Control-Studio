using ALE.ACS.Engine.Abstractions;
using ALE.ACS.Engine.ProcessVariables;

namespace ALE.ACS.Engine.Optimization;

/// <summary>
/// Spec para um problema de otimização LP/QP standalone.
/// É o caso de uso mais simples: min 0.5 x' H x + c' x  s.t.  A x ≤ b, lb ≤ x ≤ ub.
/// </summary>
public sealed class OptimizationSpec : IControllerSpec
{
    public required string Name { get; init; }
    public required ProcessVariable Setpoint { get; init; }
    public required ProcessVariable Manipulated { get; init; }
    public required ProcessVariable Controlled { get; init; }
    public IReadOnlyList<ProcessVariable> Disturbances { get; init; } = [];
    public required TimeSpan ScanTime { get; init; }

    /// <summary>Matriz H (quadrática). Se nula, é LP puro.</summary>
    public Matrix? H { get; init; }

    /// <summary>Vetor de custo linear c.</summary>
    public required double[] C { get; init; }

    /// <summary>Matriz de restrições A (Ax ≤ b).</summary>
    public Matrix? A { get; init; }

    /// <summary>Vetor b das restrições.</summary>
    public double[]? B { get; init; }

    /// <summary>Limites inferiores das variáveis.</summary>
    public double[]? LowerBounds { get; init; }

    /// <summary>Limites superiores das variáveis.</summary>
    public double[]? UpperBounds { get; init; }

    public ControllerKind Kind => ControllerKind.Optimization;

    public IControllerDesign Design()
    {
        var n = C.Length;
        if (H is not null && (H.Rows != n || H.Cols != n))
            return new OptimizationDesign(false, "Dimensões H incompatíveis com c.");

        if (A is not null && B is not null)
        {
            if (A.Cols != n || A.Rows != B.Length)
                return new OptimizationDesign(false, "A/B com dimensões incompatíveis.");
        }

        // Tenta resolver uma vez offline para validar
        try
        {
            double[] x;
            if (H is not null)
            {
                var qp = new QpSolver();
                var sol = qp.Solve(H, C, LowerBounds, UpperBounds);
                x = sol.X;
            }
            else
            {
                var lp = new SimplexSolver();
                var sol = lp.Solve(C, A!, B!);
                x = sol.X;
            }
            return new OptimizationDesign(true, $"Otimização resolvida offline. n={n}, ótimo={string.Join(",", x.Select(v => v.ToString("F3")))}");
        }
        catch (Exception ex)
        {
            return new OptimizationDesign(false, $"Falha ao resolver offline: {ex.Message}");
        }
    }
}

public sealed record OptimizationDesign(bool IsValid, string Summary) : IControllerDesign
{
    bool IControllerDesign.IsValid => IsValid;
    string IControllerDesign.Summary => Summary;
}
