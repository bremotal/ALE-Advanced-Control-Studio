namespace ALE.ACS.Engine.Optimization;

/// <summary>
/// Matriz densa row-major para álgebra linear usada pelos engines MPC e Optimization.
/// Implementação deliberadamente simples (sem BLAS) — foco em clareza e correção,
/// não em performance. Para problemas grandes (>50 variáveis), trocar por Math.NET.
/// </summary>
public sealed class Matrix
{
    private readonly double[] _data;
    public int Rows { get; }
    public int Cols { get; }

    public Matrix(int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
            throw new ArgumentException("Dimensões devem ser positivas.");
        Rows = rows; Cols = cols;
        _data = new double[rows * cols];
    }

    public double this[int i, int j]
    {
        get => _data[i * Cols + j];
        set => _data[i * Cols + j] = value;
    }

    public static Matrix Identity(int n)
    {
        var m = new Matrix(n, n);
        for (int i = 0; i < n; i++) m[i, i] = 1.0;
        return m;
    }

    public static Matrix Zeros(int rows, int cols) => new(rows, cols);

    public static Matrix FromRows(double[][] rows)
    {
        var r = rows.Length; var c = rows[0].Length;
        var m = new Matrix(r, c);
        for (int i = 0; i < r; i++)
            for (int j = 0; j < c; j++)
                m[i, j] = rows[i][j];
        return m;
    }

    public Matrix Transpose()
    {
        var t = new Matrix(Cols, Rows);
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Cols; j++)
                t[j, i] = this[i, j];
        return t;
    }

    public Matrix Multiply(Matrix other)
    {
        if (Cols != other.Rows)
            throw new ArgumentException("Dimensões incompatíveis para multiplicação.");
        var result = new Matrix(Rows, other.Cols);
        for (int i = 0; i < Rows; i++)
            for (int k = 0; k < Cols; k++)
            {
                var a = this[i, k];
                if (a == 0) continue;
                for (int j = 0; j < other.Cols; j++)
                    result[i, j] += a * other[k, j];
            }
        return result;
    }

    public Matrix Add(Matrix other)
    {
        if (Rows != other.Rows || Cols != other.Cols)
            throw new ArgumentException("Dimensões incompatíveis para soma.");
        var r = new Matrix(Rows, Cols);
        for (int i = 0; i < _data.Length; i++) r._data[i] = _data[i] + other._data[i];
        return r;
    }

    public Matrix Scale(double s)
    {
        var r = new Matrix(Rows, Cols);
        for (int i = 0; i < _data.Length; i++) r._data[i] = _data[i] * s;
        return r;
    }

    public double[] MultiplyVector(double[] v)
    {
        if (v.Length != Cols)
            throw new ArgumentException("Dimensão do vetor incompatível.");
        var result = new double[Rows];
        for (int i = 0; i < Rows; i++)
        {
            double s = 0;
            for (int j = 0; j < Cols; j++)
                s += this[i, j] * v[j];
            result[i] = s;
        }
        return result;
    }

    /// <summary>Resolve A x = b via eliminação de Gauss com pivotamento parcial.</summary>
    public static double[] Solve(Matrix A, double[] b)
    {
        if (A.Rows != A.Cols || A.Rows != b.Length)
            throw new ArgumentException("Sistema deve ser quadrado e compatível com b.");

        var n = A.Rows;
        // Cópia aumentada
        var M = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++) M[i, j] = A[i, j];
            M[i, n] = b[i];
        }

        // Eliminação com pivot
        for (int k = 0; k < n; k++)
        {
            // Pivot
            int p = k;
            for (int i = k + 1; i < n; i++)
                if (Math.Abs(M[i, k]) > Math.Abs(M[p, k])) p = i;
            if (Math.Abs(M[p, k]) < 1e-12)
                throw new InvalidOperationException("Matriz singular ou mal-condicionada.");
            if (p != k)
                for (int j = 0; j <= n; j++)
                    (M[k, j], M[p, j]) = (M[p, j], M[k, j]);

            // Eliminação
            for (int i = k + 1; i < n; i++)
            {
                var f = M[i, k] / M[k, k];
                for (int j = k; j <= n; j++)
                    M[i, j] -= f * M[k, j];
            }
        }

        // Substituição regressiva
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            var s = M[i, n];
            for (int j = i + 1; j < n; j++) s -= M[i, j] * x[j];
            x[i] = s / M[i, i];
        }
        return x;
    }

    public double[][] ToJaggedArray()
    {
        var r = new double[Rows][];
        for (int i = 0; i < Rows; i++)
        {
            r[i] = new double[Cols];
            for (int j = 0; j < Cols; j++) r[i][j] = this[i, j];
        }
        return r;
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
                sb.Append($"{this[i, j],10:F4} ");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
