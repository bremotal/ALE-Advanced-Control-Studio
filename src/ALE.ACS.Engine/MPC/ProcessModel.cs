using ALE.ACS.Engine.Optimization;

namespace ALE.ACS.Engine.Mpc;

/// <summary>
/// Modelo discreto de processo no espaço de estados:
///   x(k+1) = A x(k) + B u(k)
///   y(k)   = C x(k)
/// Com Ts já embutido nas matrizes A, B, C.
/// </summary>
/// <remarks>
/// Para MPC, este modelo é tipicamente identificado offline (subsistema ARX, N4SID)
/// ou derivado de um modelo fenomenológico linearizado. O Adaptive MPC atualiza
/// A, B em runtime via RLS a partir de (u, y) observados.
/// </remarks>
public sealed class ProcessModel
{
    public string Name { get; }
    public double Ts { get; }  // período de amostragem (segundos)
    public int States { get; }
    public int Inputs { get; }
    public int Outputs { get; }

    public Matrix A { get; set; }
    public Matrix B { get; set; }
    public Matrix C { get; set; }

    /// <summary>Estado atual estimado x̂.</summary>
    public double[] State { get; set; }

    public ProcessModel(string name, double ts, Matrix a, Matrix b, Matrix c)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (ts <= 0) throw new ArgumentException("Ts deve ser positivo.", nameof(ts));

        Name = name;
        Ts = ts;
        States = a.Rows;
        Inputs = b.Cols;
        Outputs = c.Rows;

        if (a.Rows != a.Cols) throw new ArgumentException("A deve ser quadrada.");
        if (b.Rows != States) throw new ArgumentException("B deve ter mesmo número de linhas que A.");
        if (c.Cols != States) throw new ArgumentException("C deve ter mesma número de colunas que A.");

        A = a; B = b; C = c;
        State = new double[States];
    }

    /// <summary>Avança o modelo por um passo. Atualiza <see cref="State"/> e retorna a saída y.</summary>
    public double[] Step(double[] u)
    {
        if (u.Length != Inputs)
            throw new ArgumentException($"Entrada deve ter {Inputs} elementos.", nameof(u));

        // x(k+1) = A x(k) + B u(k)
        var Ax = A.MultiplyVector(State);
        var Bu = B.MultiplyVector(u);
        var xNext = new double[States];
        for (int i = 0; i < States; i++)
            xNext[i] = Ax[i] + Bu[i];

        State = xNext;

        // y(k) = C x(k)
        return C.MultiplyVector(State);
    }

    /// <summary>Reseta o estado interno.</summary>
    public void Reset()
    {
        Array.Clear(State, 0, State.Length);
    }

    /// <summary>Cria um modelo SISO de 1ª ordem discretizado (FOPDT aproximado sem atraso).</summary>
    public static ProcessModel FirstOrder(double gain, double tauSec, double tsSec, string name = "fopdt")
    {
        // Discretização por Euler backward: a = 1 - Ts/τ, b = K * Ts/τ
        var a11 = 1.0 - tsSec / tauSec;
        var b11 = gain * tsSec / tauSec;

        var A = new Matrix(1, 1); A[0, 0] = a11;
        var B = new Matrix(1, 1); B[0, 0] = b11;
        var C = new Matrix(1, 1); C[0, 0] = 1.0;

        return new ProcessModel(name, tsSec, A, B, C);
    }

    /// <summary>Cria um modelo SISO de 2ª ordem discretizado.</summary>
    public static ProcessModel SecondOrder(double k, double tau1Sec, double tau2Sec, double tsSec, string name = "sopdt")
    {
        // (τ1 s + 1)(τ2 s + 1) y = K u  →  espaço de estados contínuo (forma canônica)
        // Discretização por Euler backward para simplicidade (precisão suficiente para design)
        var A = new Matrix(2, 2);
        A[0, 0] = 1.0; A[0, 1] = tsSec;
        var denom = tau1Sec * tau2Sec + (tau1Sec + tau2Sec) * tsSec + tsSec * tsSec;
        A[1, 0] = -tsSec * tsSec / denom;
        A[1, 1] = (tau1Sec * tau2Sec) / denom;

        var B = new Matrix(2, 1);
        B[0, 0] = k * tsSec * tsSec / denom;
        B[1, 0] = k * tsSec / denom;

        var C = new Matrix(1, 2);
        C[0, 0] = 1.0; C[0, 1] = 0.0;

        return new ProcessModel(name, tsSec, A, B, C);
    }
}
