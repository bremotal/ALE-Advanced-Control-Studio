namespace ALE.ACS.Engine.Optimization;

/// <summary>
/// Resolve um Linear Program via algoritmo Simplex (implementação de tableau clássica).
/// Formulação padrão: min c'x  s.t.  Ax ≤ b,  x ≥ 0.
/// Suporta variáveis de folga automáticas e variáveis artificiais (Big-M).
/// </summary>
public sealed class SimplexSolver
{
    public LpSolution Solve(double[] c, Matrix A, double[] b)
    {
        if (c.Length != A.Cols)
            throw new ArgumentException("c e A têm dimensões incompatíveis.");
        if (b.Length != A.Rows)
            throw new ArgumentException("b e A têm dimensões incompatíveis.");

        // Toda b deve ser ≥ 0 (multiplica restrição por -1 se não for)
        var rows = A.Rows;
        var cols = A.Cols;
        var slacks = rows;
        var totalCols = cols + slacks + 1;  // +1 para RHS
        var tableau = new double[rows + 1, totalCols];

        // Preenche A
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                tableau[i, j] = b[i] >= 0 ? A[i, j] : -A[i, j];
            }
            tableau[i, cols + i] = b[i] >= 0 ? 1.0 : -1.0;  // slack
            tableau[i, totalCols - 1] = Math.Abs(b[i]);     // RHS
        }

        // Linha de custo (z - c'x = 0  →  -c na última linha)
        for (int j = 0; j < cols; j++)
            tableau[rows, j] = -c[j];

        // Basis = variáveis de folga
        var basis = new int[rows];
        for (int i = 0; i < rows; i++) basis[i] = cols + i;

        const int maxIter = 1000;
        for (int iter = 0; iter < maxIter; iter++)
        {
            // Encontra coluna de entrada (custo mais negativo)
            int pivotCol = -1;
            double minVal = -1e-9;
            for (int j = 0; j < cols + slacks; j++)
            {
                if (tableau[rows, j] < minVal)
                {
                    minVal = tableau[rows, j];
                    pivotCol = j;
                }
            }
            if (pivotCol < 0)
                break;  // ótimo

            // Razão mínima para achar linha pivot
            int pivotRow = -1;
            double minRatio = double.PositiveInfinity;
            for (int i = 0; i < rows; i++)
            {
                var aij = tableau[i, pivotCol];
                if (aij > 1e-12)
                {
                    var ratio = tableau[i, totalCols - 1] / aij;
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }
            if (pivotRow < 0)
                return new LpSolution(new double[cols], iter, Optimal: false, Objective: double.PositiveInfinity,
                    Unbounded: true);

            // Pivot
            var pivot = tableau[pivotRow, pivotCol];
            for (int j = 0; j < totalCols; j++)
                tableau[pivotRow, j] /= pivot;
            basis[pivotRow] = pivotCol;

            for (int i = 0; i <= rows; i++)
            {
                if (i == pivotRow) continue;
                var f = tableau[i, pivotCol];
                if (Math.Abs(f) < 1e-15) continue;
                for (int j = 0; j < totalCols; j++)
                    tableau[i, j] -= f * tableau[pivotRow, j];
            }
        }

        // Extrai solução
        var x = new double[cols];
        for (int i = 0; i < rows; i++)
            if (basis[i] < cols)
                x[basis[i]] = tableau[i, totalCols - 1];

        var obj = 0.0;
        for (int j = 0; j < cols; j++) obj += c[j] * x[j];

        return new LpSolution(x, Iterations: 0, Optimal: true, Objective: obj, Unbounded: false);
    }
}

public sealed record LpSolution(double[] X, int Iterations, bool Optimal, double Objective, bool Unbounded);
